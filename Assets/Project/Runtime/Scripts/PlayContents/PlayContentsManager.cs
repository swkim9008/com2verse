/*===============================================================
* Product:		Com2Verse
* File Name:	PlayContentsManager.cs
* Developer:	haminjeong
* Date:			2023-05-26 12:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Network;
using JetBrains.Annotations;

namespace Com2Verse.Contents
{
	public interface IPlayController
	{
		public PlayContentsManager.eContentsType ContentsType { get; }
		public void                              Initialize();
		public void                              OnUpdateLoop();
		public void                              SetPause(bool isPause);
		public void                              Clear();
	}
	
	public sealed class PlayContentsManager : MonoSingleton<PlayContentsManager>, IDisposable
	{
		public enum eContentsType
		{
			NONE = -1,
			MAZE = 0,
			BOARD_GAME,
		}

		private IPlayController _currentContents;
		public  IPlayController CurrentContents => _currentContents;
		private eContentsType   _currentContentsType = eContentsType.NONE;
		public  eContentsType   CurrentContentsType => _currentContentsType;

		private bool _isInitialize;

#region Initialize
		[UsedImplicitly]
		private PlayContentsManager()
		{
			_currentContents = null;
			_isInitialize    = false;
		}

		/// <summary>
		/// Manager를 초기화합니다.
		/// </summary>
		public void Initialize()
		{
			if (_isInitialize)
				return;
			_isInitialize = true;
			
		}

		/// <summary>
		/// 컨텐츠 객체를 생성하고 초기화 합니다.
		/// </summary>
		/// <param name="type">생성할 컨텐츠 타입</param>
		public void InitializeContents(eContentsType type)
		{
			IPlayController newController = null;
			switch (type)
			{
				case eContentsType.MAZE: newController = new MazeController(); break;
				case eContentsType.BOARD_GAME:
					break;
			}
			_currentContents = newController;
			if (newController == null) return;
			newController.Initialize();
			_currentContentsType                   =  newController.ContentsType;
			Commander.Instance.OnEscapeEvent       += ContentsEnd;
			NetworkManager.Instance.OnDisconnected += ContentsEnd;
		}

		public void ContentsEnd()
		{
			Commander.Instance.OnEscapeEvent       -= ContentsEnd;
			NetworkManager.Instance.OnDisconnected -= ContentsEnd;
			if (_currentContentsType == eContentsType.NONE) return;
			_currentContents?.Clear();
			_currentContentsType = eContentsType.NONE;
		}

		public void Dispose()
		{
			_currentContents?.Clear();
			_currentContents = null;
		}
#endregion

		private void Update()
		{
			if (_currentContentsType == eContentsType.NONE) return;
			_currentContents?.OnUpdateLoop();
		}
	}
}
