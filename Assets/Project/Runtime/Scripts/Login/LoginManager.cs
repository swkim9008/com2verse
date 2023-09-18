/*===============================================================
* Product:		Com2Verse
* File Name:	LoginManager.cs
* Developer:	mikeyid77
* Date:			2023-04-04 12:23
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Com2Verse.Banner;
using Com2Verse.Chat;
using Com2Verse.Deeplink;
using Com2Verse.Hive;
using UnityEngine;
using Com2Verse.HttpHelper;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Office;
using Com2Verse.Option;
using Com2Verse.Organization;
using Com2Verse.Tutorial;
using Com2Verse.UI;
using Com2Verse.Utils;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using hive;
using Protocols.CommonLogic;
using Protocols.GameLogic;
using Client = Com2Verse.HttpHelper.Client;
using Localization = Com2Verse.UI.Localization;
using Util = Com2Verse.HttpHelper.Util;

namespace Com2Verse.Network
{
	public sealed class LoginManager : MonoSingleton<LoginManager>, IDisposable
	{
		public enum eCom2VerseLoginType
		{
			NONE,
			HIVE_DEV,
			HIVE
		}

		private enum eLoginState
		{
			NONE,
			HIVE_CHECK_COMPLETE,
			SERVER_CHECK_COMPLETE,
			SETTING_CHECK_COMPLETE,
		}

		private eCom2VerseLoginType _currentLoginType  = eCom2VerseLoginType.NONE;
		private eLoginState         _currentLoginState = eLoginState.NONE;
		private bool                _isInitialized     = false;
		private bool                _addClientEvent    = false;
		private string              _deviceId = string.Empty;
		private Action<bool>        _resultServiceLoginAction = null;

		private bool   _needLanguageSave = true;
		private Action _readyToEnter;

		private bool _tokenCheatTrigger = false;
		private int  _tokenCheatAccess  = 0;
		private int  _tokenCheatRefresh = 0;

		public bool   IsHiveLogin()    => _currentLoginType == eCom2VerseLoginType.HIVE;
		public bool   IsInitialized    => _isInitialized;
		public bool   NeedLanguageSave => _needLanguageSave;
		public string DeviceId
		{
			get => _deviceId;
			private set
			{
				_deviceId = value;
				ChatManager.Instance.DeviceId = _deviceId;
			}
		}
		public void Initialize(Action<bool> initializeCallback = null)
		{
			CommonLogic.PacketReceiver.Instance.UserInacceptableWorldNotify += NotifyUserInAcceptableWorld;
			Client.SetOnAccessTokenExpired(async () => await TokenRefreshAsync());
			Client.SetOnRefreshTokenExpired(TokenExpiredNeedLogout);
			Client.SetOnAccessTokenRetryExceed(TokenExpiredNeedLogout);
			HiveSDKHelper.Initialize((result, did) =>
			{
				_isInitialized   = result;
				DeviceId = did;
				initializeCallback?.Invoke(result);
			});
		}

		public void Dispose()
		{
			if (CommonLogic.PacketReceiver.InstanceExists)
				CommonLogic.PacketReceiver.Instance.UserInacceptableWorldNotify -= NotifyUserInAcceptableWorld;
		}

#region Com2Verse
		public void RequestCom2VerseLoginCheat(string cheatId, int access = 0, int refresh = 0)
		{
			HiveSDKHelper.CheckMaintenance(async (isComplete) =>
			{
				if (!isComplete) return;

				C2VDebug.LogCategory("LoginManager", $"{nameof(RequestCom2VerseLogin)}(CHEAT)");
				UIManager.Instance.ShowWaitingResponsePopup(0f);

				if (!_addClientEvent)
				{
					_addClientEvent          =  true;
					Client.OnTimerClearEvent += TimerClearAction;
				}

				if (_currentLoginState == eLoginState.SETTING_CHECK_COMPLETE)
				{
					RequestUserConnectable();
				}
				else if (_currentLoginState == eLoginState.SERVER_CHECK_COMPLETE)
				{
					// RequestSettingValue();
					RequestUserConnectable();
				}
				else if (_currentLoginState == eLoginState.HIVE_CHECK_COMPLETE)
				{
					RequestCom2VerseLoginProtocol();
				}
				else
				{
					_tokenCheatTrigger = true;
					_tokenCheatAccess  = access;
					_tokenCheatRefresh = 0;

					_currentLoginType                      = eCom2VerseLoginType.HIVE_DEV;
					DeviceId                       = ServerInfo.Temp.TempDid;
					User.Instance.CurrentUserData.PlayerId = GenerateHash(cheatId);
					var hiveDevUrl = ServerURL.AddressCom2VerseSignDev;
					var hiveDevRequest = new ServerAPI.RequestCom2VerseSignDev
					{
						did            = DeviceId,
						hiveToken      = ServerInfo.Temp.TempToken,
						hiveValidate   = false,
						pid            = User.Instance.CurrentUserData.PlayerId,
						platform       = ServerInfo.Platform.WindowClient,
						accessExpires  = access,
						refreshExpires = refresh
					};
					await GetCom2VerseUserDataAsync(hiveDevUrl, JsonUtility.ToJson(hiveDevRequest));
				}
			});
		}

		public void RequestCom2VerseLogin(eCom2VerseLoginType type, string dummyId = "")
		{
			HiveSDKHelper.CheckMaintenance(async (isComplete) =>
			{
				if (!isComplete) return;

				C2VDebug.LogCategory("LoginManager", $"{nameof(RequestCom2VerseLogin)}({type.ToString()})");
				UIManager.Instance.ShowWaitingResponsePopup(0f);

				if (!_addClientEvent)
				{
					_addClientEvent          =  true;
					Client.OnTimerClearEvent += TimerClearAction;
				}

				if (_currentLoginState == eLoginState.SETTING_CHECK_COMPLETE)
				{
					RequestUserConnectable();
				}
				else if (_currentLoginState == eLoginState.SERVER_CHECK_COMPLETE)
				{
					// RequestSettingValue();
					RequestUserConnectable();
				}
				else if (_currentLoginState == eLoginState.HIVE_CHECK_COMPLETE)
				{
					RequestCom2VerseLoginProtocol();
				}
				else
				{
					switch (type)
					{
						case eCom2VerseLoginType.HIVE_DEV:
							_currentLoginType                      = eCom2VerseLoginType.HIVE_DEV;
							DeviceId                       = ServerInfo.Temp.TempDid;
							User.Instance.CurrentUserData.PlayerId = GenerateHash(dummyId);
							var hiveDevUrl = ServerURL.AddressCom2VerseSignDev;
							var hiveDevRequest = new ServerAPI.RequestCom2VerseSignDev
							{
								did            = DeviceId,
								hiveToken      = ServerInfo.Temp.TempToken,
								hiveValidate   = false,
								pid            = User.Instance.CurrentUserData.PlayerId,
								platform       = ServerInfo.Platform.WindowClient,
								accessExpires  = 0,
								refreshExpires = 0
							};
							await GetCom2VerseUserDataAsync(hiveDevUrl, JsonUtility.ToJson(hiveDevRequest));
							break;
						case eCom2VerseLoginType.HIVE:
							if (_isInitialized)
							{
								_currentLoginType = eCom2VerseLoginType.HIVE;
								HiveSignIn();
							}
							else
								Logout();
							break;
					}
				}
			});
		}

		private void HiveSignIn()
		{
			C2VDebug.LogCategory("LoginManager", $"Start Hive SignIn");
			HiveSDKHelper.SignIn(async (result, info) =>
			{
				if (result)
				{
					C2VDebug.LogCategory("LoginManager", $"Hive SignIn Success");
					C2VDebug.LogCategory("LoginManager", $"DID : {info.did}");
					C2VDebug.LogCategory("LoginManager", $"PID : {info.playerId}");
					C2VDebug.LogCategory("LoginManager", $"TKN : {info.playerToken}");
					User.Instance.CurrentUserData.PlayerId = (ulong)info.playerId;
					DeviceId                       = info.did;
					C2VDebug.LogCategory("LoginManager", $"Player Id : {User.Instance.CurrentUserData.PlayerId}");

					var hiveUrl = ServerURL.AddressCom2VerseSign;
					var hiveRequest = new ServerAPI.RequestCom2VerseSign
					{
						did       = DeviceId,
						hiveToken = info.playerToken,
						pid       = (ulong)info.playerId,
						platform  = ServerInfo.Platform.WindowClient
					};
					await GetCom2VerseUserDataAsync(hiveUrl, JsonUtility.ToJson(hiveRequest));
				}
				else
				{
					UIManager.Instance.HideWaitingResponsePopup();
				}
			});
		}

		private async UniTask GetCom2VerseUserDataAsync(string url, string jsonData)
		{
			C2VDebug.LogCategory("LoginManager", $"Create HttpRequestBuilder");
			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, url);
			builder.SetContent(jsonData);
			builder.SetContentType(ServerURL.ContentJson);

			C2VDebug.LogCategory("LoginManager", $"Try HttpRequest");
			var responseString = await Client.Message.RequestStringAsync(builder.Request);
			if (responseString == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"Login Response String is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
				HiveSignOut();
			}
			else if (responseString.StatusCode == HttpStatusCode.OK)
			{
				var response = JsonUtility.FromJson<ServerAPI.ResponseCom2VerseSign>(responseString.Value);

				if (response == null)
				{
					C2VDebug.LogErrorCategory("LoginManager", $"Login Response is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
					HiveSignOut();
				}
				else
				{
					if (response.data == null)
					{
						C2VDebug.LogErrorCategory("LoginManager", $"Login Response Data is NULL");
						NetworkUIManager.Instance.ShowCommonErrorMessage();
						HiveSignOut();
					}
					else if (!ServerResult.IsSuccess(response.code))
					{
						C2VDebug.LogErrorCategory("LoginManager", $"Response = {response.msg} ({response.code})");
						NetworkUIManager.Instance.ShowCommonErrorMessage();
						HiveSignOut();
						if (ServerResult.IsInvalidToken(response.code))
							TokenExpiredNeedLogout();
					}
					else
					{
						C2VDebug.LogCategory("LoginManager", $"Get Com2Verse User Data");
						_currentLoginState = eLoginState.HIVE_CHECK_COMPLETE;
						SetCom2VerseUserData(response.data);
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("LoginManager", $"Login Fail : {responseString.StatusCode.ToString()}");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
				HiveSignOut();
			}
		}

		private void SetCom2VerseUserData(ServerAPI.AuthSignResponse response)
		{
			WebApi.SetCallback();

			User.Instance.CurrentUserData.ID           = response.accountId;
			User.Instance.CurrentUserData.UserName     = Convert.ToString(response.accountId);
			User.Instance.CurrentUserData.Domain       = "com2us";
			User.Instance.CurrentUserData.AccessToken  = response.c2vAccessToken;
			User.Instance.CurrentUserData.RefreshToken = response.c2vRefreshToken;

			SetupServerAddressAsync();
			UserState.TutorialAttentionState.SaveKey = $"TutorialAttentionState_{System.Convert.ToString(User.Instance.CurrentUserData.ID)}";

			Organization.DataManager.Instance.UserID = response.accountId;
		}

		private async void SetupServerAddressAsync()
		{
			PacketReceiver.Instance.OnLoginResponseEvent += ResponseCom2VerseLoginProtocol;

			await UniTask.DelayFrame(1);
			User.Instance.SetupServerAddress();
			RequestCom2VerseLoginProtocol();
		}

		private void RequestCom2VerseLoginProtocol()
		{
			C2VDebug.LogCategory("LoginManager", $"Request Com2Verse Login");

			NetworkManager.Instance.StartCom2VerseConnect(Disconnect, async () => await User.Instance.CurrentUserData.SendServiceLoginAsync());
		}

		private void ResponseCom2VerseLoginProtocol(LoginCom2verseResponse response)
		{
			C2VDebug.LogCategory("LoginManager", $"Response Com2Verse Login");
			PacketReceiver.Instance.OnLoginResponseEvent -= ResponseCom2VerseLoginProtocol;

			_currentLoginState = eLoginState.SERVER_CHECK_COMPLETE;
			User.Instance.CurrentUserData.LoginCheck = true;
			ChatManager.Instance.Connect();
			NetworkManager.Instance.OnDisconnected += ChatManager.Instance.Disconnect;

#if !UNITY_EDITOR
			DeeplinkParser.OnEngagementReady();
#endif
		}
#endregion // Com2Verse

#region WaitAccess
		public void CheckLoginQueue(Action readyToEnter, bool needLanguageSave)
		{
			_needLanguageSave = needLanguageSave;
			_readyToEnter = readyToEnter;
			RequestSettingValue();
			TutorialManager.Instance.LoadTutorialLocalLoad().Forget();
		}

		private void RequestSettingValue()
		{
			C2VDebug.LogCategory("LoginManager", $"Request Setting Value");
			PacketReceiver.Instance.SettingValueResponse   += ResponseSettingValue;
			PacketReceiver.Instance.AccountSettingResponse += ResponseAccountSetting;

			OptionController.Instance.InitializeAfterLogin();
			OptionController.Instance.SetTargetStoredOption();
			Commander.Instance.SettingValueRequest();
		}

		private void ResponseSettingValue(SettingValueResponse response)
		{
			PacketReceiver.Instance.SettingValueResponse -= ResponseSettingValue;

			if (!_needLanguageSave)
			{
				PacketReceiver.Instance.AccountSettingResponse -= ResponseAccountSetting;
				_currentLoginState = eLoginState.SETTING_CHECK_COMPLETE;
				RequestUserConnectable();
			}
		}

		private void ResponseAccountSetting(AccountSettingResponse response)
		{
			PacketReceiver.Instance.AccountSettingResponse -= ResponseAccountSetting;

			if (_needLanguageSave)
			{
				_currentLoginState = eLoginState.SETTING_CHECK_COMPLETE;
				RequestUserConnectable();
			}
		}

		private void RequestUserConnectable()
		{
			C2VDebug.LogCategory("LoginManager", $"Request User Connectable");
			CommonLogic.PacketReceiver.Instance.UserConnectableCheckResponse += ResponseUserConnectable;

			Commander.Instance.RequestConnectableToWorld();
		}

		private void ResponseUserConnectable(UserConnectableCheckResponse response)
		{
			CommonLogic.PacketReceiver.Instance.UserConnectableCheckResponse -= ResponseUserConnectable;

			if (response == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"{nameof(UserConnectableCheckResponse)} is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else if (response.IsSuccess)
			{
				C2VDebug.LogCategory("LoginManager", $"Ready to Enter");
				_readyToEnter?.Invoke();
			}
			else
			{
				C2VDebug.LogCategory("LoginManager", $"Check WaitAccess");
				RequestConnectQueue();
			}
		}

		private void RequestConnectQueue()
		{
			CommonLogic.PacketReceiver.Instance.ConnectQueueResponse += ResponseConnectQueue;

			Commander.Instance.RequestConnectQueue();
		}

		private void ResponseConnectQueue(ConnectQueueResponse response)
		{
			CommonLogic.PacketReceiver.Instance.ConnectQueueResponse -= ResponseConnectQueue;

			if (response == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"{nameof(ConnectQueueResponse)} is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else if (response.QueueRanking <= 0)
			{
				C2VDebug.LogCategory("LoginManager", $"Ready to Enter");
				_readyToEnter?.Invoke();
			}
			else
			{
				C2VDebug.LogCategory("LoginManager", $"Show WaitAccess Popup");
				ShowWaitAccessPopup(response.QueueRanking);
			}
		}

		private void ShowWaitAccessPopup(int queueRanking, bool isPublic = true)
		{
			var popupAddress = LoginStringKey.WaitAccessPopup;

			UIManager.Instance.HideWaitingResponsePopup();
			UIManager.Instance.CreatePopup(popupAddress, (createdGuiView) =>
			{
				createdGuiView.Show();

				var viewModel = createdGuiView.ViewModelContainer.GetViewModel<WaitAccessPopupViewModel>();
				viewModel.ReadyToEnterEvent += _readyToEnter;
				viewModel.SetPopupText(queueRanking, isPublic);
			}).Forget();
		}
#endregion // WaitAccess

#region Service
		public void RequestServiceLogin(Action<bool> resultServiceLogin)
		{
			_resultServiceLoginAction = resultServiceLogin;
			RequestServiceLoginAsync();
		}

		public void RequestOrganizationChart() => RequestServiceLoginAsync(false);

		private async void RequestServiceLoginAsync(bool needAccess = true)
		{
			C2VDebug.LogCategory("LoginManager", $"{nameof(RequestServiceLoginAsync)}");

			var request = new Components.LoginShareOfficeRequest
			{
				DeviceType = Components.DeviceType.C2VClient,
			};

			var response = await Api.Common.PostCommonLoginShareOffice(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"ServiceLoginResponse is NULL");
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("LoginManager", $"ServiceLoginResponse Value is NULL");
					NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							C2VDebug.LogCategory("LoginManager", $"ServiceLogin Success");
							DataManager.Instance.SetData(response.Value.Data?.OrganizationChart);
							MeetingReservationProvider.SetMeetingTemplates(response.Value.Data.AvailableMeetingTemplate);
							if (needAccess) ShowEnterPopup();
							break;
						case Components.OfficeHttpResultCode.EmptyMemberResult:
							C2VDebug.LogWarningCategory("LoginManager", $"Need Register Group");
							if (needAccess) ShowRegisterPopup();
							break;
						case Components.OfficeHttpResultCode.CannotMoveUndefinedTeam:
							C2VDebug.LogWarningCategory("LoginManager", $"Can't Move Undefined Team");
							DataManager.Instance.SetData(response.Value.Data?.OrganizationChart);
							MeetingReservationProvider.SetMeetingTemplates(response.Value.Data.AvailableMeetingTemplate);
							if (needAccess) NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
							break;
						default:
							C2VDebug.LogErrorCategory("LoginManager", $"ServiceLogin Error : {response.Value.Code.ToString()}");
							NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("LoginManager", $"ServiceLogin Fail : {response.StatusCode.ToString()}");
				NetworkUIManager.Instance.ShowHttpErrorMessage(response.StatusCode);
			}

			if (!needAccess)
				await RequestJoinBuildingRepresentSpaceChatGroup();
		}

		private async UniTask RequestJoinBuildingRepresentSpaceChatGroup()
		{
			var request = new Components.JoinBuildingRepresentSpaceChatGroupRequest
			{
				BuildingId = (int) OfficeService.Instance.BuildingId,
			};

			var response = await Api.Organization.PostJoinBuildingRepresentSpaceChatGroup(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"JoinBuildingRepresentSpaceChatGroup is NULL");
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("LoginManager", $"JoinBuildingRepresentSpaceChatGroup Value is NULL");
					NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
				}
				else
				{
					if (response.Value.Code == Components.OfficeHttpResultCode.Success)
					{
						ChatManager.Instance.SetAreaMove(response.Value.Data.ChatGroupId);
					}
					else
					{
						C2VDebug.LogErrorCategory("LoginManager", $"JoinBuildingRepresentSpaceChatGroup Error : {response.Value.Code.ToString()}");
						NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("LoginManager", $"JoinBuildingRepresentSpaceChatGroup Fail : {response.StatusCode.ToString()}");
				NetworkUIManager.Instance.ShowHttpErrorMessage(response.StatusCode);
			}
		}

		private void ShowRegisterPopup()
		{
			var popupAddress = LoginStringKey.RegisterMemberPopup;

			UIManager.Instance.CreatePopup(popupAddress, (createdGuiView) =>
			{
				createdGuiView.Show();

				var viewModel = createdGuiView.ViewModelContainer.GetViewModel<RegisterGroupPopupViewModel>();
				viewModel.SetInit(() => createdGuiView.Hide());
			}).Forget();
		}

		public async void RequestRegisterGroup(string inviteCode, Action forceClose)
		{
			C2VDebug.LogCategory("LoginManager", $"{nameof(RequestRegisterGroup)}");

			var request = new Components.RegisterMemberRequest()
			{
				IdentifyKey = inviteCode
			};

			var response = await Api.ConsoleMember.PostConsoleMemberRegisterMember(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"RegisterGroupResponse is NULL");
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("LoginManager", $"RegisterGroupResponse Value is NULL");
					NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							C2VDebug.LogCategory("LoginManager", $"RegisterGroup Success");
							forceClose?.Invoke();
							RequestServiceLoginAsync();
							break;
						case Components.OfficeHttpResultCode.AlreadyExistAccountId:
						case Components.OfficeHttpResultCode.CannotMoveUndefinedTeam:
							C2VDebug.LogWarningCategory("LoginManager", $"RegisterGroup Warning : {response.Value.Code.ToString()}");
							forceClose?.Invoke();
							RequestServiceLoginAsync(false);
							NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
							break;
						case Components.OfficeHttpResultCode.InvalidAuthenticationCode:
						case Components.OfficeHttpResultCode.AlreadyUsingIdentifyCode:
							C2VDebug.LogWarningCategory("LoginManager", $"RegisterGroup Warning : {response.Value.Code.ToString()}");
							NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
							break;
						default:
							C2VDebug.LogErrorCategory("LoginManager", $"RegisterGroup Error : {response.Value.Code.ToString()}");
							NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("LoginManager", $"RegisterGroup Fail : {response.StatusCode.ToString()}");
				NetworkUIManager.Instance.ShowHttpErrorMessage(response.StatusCode);
			}
		}

		private void ShowEnterPopup()
		{
			UIManager.Instance.ShowPopupConfirm(LoginStringKey.CheckEnterOfficeTitle, LoginStringKey.CheckEnterOfficeDesc,
			                                    () => { _resultServiceLoginAction?.Invoke(true); }, LoginStringKey.EnterOfficeButton);
		}
#endregion // Service

#region Token
		public async UniTask<bool> TryRefreshToken(Action onSuccess)
		{
			var success = await TokenRefreshAsync(success =>
			{
				if (success)
					onSuccess?.Invoke();
			});
			return success;
		}
		private async UniTask<bool> TokenRefreshAsync(Action<bool> onSuccess = null)
		{
			C2VDebug.LogCategory("LoginManager", $"{nameof(TokenRefreshAsync)}");

			string jsonRequest = string.Empty;
			if (_tokenCheatTrigger)
			{
				var request = new ServerAPI.RequestCom2VerseTokenRefreshDev
				{
					c2vRefreshToken = User.Instance.CurrentUserData.RefreshToken,
					accessExpires   =  _tokenCheatAccess,
					refreshExpires  = _tokenCheatRefresh
				};
				jsonRequest = JsonUtility.ToJson(request);
			}
			else
			{
				var request = new ServerAPI.RequestCom2VerseTokenRefresh
				{
					c2vRefreshToken = User.Instance.CurrentUserData.RefreshToken
				};
				jsonRequest = JsonUtility.ToJson(request);
			}

			if (string.IsNullOrEmpty(jsonRequest)) return false;
			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, ServerURL.AddressCom2VerseTokenRefresh);
			builder.SetContent(jsonRequest);
			builder.SetContentType(ServerURL.ContentJson);
			Client.Auth.SetTokenAuthentication(Util.MakeTokenAuthInfo(User.Instance.CurrentUserData.AccessToken));

			C2VDebug.LogCategory("LoginManager", $"Try HttpRequest");
			var requestInfo = new Client.RequestDataMessage(builder.Request) {CanResend = false,};
			var responseString = await Client.Message.RequestStringAsync(requestInfo);
			if (responseString == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"Refresh Com2Verse Token Response String is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else if (responseString.StatusCode == HttpStatusCode.OK)
			{
				var response = JsonUtility.FromJson<ServerAPI.ResponseCom2VerseTokenRefresh>(responseString.Value);

				if (response == null)
				{
					C2VDebug.LogErrorCategory("LoginManager", $"Refresh Com2Verse Token Response is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
				}
				else
				{
					if (response.data == null)
					{
						C2VDebug.LogErrorCategory("LoginManager", $"Refresh Com2Verse Token Data is NULL");
						NetworkUIManager.Instance.ShowCommonErrorMessage();
					}
					else if (!ServerResult.IsSuccess(response.code))
					{
						C2VDebug.LogErrorCategory("LoginManager", $"Response = {response.msg} ({response.code})");
						NetworkUIManager.Instance.ShowCommonErrorMessage();
					}
					else
					{
						C2VDebug.LogCategory("LoginManager", $"Refresh Token");
						User.Instance.CurrentUserData.AccessToken  = response.data.c2vAccessToken;
						User.Instance.CurrentUserData.RefreshToken = response.data.c2vRefreshToken;
						Client.Auth.SetTokenAuthentication(Util.MakeTokenAuthInfo(User.Instance.CurrentUserData.AccessToken));
						onSuccess?.Invoke(true);
						return true;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("LoginManager", $"Refresh Com2Verse Token Fail : {responseString.StatusCode.ToString()}");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}

			onSuccess?.Invoke(false);
			return false;
		}

		private async void TokenDelete(Action success)
		{
			C2VDebug.LogCategory("LoginManager", $"{nameof(TokenDelete)}");

			var request = new ServerAPI.RequestCom2VerseTokenDelete
			{
				did = DeviceId,
				accountId = User.Instance.CurrentUserData.ID
			};

			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, ServerURL.AddressCom2VerseTokenDelete);
			builder.SetContent(JsonUtility.ToJson(request));
			builder.SetContentType(ServerURL.ContentJson);
			Client.Auth.SetTokenAuthentication(Util.MakeTokenAuthInfo(User.Instance.CurrentUserData.AccessToken));

			C2VDebug.LogCategory("LoginManager", $"Try HttpRequest");
			var responseString = await Client.Message.RequestStringAsync(builder.Request);
			if (responseString == null)
			{
				C2VDebug.LogErrorCategory("LoginManager", $"Delete Com2Verse Token Response String is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else if (responseString.StatusCode == HttpStatusCode.OK)
			{
				var response = JsonUtility.FromJson<ServerAPI.ResponseCom2VerseTokenDelete>(responseString.Value);

				if (response == null)
				{
					C2VDebug.LogErrorCategory("LoginManager", $"Delete Com2Verse Token Response is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
				}
				else
				{
					if (!ServerResult.IsSuccess(response.code))
					{
						C2VDebug.LogErrorCategory("LoginManager", $"Response = {response.msg} ({response.code})");
						NetworkUIManager.Instance.ShowCommonErrorMessage();
					}
					else
					{
						C2VDebug.LogCategory("LoginManager", $"Delete Token");
						success?.Invoke();
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("LoginManager", $"Delete Com2Verse Token Fail : {responseString.StatusCode.ToString()}");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
		}

		private void TokenExpiredNeedLogout()
		{
			// TODO : 씬 이동 가능한 상태인지 체크
			UIManager.Instance.ShowPopupCommon("[StringKey 필요]\n토큰 만료됨.\n로그인 화면으로 이동합니다.", () => LoadingManager.Instance.ChangeScene<SceneLogin>());
		}
#endregion // Token

#region Logout
		public void Logout()
		{
			C2VDebug.LogWarningCategory("LoginManager", $"Logout");

			HiveSignOut();
			NetworkManager.Instance.OnLogout += OnLogout;
			NetworkManager.Instance.Disconnect(true);
		}

		private void OnLogout()
		{
			NetworkManager.Instance.OnLogout -= OnLogout;
			UIManager.Instance.HideWaitingResponsePopup();
			LoadingManager.Instance.ChangeScene<SceneLogin>();
			LocalSave.ClearId();
		}

		public void Disconnect()
		{
			C2VDebug.LogWarningCategory("LoginManager", $"Disconnect");

			Organization.DataManager.DisposeOrganization();

			UIManager.Instance.HideWaitingResponsePopup();
			_currentLoginState        = eLoginState.NONE;
			_resultServiceLoginAction = null;
			_addClientEvent           = false;
			_tokenCheatTrigger        = false;
			_tokenCheatAccess         = 0;
			_tokenCheatRefresh        = 0;
			User.Instance.OnLogout();
			BannerManager.Instance.ResetBanner(true);
			DeeplinkManager.ResetComponent();
		}

		private void HiveSignOut()
		{
			HiveSDKHelper.SignOut((result) =>
			{
				if (result)
					_currentLoginState = eLoginState.NONE;
			});
		}
#endregion // Logout

#region Utils
		private static ulong GenerateHash(string input)
		{
			using (SHA256 sha = SHA256.Create())
			{
				byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
				var hashValue = BitConverter.ToUInt64(hashBytes, 0);
				return hashValue;
			}
		}

		private void NotifyUserInAcceptableWorld(UserInacceptableWorldNotify response)
		{
			C2VDebug.LogWarningCategory("LoginManager", $"NotifyUserInAcceptableWorld");

			// TODO : 입장 직전에 해당 Notify를 받으면 다시 대기열로 돌아감
			// TODO : 이부분에 대한 UI가 별도로 필요해보임
			RequestUserConnectable();
		}

		public void TimerClearAction() { }
#endregion // Utils

#region API
		// ReSharper disable InconsistentNaming
		private static class LoginStringKey
		{
			// TODO : StringKey 필요
			public static readonly string WaitAccessPopup     = "UI_Popup_WaitAccess";
			public static readonly string RegisterMemberPopup = "UI_Popup_RegisterGroup";

			public static string InvalidInviteCode     => Localization.Instance.GetString("UI_Spaxe_AuthenticationCode_Toast_Msg");
			public static string AlreadyExistDesc      => Localization.Instance.GetString("UI_Spaxe_Enter_Popup_Desc2");
			public static string CheckEnterOfficeTitle => Localization.Instance.GetString("UI_Spaxe_Enter_Popup_Title");
			public static string CheckEnterOfficeDesc  => Localization.Instance.GetString("UI_Spaxe_Enter_Popup_Desc");
			public static string EnterOfficeButton     => Localization.Instance.GetString("UI_SPAXE_EnterSPAXE_Btn");
			public static string CommonTitle           => Localization.Instance.GetString("UI_Common_Notice_Popup_Title");
			public static string CommonButtonOk        => Localization.Instance.GetString("UI_Common_Btn_OK");

			public static string FailedLinkIdP   => Localization.Instance.GetString("UI_Setting_Account_SyncAccount_FailedLink_Popup_Desc");
			public static string FailedUnlinkIdP => Localization.Instance.GetString("UI_Setting_Account_SyncAccount_FailedUnlink_Popup_Desc");
		}

		public static class ServerURL
		{
			private static string TargetAuth => Configurator.Instance.Config?.WorldAuth;

			private static readonly string AuthApi    = "/api/auth/v1";
			private static readonly string AdapterApi = "/api/adapter/v1";

			private static readonly string Com2VerseSign         = $"{AuthApi}/sign";
			private static readonly string Com2VerseSignDev      = $"{AuthApi}/sign-dev";
			private static readonly string Com2VerseTokenRefresh = $"{AuthApi}/token/refresh";
			private static readonly string Com2VerseTokenDelete  = $"{AuthApi}/token/delete";

			private static readonly string ServicePeek       = $"{AdapterApi}/world/peek";
			private static readonly string ServiceOAuth      = $"{AdapterApi}/world/authcode";
			private static readonly string ServiceCheck      = $"{AdapterApi}/check";
			private static readonly string ServiceList       = $"{AdapterApi}/list";
			private static readonly string ServiceDisconnect = $"{AdapterApi}/disconnect";

			private static readonly string ServiceRegister = "/api/ConsoleMember/RegisterMember";
			private static readonly string ServiceSign     = "/api/Common/LoginShareOffice";

			public static string AddressCom2VerseSign         => $"{TargetAuth}{Com2VerseSign}";
			public static string AddressCom2VerseSignDev      => $"{TargetAuth}{Com2VerseSignDev}";
			public static string AddressCom2VerseTokenRefresh => $"{TargetAuth}{Com2VerseTokenRefresh}";
			public static string AddressCom2VerseTokenDelete  => $"{TargetAuth}{Com2VerseTokenDelete}";
			public static string AddressServicePeek           => $"{TargetAuth}{ServicePeek}";
			public static string AddressServiceOAuth          => $"{TargetAuth}{ServiceOAuth}";
			public static string AddressServiceCheck          => $"{TargetAuth}{ServiceCheck}";
			public static string AddressServiceList           => $"{TargetAuth}{ServiceList}";
			public static string AddressServiceDisconnect     => $"{TargetAuth}{ServiceDisconnect}";

			public static readonly string ContentJson = "application/json";
		}

		public static class ServerInfo
		{
			public static class Temp
			{
				public static readonly string TempDid     = "0";
				public static readonly string TempToken   = "0";
				public static readonly string EditorDid   = "10000005958";
				public static readonly string EditorToken = "0";
				public static readonly ulong  EditorPid   = 30000091518;
			}

			public static class ServiceType
			{
				public static readonly int Office = 1;
			}

			public static class OfficeCode
			{
				public static readonly string Com2us = "com2us_office";
			}

			public static class Platform
			{
				public static readonly int WindowClient = 20827;
			}

			public static class IDP
			{
				public static readonly string CustomProviderType = "CUSTOM_GAME";
			}
		}

		private static class ServerResult
		{
			//public static readonly int  Success      = 200;
			private static readonly int ErrorRequestHeader = -201;
			private static readonly int ErrorInvalidTokenNeedLogout = -204;
			private static readonly int ErrorIsValidToken = -205;
			public static readonly int BadRequest   = -400;
			public static readonly int Unauthorized = -401;
			public static readonly int NotFound     = -404;
			public static readonly int ServerError  = -500;
			public static readonly int NeedConnectIdp    = 2102;
			public static readonly int HiveValidateFalse = 9999;

			public static bool IsSuccess(int code) => code > 0 || code == ErrorIsValidToken;
			public static bool IsInvalidToken(int code) => code == ErrorRequestHeader || code == ErrorInvalidTokenNeedLogout;
		}

		public static class ServerAPI
		{
			[Serializable]
			public class RequestCom2VerseSign
			{
				public string did;
				public string hiveToken;
				public ulong  pid;
				public int    platform;
			}

			[Serializable]
			public class RequestCom2VerseSignDev
			{
				public string did;
				public string hiveToken;
				public bool   hiveValidate;
				public ulong  pid;
				public int    platform;
				public int    accessExpires;
				public int    refreshExpires;
			}

			[Serializable]
			public class RequestServicePeek
			{
				public int    serviceType;
				public string code;
				public ulong  pid;
				public string hiveToken;
				public string did;
				public int    platform;
				public bool   hiveValidate;
			}

			[Serializable]
			public class RequestServiceCheck
			{
				public ulong  pid;
				public string hiveToken;
				public string did;
				public int    platform;
			}

			[Serializable]
			public class RequestServiceDisconnect
			{
				public int    serviceId;
				public string serviceUserId;
			}

			[Serializable]
			public class RequestCom2VerseTokenRefresh
			{
				public string c2vRefreshToken;
			}

			[Serializable]
			public class RequestCom2VerseTokenRefreshDev
			{
				public string c2vRefreshToken;
				public int    accessExpires;
				public int    refreshExpires;
			}

			[Serializable]
			public class RequestCom2VerseTokenDelete
			{
				public string did;
				public long   accountId;
			}

			[Serializable]
			public class ResponseBase
			{
				public int    code;
				public string msg;
			}

			[Serializable]
			public class ResponseCom2VerseSign : ResponseBase
			{
				public AuthSignResponse data;
			}

			[Serializable]
			public class AuthSignResponse
			{
				public int    accountId;
				public string c2vAccessToken;
				public string c2vRefreshToken;
			}

			[Serializable]
			public class ResponseServicePeek : ResponseBase
			{
				public AuthServicePeekResponse data;
			}

			[Serializable]
			public class AuthServicePeekResponse
			{
				public string serviceUri;
			}

			[Serializable]
			public class ResponseServiceOAuth : ResponseBase
			{
				public ObjData data;
			}

			[Serializable]
			public class ObjData
			{
				public string         authKey;
				public ObjServiceInfo serviceInfo;
			}

			[Serializable]
			public class ObjServiceInfo
			{
				public string serviceAccessToken;
				public string serviceRefreshToken;
				public int    serviceType;
				public int    serviceId;
				public string serviceUserId;
			}

			[Serializable]
			public class ResponseServiceCheck : ResponseBase { }

			[Serializable]
			public class ResponseServiceList : ResponseBase
			{
				public ListData data;
			}

			[Serializable]
			public class ListData
			{
				public ServiceUserList[] serviceUserList;
			}

			[Serializable]
			public class ServiceUserList
			{
				public int    serviceId;
				public string serviceUserId;
			}

			[Serializable]
			public class ResponseServiceDisconnect : ResponseBase { }

			[Serializable]
			public class ResponseCom2VerseTokenRefresh : ResponseBase
			{
				public AuthTokenRefreshResponse data;
			}

			[Serializable]
			public class AuthTokenRefreshResponse
			{
				public string c2vAccessToken;
				public string c2vRefreshToken;
			}

			[Serializable]
			public class ResponseCom2VerseTokenDelete : ResponseBase { }
		}
		// ReSharper disable InconsistentNaming
#endregion
	}
}
