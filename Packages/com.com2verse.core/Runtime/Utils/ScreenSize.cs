/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenSize.cs
* Developer:	urun4m0r1
* Date:			2022-07-21 09:51
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Utils
{
	public sealed class ScreenSize : Singleton<ScreenSize>
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private ScreenSize()
		{
			UpdateScreenSize();

			Application.quitting -= Dispose;
			Application.quitting += Dispose;

			_tokenSource ??= new CancellationTokenSource();
		}

#region Base value of platform
		public static readonly Vector2Int DefaultSize = new(1920, 1080);

		public enum eBaseAxis
		{
			WIDTH,
			HEIGHT,
		}

		public static eBaseAxis BaseAxis { get; set; } = eBaseAxis.HEIGHT;
#endregion // Base value of platform

		private Action<int, int>? _screenResized;

		public event Action<int, int>? ScreenResized
		{
			add
			{
				_screenResized -= value;
				_screenResized += value;
			}
			remove => _screenResized -= value;
		}

		public int Width  { get; private set; }
		public int Height { get; private set; }

		private CancellationTokenSource? _tokenSource;

		public void Initialize()
		{
			CheckScreenSizeChanged().Forget();
		}

		private async UniTask CheckScreenSizeChanged()
		{
			while (_tokenSource is { IsCancellationRequested: false })
			{
				if (!Application.isPlaying)
					return;

				await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _tokenSource.Token);

				UpdateScreenSize();
			}
		}

		private void UpdateScreenSize()
		{
			var size = GetScreenSize();
			NotifyScreenSizeChanged(size.x, size.y);
		}

		private Vector2Int GetScreenSize()
		{
#if UNITY_EDITOR
			var gameViewSize = GetMainGameViewSize();
			var width        = gameViewSize.x;
			var height       = gameViewSize.y;
#else
			var width = Screen.width;
			var height = Screen.height;
#endif // UNITY_EDITOR

			return new(width, height);
		}

#if UNITY_EDITOR
		private MethodInfo? _methodGetSizeOfMainGameView;

		public Vector2Int GetMainGameViewSize()
		{
			if (_methodGetSizeOfMainGameView == null)
			{
				var gameViewTypeInfo = Type.GetType("UnityEditor.GameView,UnityEditor");
				_methodGetSizeOfMainGameView = gameViewTypeInfo?.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
			}

			var size = (Vector2?)_methodGetSizeOfMainGameView?.Invoke(null!, null!);
			return size == null ? DefaultSize : new Vector2Int((int)size.Value.x, (int)size.Value.y);
		}
#endif // UNITY_EDITOR

		private void NotifyScreenSizeChanged(int width, int height)
		{
			var isChanged = Width != width || Height != height;
			if (isChanged)
			{
				Width  = width;
				Height = height;
				_screenResized?.Invoke(Width, Height);
			}
		}

		private void Dispose()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;

			Application.quitting -= Dispose;
		}
	}
}
