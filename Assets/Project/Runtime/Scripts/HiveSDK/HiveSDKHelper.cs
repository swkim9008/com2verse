/*===============================================================
* Product:		Com2Verse
* File Name:	HiveSDKHelper.cs
* Developer:	mikeyid77
* Date:			2023-08-31 10:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.BuildHelper;
using Com2Verse.Deeplink;
using Com2Verse.Logger;
using Com2Verse.UI;
using hive;

namespace Com2Verse.Hive
{
	public sealed class HiveSDKHelper
	{
		private static AuthV4.PlayerInfo         _playerInfo    = null;
		private static List<AuthV4.ProviderType> _providerTypes = new();

		public static void Initialize(Action<bool, string> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Initialize Hive Plugin");
				HIVEUnityPlugin.InitPlugin();

				// var appId = AppInfo.Instance?.Data.AppId;
				// C2VDebug.LogCategory("HiveSDKHelper", $"Set AppId : {appId}");
				// if (string.IsNullOrEmpty(appId))
				// {
				// 	C2VDebug.LogErrorCategory("HiveSDKHelper", $"Hive Initialize AppId Failed");
				// 	ShowHiveErrorMessage("Initialize AppId Failed", true);
				// 	_playerInfo = null;
				// 	afterHelperCall?.Invoke(false, string.Empty);
				// }
				// else
				// {
				// 	Configuration.setAppId(appId);
				// }

				// var zone = AppInfo.Instance?.Data.HiveEnvType;
				// C2VDebug.LogCategory("HiveSDKHelper", $"Set Zone : {zone.ToString()}");
				// switch (zone)
				// {
				// 	case eHiveEnvType.SANDBOX:
				// 		Configuration.setZone(ZoneType.SANDBOX);
				// 		break;
				// 	case eHiveEnvType.LIVE:
				// 		Configuration.setZone(ZoneType.REAL);
				// 		break;
				// 	default:
				// 		C2VDebug.LogErrorCategory("HiveSDKHelper", $"Hive Initialize Zone Failed");
				// 		ShowHiveErrorMessage("Initialize Zone Failed", true);
				// 		_playerInfo = null;
				// 		afterHelperCall?.Invoke(false, string.Empty);
				// 		break;
				// }

				C2VDebug.LogCategory("HiveSDKHelper", $"Start Hive Setup");
				AuthV4.setup((result, isAutoSignIn, did, providerTypeList) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Hive Setup Success");
						C2VDebug.LogCategory("HiveSDKHelper", $"Add EngagementListener Callback");
						Promotion.setEngagementListener(DeeplinkParser.OnEngagementCB);
						_providerTypes = providerTypeList;
						_playerInfo    = null;
						CheckMaintenance((isComplete) => afterHelperCall?.Invoke(isComplete, did));
					}
					else
					{
						C2VDebug.LogErrorCategory("HiveSDKHelper", $"Hive Setup Failed");
						ShowHiveErrorMessage(result, true);
						_playerInfo = null;
						afterHelperCall?.Invoke(false, string.Empty);
					}
				});
			}
			else
			{
				_playerInfo = null;
				afterHelperCall?.Invoke(true, TempDid);
			}
		}

		public static void CheckMaintenance(Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Start Hive Maintenance Check");
				AuthV4.checkMaintenance(true, (result, maintenanceInfoList) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Hive Maintenance Check Success");
						afterHelperCall?.Invoke(maintenanceInfoList?.Count == 0);
					}
					else
					{
						C2VDebug.LogErrorCategory("HiveSDKHelper", $"Hive Maintenance Check Failed");
						ShowHiveErrorMessage(result);
						afterHelperCall?.Invoke(false);
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(true);
			}
		}

		public static void SignIn(Action<bool, AuthV4.PlayerInfo> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Start Hive SignIn");
				AuthV4.Helper.signIn((result, info) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Hive SignIn Success");
						_playerInfo = info;
						afterHelperCall?.Invoke(true, info);
					}
					else
					{
						switch (result.errorCode)
						{
							case ResultAPI.ErrorCode.CANCELED:
								C2VDebug.LogCategory("HiveSDKHelper", $"Hive SignIn Canceled");
								break;
							default:
								C2VDebug.LogErrorCategory("HiveSDKHelper", $"Hive SignIn Failed");
								ShowHiveErrorMessage(result);
								break;
						}
						_playerInfo = null;
						afterHelperCall?.Invoke(false, null);
					}
				});
			}
			else
			{
				_playerInfo = null;
				afterHelperCall?.Invoke(false, null);
			}
		}

		public static void SignOut(Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Start Hive SignOut");
				AuthV4.Helper.signOut((result, info) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Hive SignOut Success");
						_playerInfo = null;
						afterHelperCall?.Invoke(true);
					}
					else
					{
						C2VDebug.LogErrorCategory("HiveSDKHelper", $"Hive SignOut Failed");
						ShowHiveErrorMessage(result);
						_playerInfo = null;
						afterHelperCall?.Invoke(false);
					}
				});
			}
			else
			{
				_playerInfo = null;
				afterHelperCall?.Invoke(true);
			}
		}

# region IdP
		public static void SetIdp(AuthV4.ProviderType type, Action<bool, bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				if (_playerInfo == null)
				{
					afterHelperCall?.Invoke(false, false);
				}
				else
				{
					var typeExists    = _providerTypes?.Contains(type)                   ?? false;
					var typeConnected = _playerInfo?.providerInfoData?.ContainsKey(type) ?? false;
					afterHelperCall?.Invoke(typeExists, typeConnected);
				}
			}
			else
			{
				afterHelperCall?.Invoke(false, false);
			}
		}

		public static void ConnectIdp(AuthV4.ProviderType type, Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Start Connect {type.ToString()}");
				AuthV4.Helper.connect(type, (result, info) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Connect {type.ToString()} Success");
						_playerInfo = info;
						afterHelperCall?.Invoke(true);
					}
					else
					{
						switch (result.code)
						{
							case ResultAPI.Code.AuthV4CancelDialog:
								C2VDebug.LogCategory("HiveSDKHelper", $"Connect {type.ToString()} Canceled");
								afterHelperCall?.Invoke(false);
								break;
							case ResultAPI.Code.AuthV4ConflictPlayer:
								C2VDebug.LogWarningCategory("HiveSDKHelper", $"Need Resolve Conflict");
								UIManager.Instance.ShowPopupCommon(FailedLinkIdP);
								ResolveConflict();
								afterHelperCall?.Invoke(false);
								break;
							default:
								C2VDebug.LogErrorCategory("HiveSDKHelper", $"Connect {type.ToString()} Failed");
								ShowHiveErrorMessage(result);
								afterHelperCall?.Invoke(false);
								break;
						}
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(false);
			}
		}

		public static void DisconnectIdp(AuthV4.ProviderType type, Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Start Disconnect {type.ToString()}");
				AuthV4.Helper.disconnect(type, (result, info) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Disconnect {type.ToString()} Success");
						_playerInfo = AuthV4.getPlayerInfo();
						afterHelperCall?.Invoke(true);
					}
					else
					{
						switch (result.code)
						{
							case ResultAPI.Code.AuthV4CancelDialog:
								C2VDebug.LogCategory("HiveSDKHelper", $"Disconnect {type.ToString()} Canceled");
								afterHelperCall?.Invoke(false);
								break;
							case ResultAPI.Code.AuthV4LastProviderCantDisconnect:
								C2VDebug.LogWarningCategory("HiveSDKHelper", $"Last Provider Can't Disconnect");
								UIManager.Instance.ShowPopupCommon(FailedUnlinkIdP);
								afterHelperCall?.Invoke(false);
								break;
							default:
								C2VDebug.LogErrorCategory("HiveSDKHelper", $"Disconnect {type.ToString()} Failed");
								ShowHiveErrorMessage(result);
								afterHelperCall?.Invoke(false);
								break;
						}
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(false);
			}
		}

		private static void ResolveConflict()
		{
			if (CheckPlatform())
			{
				AuthV4.resolveConflict(result =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogWarningCategory("HiveSDKHelper", $"Resolve Conflict Success");
					}
					else
					{
						C2VDebug.LogErrorCategory("HiveSDKHelper", $"Resolve Conflict Failed");
						ShowHiveErrorMessage(result);
					}
				});
			}
		}
#endregion // IdP

#region CustomView
		public static void GetBannerInfo(Action<bool, List<PromotionBannerInfo>> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Get BannerInfo");
				Promotion.getBannerInfo(PromotionCampaignType.EVENT, PromotionBannerType.SMALL, (result, list) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Get BannerInfo Success");
						afterHelperCall?.Invoke(true, list);
					}
					else
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Get BannerInfo Failed");
						ShowHiveErrorMessage(result);
						afterHelperCall?.Invoke(false, null);
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(false, null);
			}
		}

		public static void ShowNews(Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Show News");
				Promotion.showPromotion(PromotionType.NEWS, true, (result, type) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Show News Success");
						afterHelperCall?.Invoke(true);
					}
					else
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Show News Failed");
						ShowHiveErrorMessage(result);
						afterHelperCall?.Invoke(false);
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(false);
			}
		}

		public static void ShowTerms(Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Show Terms");
				AuthV4.showTerms((result) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Show Terms Success");
						afterHelperCall?.Invoke(true);
					}
					else
					{
						C2VDebug.LogErrorCategory("HiveSDKHelper", $"Show Terms Failed");
						ShowHiveErrorMessage(result);
						afterHelperCall?.Invoke(false);
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(false);
			}
		}

		public static void ShowCustomView(string targetKey, Action<bool> afterHelperCall = null)
		{
			if (CheckPlatform())
			{
				C2VDebug.LogCategory("HiveSDKHelper", $"Show Custom View");
				Promotion.showCustomContents(PromotionCustomType.VIEW, targetKey, (result, eventType) =>
				{
					if (result?.isSuccess() ?? false)
					{
						C2VDebug.LogCategory("HiveSDKHelper", $"Show Custom View Success");
						afterHelperCall?.Invoke(true);
					}
					else
					{
						C2VDebug.LogErrorCategory("HiveSDKHelper", $"Show Custom View Failed");
						ShowHiveErrorMessage(result);
						afterHelperCall?.Invoke(false);
					}
				});
			}
			else
			{
				afterHelperCall?.Invoke(false);
			}
		}
#endregion // CustomView

#region Utils
		private static readonly string TempDid = "0";
		private static          string FailedLinkIdP   => Localization.Instance.GetString("UI_Setting_Account_SyncAccount_FailedLink_Popup_Desc");
		private static          string FailedUnlinkIdP => Localization.Instance.GetString("UI_Setting_Account_SyncAccount_FailedUnlink_Popup_Desc");

		private static bool CheckPlatform()
		{
#if UNITY_EDITOR
			return false;
#else
			return true;
#endif
		}

		private static void ShowHiveErrorMessage(string result, bool forceQuit = false)
		{
			NetworkUIManager.Instance.ShowHiveErrorMessage(result, forceQuit);
		}

		private static void ShowHiveErrorMessage(ResultAPI result, bool forceQuit = false)
		{
			if (result == null)
			{
				C2VDebug.LogWarningCategory("HiveSDKHelper", $"Error ResultAPI is NULL");
				NetworkUIManager.Instance.ShowHiveErrorMessage(result, forceQuit);
			}
			else
			{
				C2VDebug.LogErrorCategory("HiveSDKHelper", $"{result.code} : {result.message}");
				C2VDebug.LogErrorCategory("HiveSDKHelper", $"{result.errorCode} : {result.errorMessage}");
				NetworkUIManager.Instance.ShowHiveErrorMessage(result, forceQuit);
			}
		}
#endregion // Utils
	}
}
