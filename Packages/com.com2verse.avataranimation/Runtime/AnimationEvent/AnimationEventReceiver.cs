/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationEventReceiver.cs
* Developer:	eugene9721
* Date:			2023-02-10 15:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;
using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine.Pool;

namespace Com2Verse.AvatarAnimation
{
	public sealed class AnimationEventReceiver : IDisposable
	{
		private const float EndOffsetTime = 0.001f;

#region InternalClass
		[Serializable]
		public class AnimationEventAttr
		{
			[field: SerializeField] public float           EventTime     { get; set; }
			[field: SerializeField] public float           CurrentTime   { get; set; }
			[field: SerializeField] public AnimationClip?  AnimationClip { get; set; }
			[field: SerializeField] public eAnimationEvent EventType     { get; set; }
			[field: SerializeField] public string          StringParam   { get; set; } = string.Empty;
			[field: SerializeField] public int             IntParam      { get; set; }
			[field: SerializeField] public bool            IsPlaying     { get; set; }

			/// <summary>
			/// 이벤트가 할당되고 1번 이상 실행되었는지 여부 체크
			/// </summary>
			[field: SerializeField] public bool IsInvoked { get; set; }

			public void Set(float eventTime, float startTime, AnimationClip animationClip, eAnimationEvent eventType, string stringParam, int intParam)
			{
				EventTime     = eventTime;
				CurrentTime   = startTime;
				AnimationClip = animationClip;
				EventType     = eventType;
				StringParam   = stringParam;
				IntParam      = intParam;
				IsPlaying     = false;
				IsInvoked     = false;
			}
		}
#endregion InternalClass

#region Fields
		private Action<eAnimationEvent, string, int, bool>? _onAnimationEvent;

		private readonly List<AnimationEventAttr>        _eventList  = new();
		private readonly IObjectPool<AnimationEventAttr> _eventPools = new ObjectPool<AnimationEventAttr>(() => new AnimationEventAttr());

		private float _timeScale = 1f;
		private bool  _isPaused;
#endregion Fields

#region Initalize
		/// <summary>
		/// 해당 클래스의 애니메이션 이벤트를 주입
		/// </summary>
		public void Initialize(IAnimationEventCommand animationEventCommand)
		{
			_onAnimationEvent = animationEventCommand.OnAnimationEvent;
		}

		public void Dispose()
		{
			_eventPools.Clear();
			_eventList.Clear();
			_onAnimationEvent = null;
		}
#endregion Initalize

#region Public Methods
		public void OnUpdate(float deltaTime)
		{
			if (_isPaused) return;

			foreach (var gameEvent in _eventList)
			{
				if (gameEvent == null) continue;

				var clip = gameEvent.AnimationClip;
				if (!clip.IsReferenceNull() && clip!.isLooping)
				{
					var newCurrTime = gameEvent.CurrentTime - clip.length - EndOffsetTime;
					gameEvent.CurrentTime = newCurrTime;
					gameEvent.IsPlaying   = false;
				}

				gameEvent.CurrentTime += deltaTime * _timeScale;
				if (gameEvent.CurrentTime < gameEvent.EventTime) continue;
				if (gameEvent.IsPlaying) continue;
				if (_onAnimationEvent == null) continue;
				_onAnimationEvent.Invoke(gameEvent.EventType, gameEvent.StringParam, gameEvent.IntParam, gameEvent.IsInvoked);
				gameEvent.IsInvoked = true;
				gameEvent.IsPlaying = true;
			}
		}

		/// <summary>
		/// 애니메이터 변경시 기존 애니메이터의 이벤트를 모두 제거
		/// </summary>
		public void ClearEventList()
		{
			foreach (var animationEvent in _eventList)
				_eventPools.Release(animationEvent);
			_eventList.Clear();
		}

		public void AddEvent(float eventTime, float startTime, AnimationClip animationClip, eAnimationEvent eventType, string stringParam, int intParam)
		{
			var gameEvent = _eventPools.Get();
			gameEvent!.Set(eventTime, startTime, animationClip, eventType, stringParam, intParam);
			_eventList.Add(gameEvent);
		}

		public void PauseEvent()
		{
			_isPaused = true;
		}
#endregion Public Methods
	}
}
