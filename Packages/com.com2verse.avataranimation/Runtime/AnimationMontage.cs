/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationMontage.cs
* Developer:	eugene9721
* Date:			2023-04-07 15:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Cysharp.Threading.Tasks;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Com2Verse.AvatarAnimation
{
	public sealed class AnimationMontage
	{
		private const int SourceOutputPort  = 0;
		private const int AnimatorInputPort = 0;
		private const int ClipInputPort     = 1;

		private bool _isInitialized;

		private PlayableGraph               _playableGraph;
		private AnimationLayerMixerPlayable _mixerPlayable;
		private AnimationClipPlayable       _clipPlayable;
		private AnimationPlayableOutput     _playableOutput;

		private AvatarMask? _fullBodyMask;
		private AvatarMask? _upperBodyMask;

		public void Initialize(string graphName, eAvatarType type, Animator animator)
		{
			OnRelease();
			if (animator.runtimeAnimatorController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Animator Controller is null");
				return;
			}

			_fullBodyMask  = AnimationManager.Instance.GetCinematicAvatarMask(type);
			// TODO: UpperBodyMask Cinematic마스크로 변경 필요할듯
			_upperBodyMask = AnimationManager.Instance.GetUpperBodyAvatarMask;

			if (_fullBodyMask.IsReferenceNull() || _upperBodyMask.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Avatar Mask is null");
				return;
			}

			_playableGraph  = PlayableGraph.Create($"{graphName}_AnimationMontage");
			_playableOutput = AnimationPlayableOutput.Create(_playableGraph, AnimationDefine.AnimationKeyword, animator);

			_mixerPlayable = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
			_playableOutput.SetSourcePlayable(_mixerPlayable);

			var controllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, animator.runtimeAnimatorController);
			_mixerPlayable.ConnectInput(AnimatorInputPort, controllerPlayable, SourceOutputPort, 1f);
			// StateMachineBehaviour 중첩 버그 방지
			animator.Rebind();

			_playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
			_playableGraph.Play();

			_isInitialized = true;
		}

		public void OnRelease()
		{
			if (!_isInitialized) return;

			_playableGraph.Destroy();
			_isInitialized = false;
		}

		/// <summary>
		/// 애니메이터와 Playable Clip을 블렌딩하여 재생
		/// 현재 애니메이터와 Playable Clip간 Layer를 이용한 블랜딩시 블랜딩이 제대로 되지 않는 문제가 있어
		/// 특정 레이어를 사용하는 경우 Playable의 IK를 활성화해서 사용해야함
		/// <a href="https://forum.unity.com/threads/playables-animations.732578/">해당 문서 참조</a>
		/// </summary>
		/// <param name="addressableName">재생할 클립의 어드레서블 네임</param>
		/// <param name="tokenSource">클립 재생을 중지시킬때 사용할 토큰</param>
		/// <param name="isFullBody">fullBody Layer인지 여부, 아닐경우 upperBody레이어를 통해 클립 실행</param>
		/// <param name="applyFootIk">FootIk 사용 여부</param>
		/// <param name="applyPlayableIk">PlayableIk 사용 여부</param>
		/// <param name="transitionDuration">종료후 애니메이터의 애니메이션으로 전환되는 시간</param>
		/// <returns></returns>
		public async UniTask<bool> PlayClip(string addressableName, CancellationTokenSource tokenSource, bool isFullBody = true, bool applyFootIk = true, bool applyPlayableIk = true, float transitionDuration = 0.25f)
		{
			if (!_isInitialized) return true;

			// var clipHandle = C2VAddressables.LoadAssetAsync<AnimationClip>($"{addressableName}_anim.anim");
			// if (clipHandle == null)
			// {
			// 	C2VDebug.LogErrorCategory(GetType().Name, "clipHandle is null!");
			// 	return true;
			// }

			//var loadedAsset = await clipHandle.ToUniTask();
			var loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<AnimationClip>($"{addressableName}_anim.anim", tokenSource);
			if (loadedAsset.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AnimationClip is null!");
				return true;
			}

			return await PlayClip(loadedAsset, tokenSource, isFullBody, applyFootIk, applyPlayableIk, transitionDuration);
		}

		/// <summary>
		/// 애니메이터와 Playable Clip을 블렌딩하여 재생
		/// 현재 애니메이터와 Playable Clip간 Layer를 이용한 블랜딩시 블랜딩이 제대로 되지 않는 문제가 있어
		/// 특정 레이어를 사용하는 경우 Playable의 IK를 활성화해서 사용해야함
		/// <a href="https://forum.unity.com/threads/playables-animations.732578/">해당 문서 참조</a>
		/// </summary>
		/// <param name="animationClip">재생할 클립의 애니메이션 클립</param>
		/// <param name="tokenSource">클립 재생을 중지시킬때 사용할 토큰</param>
		/// <param name="isFullBody">fullBody Layer인지 여부, 아닐경우 upperBody레이어를 통해 클립 실행</param>
		/// <param name="applyFootIk">FootIk 사용 여부</param>
		/// <param name="applyPlayableIk">PlayableIk 사용 여부</param>
		/// <param name="transitionDuration">종료후 애니메이터의 애니메이션으로 전환되는 시간</param>
		/// <returns></returns>
		public async UniTask<bool> PlayClip(AnimationClip animationClip, CancellationTokenSource tokenSource, bool isFullBody = true, bool applyFootIk = true, bool applyPlayableIk = true, float transitionDuration = 0.25f)
		{
			// TODO: 블링크 설정
			if (!_isInitialized) return true;

			if (!_clipPlayable.IsNull())
				_mixerPlayable.DisconnectInput(ClipInputPort);

			_clipPlayable = AnimationClipPlayable.Create(_playableGraph, animationClip);
			var currentClipPlayable = _clipPlayable;
			_clipPlayable.SetApplyFootIK(applyFootIk);
			_clipPlayable.SetApplyPlayableIK(applyPlayableIk);
			_mixerPlayable.SetLayerMaskFromAvatarMask(1, isFullBody ? _fullBodyMask! : _upperBodyMask!);
			_mixerPlayable.ConnectInput(ClipInputPort, _clipPlayable, SourceOutputPort, 1);
			_mixerPlayable.SetInputWeight(ClipInputPort, 1);

			_playableGraph.Play();

			bool isCanceled;
			if (_clipPlayable.GetAnimationClip()?.isLooping ?? false)
			{
				await UniTask.WaitUntil(() => tokenSource.IsCancellationRequested).SuppressCancellationThrow();
				isCanceled = true;
			}
			else
			{
				var time   = TimeSpan.FromSeconds(_clipPlayable.GetAnimationClip()!.length);
				isCanceled = await UniTask.Delay(time, cancellationToken: tokenSource.Token).SuppressCancellationThrow();
			}

			float timer = 0;
			while (!isCanceled && timer <= transitionDuration)
			{
				if (!_isInitialized) return true;

				if (_clipPlayable.Equals(currentClipPlayable))
					_mixerPlayable.SetInputWeight(ClipInputPort, 1 - timer / transitionDuration);
				else break;

				timer      += Time.deltaTime;
				isCanceled =  await UniTask.Yield(cancellationToken: tokenSource.Token).SuppressCancellationThrow();
			}

			if (!_isInitialized) return true;
			if (_clipPlayable.Equals(currentClipPlayable))
			{
				_mixerPlayable.SetInputWeight(ClipInputPort, 0);
				_mixerPlayable.DisconnectInput(ClipInputPort);
			}
			return isCanceled;
		}
	}
}
