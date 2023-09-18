/*===============================================================
* Product:		Com2Verse
* File Name:	TweenAnimationHolder.cs
* Developer:	hyj
* Date:			2022-04-28 14:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.Tweener
{
	public abstract class TweenController : MonoBehaviour
	{
#region Command Handler Target
		[HideInInspector] public UnityEvent<Transform>? OnTweenedImmediately;
		[HideInInspector] public UnityEvent<Transform>? OnRestoredImmediately;

		[HideInInspector] public UnityEvent<Transform>? OnTweened;
		[HideInInspector] public UnityEvent<Transform>? OnRestored;
#endregion Command Handler Target
		[UsedImplicitly] public Transform TransformProperty { get; set; }

		[SerializeField] private TweenStyle _tweenStyle = TweenStyle.Default;

		public event Action? TweenedImmediately;
		public event Action? RestoredImmediately;

		public event Action<TweenStyle>? Tweened;
		public event Action<TweenStyle>? Restored;
		public event Action<TweenStyle>? LoopStarted;
		public event Action<TweenStyle>? LoopStopped;
		public event Action<TweenStyle>? TweenedOnce;

		private void Awake()
		{
			TransformProperty = transform;
		}

		protected void TweenImmediately()
		{
			OnTweenedImmediately?.Invoke(TransformProperty);
			TweenedImmediately?.Invoke();
		}

		protected void RestoreImmediately()
		{
			OnRestoredImmediately?.Invoke(TransformProperty);
			RestoredImmediately?.Invoke();
		}

		protected void Tween()
		{
			OnTweened?.Invoke(TransformProperty);
			Tweened?.Invoke(_tweenStyle);
		}

		protected void Restore()
		{
			OnRestored?.Invoke(TransformProperty);
			Restored?.Invoke(_tweenStyle);
		}

		protected void StartLoop()
		{
			LoopStarted?.Invoke(_tweenStyle);
		}

		protected void StopLoop()
		{
			LoopStopped?.Invoke(_tweenStyle);
		}

		protected void TweenOnce()
		{
			TweenedOnce?.Invoke(_tweenStyle);
		}
	}
}
