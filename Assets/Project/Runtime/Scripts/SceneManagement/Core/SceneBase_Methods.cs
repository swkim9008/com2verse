/*===============================================================
* Product:		Com2Verse
* File Name:	SceneBase_Methods.cs
* Developer:	urun4m0r1
* Date:			2022-10-28 13:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Notification;
using Com2Verse.Organization;
using Com2Verse.Pathfinder;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using User = Com2Verse.Network.User;

namespace Com2Verse
{
	public partial class SceneBase
	{
		protected async UniTask WaitForUserLoginAsync()
		{
			if (IsDebug) return;

			await UniTask.WaitUntil(static () => User.InstanceExists && User.Instance.LoginComplete);
		}

		protected async UniTask WaitForUserStandbyAsync()
		{
			if (IsDebug) return;

			User.Instance.ReadyToReceive();
			while (User.InstanceExists && !User.Instance.Standby)
			{
				if (!NetworkManager.Instance.IsConnected) break;
				await UniTask.Yield();
			}
		}

		protected async UniTask WaitForCreateUserCharacter()
		{
			var myCharacter = User.Instance.CharacterObject;
			if (myCharacter.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "캐릭터가 생성되지 않았습니다.");
				return;
			}

			var avatarController = myCharacter!.AvatarController;
			if (!avatarController.IsUnityNull())
				await UniTask.WaitUntil(() => avatarController!.IsCompletedLoadAvatarParts);
		}

		protected async UniTask LoadSystemViewListAsync()
		{
			await UIManager.Instance.LoadSystemViewListAsync();
		}

		protected async UniTask LoadUICanvasRootAsync()
		{
			await UIManager.Instance.LoadUICanvasRootAsync($"2DCanvasRoot_{GetType().Name}", UserFunctionUI.SetGUIView);

			var sceneOverlaySettings = await Resources.LoadAsync<SceneOverlaySettings>(SceneManagement.Define.SceneOverlaySettingsResourcesPath)!.ToUniTask() as SceneOverlaySettings;
			foreach (var overlayDefine in sceneOverlaySettings!.OverlayDefines)
			{
				if (!overlayDefine.IsPropertyMatch(SceneProperty))
					continue;

				foreach (var overlayAddressableName in overlayDefine.OverlayAddressableNames)
				{
					await UIManager.Instance.LoadUICanvasRootAsync(overlayAddressableName, UserFunctionUI.SetGUIView);
				}
			}
		}

		protected async UniTask LoadNavmeshAsync()
		{
			C2VAsyncOperationHandle<TextAsset>? handle;

			switch (SceneProperty.ServiceType)
			{
				case eServiceType.WORLD:
					handle = C2VAddressables.LoadAssetAsync<TextAsset>(ZString.Format(ClientPathFinding.NavmeshNameTemplate, 1));
					break;
				default:
					handle = C2VAddressables.LoadAssetAsync<TextAsset>(ZString.Format(ClientPathFinding.NavmeshNameTemplate, SpaceTemplate?.ID));
					break;
			}

			if (handle == null)
			{
				C2VDebug.LogError("NavMesh를 찾지 못했습니다.");
				return;
			}

			var navMeshData = await handle.ToUniTask();
			ClientPathFinding.Instance.LoadNavGraph(navMeshData);

			handle.Release();
		}

		protected async UniTask LoadOrganizationDataAsync()
		{
			if (IsDebug) return;
			if (DataManager.Instance.GroupID <= 0) return;

			var isComplete = false;
			if (!DataManager.Instance.IsReady)
			{
				DataManager.SendOrganizationChartRequest(DataManager.Instance.GroupID, result => isComplete = result);
			}
			else
			{
				isComplete = true;
			}

			await UniTask.WaitUntil(() => isComplete);
		}

		protected async UniTask InitializeNotificationManagerAsync()
		{
			NotificationManager.Instance.Initialize();
			await UniTask.CompletedTask;
		}
	}
}
