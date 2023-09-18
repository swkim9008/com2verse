/*===============================================================
* Product:		Com2Verse
* File Name:	TextureTestEditor.cs
* Developer:	jhkim
* Date:			2023-03-24 20:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Com2Verse.Utils
{
	public sealed class TextureTestEditor : EditorWindow
	{
		private string _path;
		private Texture _texture;
		private long _elapsedMs = 0;

		private readonly ITextureLoader[] _textureLoaders = new ITextureLoader[]
		{
			new WWWTextureLoader(),
			new UnityWebRequestTextureLoader(),
		};

		[MenuItem("Com2Verse/Test/텍스쳐 로드 테스트")]
		private static void ShowWindow()
		{
			GetWindow<TextureTestEditor>("텍스쳐 로드 테스트").Show();
		}

		private void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				_path = EditorGUILayout.TextField(_path);
				if (GUILayout.Button("파일 선택", GUILayout.Width(100)))
				{
					var selected = SelectFile();
					_path = selected;
				}

				if (GUILayout.Button("텍스쳐 로드", GUILayout.Width(100)))
				{
					if (!string.IsNullOrWhiteSpace(_path))
					{
						foreach (var textureLoader in _textureLoaders)
							textureLoader.LoadAsync(_path).Forget();
					}
				}

				if (GUILayout.Button("Clear", GUILayout.Width(50)))
				{
					foreach (var textureLoader in _textureLoaders)
						textureLoader.ClearTexture();
				}
			}

			foreach (var textureLoader in _textureLoaders)
			{
				if (GUILayout.Button($"Load {textureLoader.GetName()}"))
				{
					if (!string.IsNullOrWhiteSpace(_path))
						textureLoader.LoadAsync(_path);
				}
			}

			var rect = GUILayoutUtility.GetLastRect();
			rect.width = 300;
			rect.height = 300;
			rect.y += 20 * _textureLoaders.Length;

			foreach (var textureLoader in _textureLoaders)
			{
				DrawTextureInfo(rect, textureLoader);
				rect.y += rect.height;
			}
		}

		string SelectFile()
		{
			var selected = EditorUtility.OpenFilePanel("파일 선택", "C:/Users/user/AppData/Local/Temp/Com2Verse/Com2Verse/DEFAULT/", "");
			if (!(selected.EndsWith(".png") || selected.EndsWith(".jpg"))) return string.Empty;

			return selected;
		}

		void DrawTextureInfo(Rect rect, [NotNull] ITextureLoader textureLoader)
		{
			if (!textureLoader.HasTexture()) return;

			var texture = textureLoader.GetTexture();
			EditorGUI.DrawPreviewTexture(rect, texture);

			rect.x += 320;
			rect.y += 30;
			rect.width = 400;
			rect.height = 400;
			using (new GUILayout.AreaScope(rect))
			{
				EditorGUILayout.LabelField($"Loader : {textureLoader.GetName()}");
				EditorGUILayout.LabelField($"Width : {texture.width}");
				EditorGUILayout.LabelField($"Height : {texture.height}");
				EditorGUILayout.LabelField($"Texture Dimension : {texture.dimension}");
				EditorGUILayout.LabelField($"Aniso Level : {texture.anisoLevel}");
				EditorGUILayout.LabelField($"Filter Mode : {texture.filterMode}");
				EditorGUILayout.LabelField($"Graphic Mode : {texture.graphicsFormat}");
				EditorGUILayout.LabelField($"Texel Size : {texture.texelSize.x}, {texture.texelSize.y}");
				EditorGUILayout.LabelField($"Contents Hash : {texture.imageContentsHash}");
				EditorGUILayout.LabelField($"Elapsed Time : {textureLoader.GetElapsedTimeMs()}");
			}
		}

		private interface ITextureLoader
		{
			string GetName();
			UniTask<Texture2D> LoadAsync(string filePath);
			long GetElapsedTimeMs();
			bool HasTexture();
			Texture2D GetTexture();
			void ClearTexture();
		}
		private sealed class Profiler
		{
			[NotNull] private readonly Stopwatch _sw;
			public long ElapsedTimeMs => _sw.ElapsedMilliseconds;

			private Profiler() => _sw = new Stopwatch();
			[NotNull] public static Profiler CreateNew() => new();

			public void Start() => _sw.Restart();
			public long Stop()
			{
				_sw.Stop();
				return _sw.ElapsedMilliseconds;
			}
		}

		private abstract class BaseTextureLoader : ITextureLoader
		{
			[NotNull] protected readonly Profiler Profiler = Profiler.CreateNew();
			protected Texture2D Texture;
			public virtual string GetName() => throw new System.NotImplementedException();

			public virtual UniTask<Texture2D> LoadAsync(string filePath) => throw new System.NotImplementedException();

			public virtual long GetElapsedTimeMs() => throw new System.NotImplementedException();

			public bool HasTexture() => Texture != null;

			public Texture2D GetTexture() => Texture;

			public void ClearTexture() => Texture = null;
		}
		private class WWWTextureLoader : BaseTextureLoader
		{
			public override string GetName() => "WWW Texture Loader";
			public override async UniTask<Texture2D> LoadAsync(string filePath)
			{
				if (File.Exists(filePath))
				{
					Profiler.Start();
					var textureSize = TextureUtil.GetTextureSize(filePath);
					if (textureSize != (-1, -1))
					{
						var path = $"file://{filePath}";
						var www = new WWW(path);
						var tex = new Texture2D(textureSize.Item1, textureSize.Item2, TextureFormat.RGBA32, false);
						await UniTask.WaitUntil(() => www.isDone);
						www.LoadImageIntoTexture(tex);
						Texture = tex;
					}
				}
				Profiler.Stop();
				return Texture;
			}

			public override long GetElapsedTimeMs() => Profiler.ElapsedTimeMs;
		}

		private class UnityWebRequestTextureLoader : BaseTextureLoader
		{
			public override string GetName() => "UnityWebRequest Texture Loader";
			public override async UniTask<Texture2D> LoadAsync(string filePath)
			{
				if (File.Exists(filePath))
				{
					Profiler.Start();
					using var request = UnityWebRequestTexture.GetTexture(filePath);
					await request.SendWebRequest();

					Texture2D tex = null;
					if (request.result == UnityWebRequest.Result.Success)
						tex = DownloadHandlerTexture.GetContent(request);

					Texture = tex;
				}

				Profiler.Stop();
				return Texture;
			}

			public override long GetElapsedTimeMs() => Profiler.ElapsedTimeMs;
		}
	}
}
#endif // UNITY_EDITOR