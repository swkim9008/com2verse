/*===============================================================
* Product:		Com2Verse
* File Name:	SceneLeaveWork.cs
* Developer:	eugene9721
* Date:			2022-12-07 19:25
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.CameraSystem;
using Com2Verse.Director;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse
{
	/// <summary>
	/// 로그아웃 연출이 필요한 경우,
	/// 서버와의 연결을 종료한 후 로그아웃 연출을 수행하는 씬
	/// </summary>
	public sealed class SceneLeave : SceneBase
	{
		protected override eMainCameraType MainCameraType => eMainCameraType.BACKGROUND;

		protected override void OnExitScene(SceneBase nextScene) { }

		protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks) { }

		protected override void OnLoadingCompleted()
		{
			NetworkManager.Instance.Disconnect(true);
			PlayLeaveDirector().Forget();
		}

		private async UniTask PlayLeaveDirector()
		{
			var loadingPage = UIManager.Instance.GetSystemView(eSystemViewType.UI_LOADING_PAGE);
			await UniTask.WaitUntil(() => loadingPage.VisibleState == GUIView.eVisibleState.CLOSING);

			var avatarInfo = User.Instance.AvatarInfo;
			var directorObject = GameObject.Find($"Director_{SceneName}");
			directorObject.SetActive(true);

			var director = directorObject.GetOrAddComponent<LeaveWorkDirector>();
			if (director.IsReferenceNull())
			{
				C2VDebug.LogError("loadedAsset is not LeaveWorkDirector");
				return;
			};

			director.Play(new LeaveWorkMessage(avatarInfo));
		}
	}
}
