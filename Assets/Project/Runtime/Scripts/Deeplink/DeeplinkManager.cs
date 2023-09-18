/*===============================================================
* Product:		Com2Verse
* File Name:	DeeplinkManager.cs
* Developer:	mikeyid77
* Date:			2023-08-16 18:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.Data;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Deeplink
{
	public static class DeeplinkManager
	{
		private static readonly Dictionary<eServiceType, DeeplinkBaseController> ControllerList = new();

		public  static bool InfoStandby  => _infoStandby;
		private static bool _infoStandby = false;
		private static bool _userStandby = false;

		public static void Initialize()
		{
			C2VDebug.LogCategory("Deeplink", $"Initialize Manager");
			User.Instance.OnTeleportCompletion += ReadyToInvoke;
			_infoStandby = false;
			_userStandby = false;

			var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(ServiceAttribute)));
			foreach (var type in types)
			{
				DeeplinkBaseController targetController = Activator.CreateInstance(type) as DeeplinkBaseController;
				targetController?.Initialize();
				ControllerList?.TryAdd(type.GetCustomAttribute<ServiceAttribute>().ServiceType, targetController);
				C2VDebug.LogCategory("Deeplink", $"Add Controller: {targetController?.Name} (ServiceType : {type.GetCustomAttribute<ServiceAttribute>()?.ServiceType})");
			}
		}

		public static void InvokeEngagement(EngagementInfo info)
		{
			if (info == null)
			{
				C2VDebug.LogErrorCategory("Deeplink", $"EngagementInfo is NULL");
			}
			else if (_infoStandby)
			{
				C2VDebug.LogWarningCategory("Deeplink", "Already Set Engagement");
			}
			else if (SceneManager.Instance.CurrentScene.SceneProperty?.SpaceTemplate?.SpaceCode == eSpaceCode.MEETING)
			{
				C2VDebug.LogWarningCategory("Deeplink", $"Cancel Invoke");
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingApp_Popup_Fail_Desc_Enter"), 3f, UIManager.eToastMessageType.WARNING);
			}
			else
			{
				_infoStandby = true;
				InvokeEngagementAsync(info).Forget();
			}
		}

		private static async UniTask InvokeEngagementAsync(EngagementInfo info)
		{
			while (!CheckBeforeInvoke())
			{
				await UniTask.Yield();
			}
			await UniTask.Yield();

			C2VDebug.LogCategory("Deeplink", $"Start Invoke Action");
			_userStandby = false;
			ControllerList?[info.ServiceType]?.Invoke(info, () =>
			{
				C2VDebug.LogCategory("Deeplink", $"Finish Invoke Action");
				_userStandby = true;
				_infoStandby = false;
			});
		}

#region Utils
		private static bool CheckBeforeInvoke()
		{
			// Hive SDK 초기화 중 Invoke 방지
			if (!LoginManager.Instance.IsInitialized)
				return false;

			// Loading UI 출력 중 Invoke 방지
			if (LoadingManager.Instance.IsLoading)
				return false;

			// 공간 Scene이 아닌 경우 Invoke 방지
			if (SceneManager.Instance.CurrentScene is not SceneSpace)
				return false;

			// 유저가 준비되지 않은 경우 Invoke 방지
			if (!_userStandby)
				return false;

			return true;
		}

		private static void ReadyToInvoke()
		{
			User.Instance.OnTeleportCompletion -= ReadyToInvoke;
			_userStandby = true;
		}

		public static void ResetComponent()
		{
			C2VDebug.LogCategory("Deeplink", $"Reset Manager");
			User.Instance.OnTeleportCompletion += ReadyToInvoke;
			_infoStandby = false;
			_userStandby = false;
		}
#endregion
	}
}
