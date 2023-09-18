/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationClipExtractorLegacy.cs
* Developer:	eugene9721
* Date:			2022-08-30 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Text;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.AvatarAnimation
{
	public sealed class AnimationClipExtractorLegacy : EditorWindow
	{
		private readonly Dictionary<(eAnimationClipType, AnimationClip), bool> _selectAnimationClips = new();

		private eAnimationClipType                           _currentType = eAnimationClipType.MOVEMENT;
		private HashSet<(eAnimationClipType, AnimationClip)> _animationClips;

		private bool    _preserveResponse = true;
		private Vector2 _scrollPosition;
		private string  _basePath;
		private string  _selectedFolder;

		public void Initialize(ICollection<(eAnimationClipType, AnimationClip)> animationClips, string basePath, string folderName)
		{
			_animationClips = animationClips.ToHashSet();
			_selectAnimationClips.Clear();
			foreach (var clip in _animationClips) _selectAnimationClips.Add(clip, false);

			_basePath       = basePath;
			_selectedFolder = folderName;
		}

		private Vector2 scrollPos;

		private void OnGUI()
		{
			GUILayout.TextField($"선택된 폴더: {_selectedFolder}");

			GUILayout.BeginHorizontal();
			foreach (eAnimationClipType animationClipType in Enum.GetValues(typeof(eAnimationClipType)))
				if (GUILayout.Button(animationClipType.ToString()))
					SetClipType(animationClipType);

			GUILayout.EndHorizontal();

			if (GUILayout.Button("선택된 타입 클립 전체 선택하기")) SetCurrentTypeClips(_currentType);
			if (GUILayout.Button("모든 클립 선택하기")) SetAllClips();

			if (_animationClips != null)
			{
				EditorGUILayout.BeginVertical();
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				foreach (var clip in _animationClips)
				{
					var type = clip.Item1;
					if (type != _currentType) continue;
					GUILayout.BeginHorizontal("box");
					_selectAnimationClips[clip] = GUILayout.Toggle(_selectAnimationClips[clip], $"{clip.Item1}, {clip.Item2.name}");
					GUILayout.EndHorizontal();
				}

				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
			}

			_preserveResponse = GUILayout.Toggle(_preserveResponse, "이벤트 데이터를 보존");

			if (GUILayout.Button("애니메이션 클립 추출하기")) ExtractClipAssets(_preserveResponse);
		}

		private void SetClipType(eAnimationClipType type)
		{
			_currentType = type;
		}

		private void SetAllClips()
		{
			foreach (eAnimationClipType type in Enum.GetValues(typeof(eAnimationClipType))) SetCurrentTypeClips(type);
		}

		private void SetCurrentTypeClips(eAnimationClipType clipType)
		{
			if (_animationClips == null) return;
			var keys = _selectAnimationClips.Keys.ToList();
			foreach (var clip in keys)
			{
				var type                                          = clip.Item1;
				if (type == clipType) _selectAnimationClips[clip] = true;
			}
		}

		private void ExtractClipAssets(bool preserveResponse)
		{
			var fbxFolderPath  = Path.Combine(_basePath, AnimatorBuilderDefine.AnimFBXFolderName);
			var clipFolderPath = Path.Combine(_basePath, AnimatorBuilderDefine.AnimClipFolderName);

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"FBX 파일에서 애니메이션을 추출합니다...\n이벤트 보존 체크: {preserveResponse.ToString()}\n----------");
			foreach (eAnimationClipType animationClipType in Enum.GetValues(typeof(eAnimationClipType)))
			{
				var clipTypeName       = animationClipType.ToString().ToLower();
				var fbxTypeFolderPath  = Path.Combine(fbxFolderPath, clipTypeName);
				var clipTypeFolderPath = Path.Combine(clipFolderPath, clipTypeName);
				if (!Directory.Exists(fbxTypeFolderPath)) continue;
				var fbxClipPath = Directory.GetFiles(fbxTypeFolderPath);

				foreach (var fileName in fbxClipPath)
				{
					List<AnimationClip> animationClips = new();
					AnimationEditorUtils.LoadAnimationClips(fileName, animationClips);
					foreach (var clip in animationClips)
					{
						if (!_selectAnimationClips.ContainsKey((animationClipType, clip))) continue;
						if (!_selectAnimationClips[(animationClipType, clip)]) continue;
						if (clip.name.StartsWith(AnimatorBuilderDefine.PreviewHeader)) continue;
						if (clip.name.Contains(AnimatorBuilderDefine.PreviewAnimation)) continue;

						var clipPath = $"{Path.Combine(clipTypeFolderPath, clip.name)}.anim";

						var prevClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
						if (prevClip == null) SaveAnimationClip(clip, clipPath, stringBuilder);
						else OverwriteAnimationClip(clip, prevClip, clipPath, clipFolderPath, preserveResponse, stringBuilder);
					}
				}
			}

			Debug.Log(stringBuilder.ToString());
			AssetDatabase.Refresh();
		}

		private void SaveAnimationClip(AnimationClip clip, string clipPath, Utf16ValueStringBuilder stringBuilder)
		{
			var temp = new AnimationClip();

			EditorUtility.CopySerialized(clip, temp);
			AssetDatabase.CreateAsset(temp, clipPath);
			AssetDatabase.SaveAssets();
			stringBuilder.AppendLine($"생성: {clipPath}");
		}

		private void OverwriteAnimationClip(AnimationClip clip, AnimationClip prevClip, string clipPath,
		                                    string clipTypeFolderPath, bool preserveResponse, Utf16ValueStringBuilder stringBuilder)
		{
			var temp = new AnimationClip();

			AssetDatabase.DisallowAutoRefresh();
			var tempDirectoryPath = Path.Combine(clipTypeFolderPath, AnimatorBuilderDefine.TempDirectoryName);
			Directory.CreateDirectory(tempDirectoryPath);

			AnimationEvent[] animationEvents      = null;
			if (preserveResponse) animationEvents = prevClip.events;

			var tempPath = $"{Path.Combine(tempDirectoryPath, $"{clip.name}")}.anim";
			EditorUtility.CopySerialized(clip, temp);
			if (preserveResponse && animationEvents != null) AnimationUtility.SetAnimationEvents(temp, animationEvents);

			AssetDatabase.CreateAsset(temp, tempPath);
			FileUtil.ReplaceFile(tempPath, clipPath);

			AssetDatabase.DeleteAsset(tempPath);
			AssetDatabase.DeleteAsset(tempDirectoryPath);

			AssetDatabase.SaveAssets();
			AssetDatabase.ImportAsset(clipPath);
			AssetDatabase.AllowAutoRefresh();
			stringBuilder.AppendLine($"대체: {clipPath}");
		}
	}
}
