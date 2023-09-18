/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarStateMachineIdle.cs
* Developer:	eugene9721
* Date:			2022-06-13 12:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Threading;
using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Project.Animation
{
	public sealed class AvatarStateMachineIdle : StateMachineBehaviour
	{
		private readonly float _standByWaitInterval = 5f; // 임시값

		private bool  _isInIdleState;
		private float _standByWaitTimer;

		private CancellationTokenSource _cancellationTokenSource;

		private AvatarAnimatorController _animatorController;

		private void Awake()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfo.shortNameHash != AnimationDefine.HashIdle) return;
			_standByWaitTimer = 0f;
			_isInIdleState    = true;

			StandByWaitState(animator).Forget();
		}

		// transition interruption 발생시 호출되지 않을 수 있음을 주의
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			_standByWaitTimer = 0f;
			_isInIdleState    = false;
		}

		private async UniTask StandByWaitState(Animator animator)
		{
			if (_cancellationTokenSource == null) return;
			while (_standByWaitTimer < _standByWaitInterval)
			{
				await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: _cancellationTokenSource.Token);
				if (animator.IsUnityNull()) return;

				if (IsOnEmotion(animator))
				{
					_standByWaitTimer = 0f;
					continue;
				}

				if (IsMoving(animator) || !_isInIdleState)
				{
					_standByWaitTimer = 0f;
					return;
				}

				_standByWaitTimer += Time.deltaTime;
			}

			animator.SetTrigger(AnimationDefine.HashSetWait);
		}

		private bool IsOnEmotion(Animator animator)
		{
			if (animator.IsUnityNull()) return false;
			if (_animatorController.IsUnityNull() && !animator.TryGetComponent(out _animatorController)) return false;
			return _animatorController!.IsGesturing || animator.GetBool(AnimationDefine.HashHandsUpIdle);
		}

		private bool IsMoving(Animator animator)
		{
			return AnimationHelper.IsJumpStart(animator) ||
			       Mathf.Abs(animator.GetFloat(AnimationDefine.HashSpeed)) > AnimationDefine.MoveThreshold;
		}

		private void OnDestroy()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}
	}
}
