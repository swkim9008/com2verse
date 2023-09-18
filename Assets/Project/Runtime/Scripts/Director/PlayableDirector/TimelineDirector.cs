/*===============================================================
* Product:		Com2Verse
* File Name:	TimelineDirector.cs
* Developer:	eugene9721
* Date:			2022-12-08 11:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Cinemachine;
using Com2Verse.AssetSystem;
using Com2Verse.AvatarAnimation;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Com2Verse.Director
{
	/// <summary>
	/// 타임라인을 사용한 연출을 관리하는 클래스
	/// PlayableAsset으로 Timeline을 이용
	/// TODO: 모바일 디바이스 빌드시 정상동작 확인 필요
	/// <a href="https://meta-bitbucket.com2us.com/projects/C2VERSE/repos/c2vclient/pull-requests/1401/overview">해당 문서 참조</a>
	/// </summary>
	public abstract class TimelineDirector : MetaverseDirector
	{
		// 메터리얼의 경우 프리팹에 저장이 안되므로 씬에 종속된 Director를 제작해서 사용할 것
		// https://forum.unity.com/threads/material-is-lost-when-saving-a-prefab-from-script.906902/

		private readonly Queue<AnimationClip>          _defaultClipList         = new();
		private readonly Queue<AnimationPlayableAsset> _animationPlayableAssets = new();

		protected TimelineAsset? TimelineAsset
		{
			get
			{
				if (PlayableAsset.IsUnityNull()) return null;
				return PlayableAsset as TimelineAsset;
			}
		}

		public virtual double GetDuration()
		{
			if (TimelineAsset.IsUnityNull()) return 0;

			return TimelineAsset!.duration;
		}

#region MonoBehaviour
		private void OnDestroy()
		{
			ResetAnimationClips();
			_animationPlayableAssets.Clear();
			_defaultClipList.Clear();

			if (!PlayableDirector.IsReferenceNull())
				PlayableDirector!.stopped -= OnPlayableDirectorStopped;
		}
#endregion MonoBehaviour

#region Timeline Track
		protected void SetObjectToTrack(Object targetObject, string trackName)
		{
			if (TimelineAsset.IsReferenceNull()) return;
			foreach (var binding in TimelineAsset!.outputs)
			{
				if (binding.streamName != trackName) continue;
				PlayableDirector!.SetGenericBinding(binding.sourceObject, targetObject);
			}
		}

		protected void SetMuteUnmuteTrack(string trackName, bool isMute)
		{
			if (TimelineAsset.IsReferenceNull())
			{
				C2VDebug.LogWarningCategory(GetType().Name, "TimelineAsset is null");
				return;
			}

			// TODO: group 체크 되는지 확인
			foreach (var trackAsset in TimelineAsset!.GetOutputTracks())
			{
				if (trackAsset.name != trackName) continue;
				trackAsset.muted = isMute;
			}
		}
#endregion Timeline Track

#region TimelineAnimationClip
		protected bool SetAvatarToTrack(GameObject avatarObject, string trackName)
		{
			bool isChangedAvatar = false;

			if (TimelineAsset.IsReferenceNull()) return false;

			foreach (var binding in TimelineAsset!.outputs)
			{
				if (binding.streamName != trackName) continue;

				var animator = avatarObject.GetComponent<Animator>();
				if (animator.IsReferenceNull()) continue;

				PlayableDirector!.SetGenericBinding(binding.sourceObject, animator);
				isChangedAvatar = true;
			}

			if (!isChangedAvatar) return false;

			avatarObject.SetActive(true);
			return true;
		}

		/// <summary>
		/// 타임라인 에셋은 여성을 기준으로 제작,
		/// 남성 아바타를 사용하는 경우를 대응하기 위해 제작
		/// </summary>
		protected async UniTask SetManAnimationClips()
		{
			var outputTracks = TimelineAsset!.GetOutputTracks();
			if (outputTracks == null) return;

			foreach (var trackAsset in outputTracks)
			{
				if (trackAsset is not AnimationTrack animationTrack) continue;
				var clips = animationTrack.GetClips();
				if (clips == null) continue;

				foreach (var timelineClip in clips)
				{
					if (timelineClip.asset is not AnimationPlayableAsset animationPlayableAsset) continue;
					if (animationPlayableAsset.clip.IsUnityNull()) continue;
					if (!AnimationDefine.AnimClipPattern.IsMatch(animationPlayableAsset.clip.name)) continue;

					_animationPlayableAssets.Enqueue(animationPlayableAsset);
					_defaultClipList.Enqueue(animationPlayableAsset.clip);

					var manClip = await LoadManClipAsync(timelineClip.animationClip.name);
					if (!manClip.IsUnityNull())
						animationPlayableAsset.clip = manClip;
				}
			}

			PlayableDirector!.stopped += OnPlayableDirectorStopped;
		}

		private async UniTask<AnimationClip> LoadManClipAsync(string clipName)
		{
			string manClipName = AnimationHelper.GetManClipNameFromWomanClip(clipName);
			//var clip = await C2VAddressables.LoadAssetAsync<AnimationClip>(manClipName).ToUniTask();
			return await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<AnimationClip>(manClipName);
		}

		private void ResetAnimationClips()
		{
			while (_animationPlayableAssets.Count > 0 && _defaultClipList.Count > 0)
			{
				var animationPlayableAsset = _animationPlayableAssets.Dequeue();
				animationPlayableAsset.clip = _defaultClipList.Dequeue();
			}

			_animationPlayableAssets.Clear();
			_defaultClipList.Clear();
		}

		private void OnPlayableDirectorStopped(PlayableDirector playableDirector)
		{
			if (!PlayableDirector.IsReferenceNull())
				PlayableDirector!.stopped -= OnPlayableDirectorStopped;

			ResetAnimationClips();
		}
#endregion TimelineAnimationClip

#region CinemachineTrack
		/// <summary>
		/// 타임라인 연출이 끝난 후 원래 사용할 카메라로 돌아가기 위한 기능입니다.
		/// </summary>
		/// <param name="clipName">원래 사용할 카메라를 담을 클립 이름</param>
		/// <param name="defaultCamera">연출이 끝나고 사용될 카메라</param>
		protected void SetDefaultCameraToClip(string clipName, CinemachineVirtualCamera defaultCamera)
		{
			if (TimelineAsset.IsReferenceNull()) return;

			var outputTracks = TimelineAsset!.GetOutputTracks();
			if (outputTracks == null) return;

			foreach (var trackAsset in outputTracks)
			{
				if (trackAsset is not CinemachineTrack cinemachineTrack) continue;
				var clips = cinemachineTrack.GetClips();
				if (clips == null) continue;

				foreach (var timelineClip in clips)
				{
					if (timelineClip.asset is not CinemachineShot cinemachineShot) continue;
					if (timelineClip.displayName != clipName) continue;
					cinemachineShot.VirtualCamera.defaultValue = defaultCamera;
				}
			}
		}
#endregion CinemachineTrack
	}
}
