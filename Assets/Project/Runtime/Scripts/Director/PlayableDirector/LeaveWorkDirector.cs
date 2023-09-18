/*===============================================================
* Product:		Com2Verse
* File Name:	LeaveWorkDirector.cs
* Developer:	eugene9721
* Date:			2022-12-07 12:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Director
{
	/// <summary>
	/// 퇴근 연출을 위한 디렉터
	/// </summary>
	public sealed class LeaveWorkDirector : TimelineDirector
	{
		[SerializeField] private string _avatarTrackName = "Avatar";

		[SerializeField] private Transform?  _avatarJig;
		[SerializeField] private GameObject? _sampleAvatar;

		public override bool Play(IDirectorMessage? message = null)
		{
			if (message is not LeaveWorkMessage leaveWorkMessage)
			{
				C2VDebug.LogErrorCategory(nameof(Director), "Message is not LeaveWorkMessage");
				return false;
			}

			if (TimelineAsset.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "TimelineAsset is null");
				return false;
			}

			if (_avatarJig.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "AvatarJig is null");
				return false;
			}

			if (_sampleAvatar.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "SampleAvatar is null");
				return false;
			}

			PlayAsync(leaveWorkMessage).Forget();

			return true;
		}

		private async UniTask PlayAsync(LeaveWorkMessage leaveWorkMessage)
		{
			var avatarInfo = leaveWorkMessage.AvatarInfo;

			if (avatarInfo.AvatarType == eAvatarType.PC01_M)
				await SetManAnimationClips();

			var avatarController = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.AVATAR_CUSTOMIZE, Vector3.zero, (int)Define.eLayer.CHARACTER);

			if (avatarController.IsUnityNull()) return;
			
			var avatarTransform = avatarController!.transform;
			if (SetAvatarToTrack(avatarController.gameObject, _avatarTrackName))
			{
				_sampleAvatar!.SetActive(false);

				avatarTransform.SetParent(_avatarJig);
				avatarTransform.localPosition = Vector3.zero;
				avatarTransform.localRotation = Quaternion.identity;
			}

			base.Play(leaveWorkMessage);
		}

		/// <summary>
		/// Signal Receiver(OnQuit)를 통하여 로딩창 마지막 타이밍에 실행
		/// </summary>
		[UsedImplicitly]
		public void OnQuit()
		{
			C2VDebug.Log("Application Quit");

#if UNITY_EDITOR

			UnityEditor.EditorApplication.ExitPlaymode();

#else
			Application.Quit();

#endif // UNITY_EDITOR
		}
	}
}
