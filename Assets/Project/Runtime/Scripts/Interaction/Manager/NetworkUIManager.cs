/*===============================================================
* Product:		Com2Verse
* File Name:	PortalManager.cs
* Developer:	tlghks1009
* Date:			2022-09-28 12:49
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Com2Verse.Data;
using Com2Verse.Director;
using Com2Verse.EventTrigger;
using Com2Verse.HttpHelper;
using Com2Verse.Interaction;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Protocols.GameLogic;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class NetworkUIManager : Singleton<NetworkUIManager>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private NetworkUIManager() { }

		private readonly Dictionary<eLogicType, BaseLogicTypeProcessor> _logicTypeProcessorDict = new();
		public RepeatedField<Portal> ElevatorPortalListCache { get; private set; }

		public long CurrentMapId { get; private set; } = 0;
		public long LastMapId { get; private set; } = 0;

		public long TargetFieldID { get; set; } = 0;

		public long TeleportTargetID { get; set; } = 0;
		private Vector3 _teleportTargetPosition;
		private Dictionary<long, SpaceTemplate> _spaceTemplates = null;

		public IReadOnlyDictionary<long, SpaceTemplate> SpaceTemplates => _spaceTemplates;

		/// <summary>
		/// 최초 필요한 이벤트 등록
		/// </summary>
		public void Initialize()
		{
			PacketReceiver.Instance.OnStandInTriggerNotifyEvent    += OnResponseStandInTriggerNotify;
			PacketReceiver.Instance.OnGetOffTriggerNotifyEvent     += OnResponseGetOffTriggerNotify;
			PacketReceiver.Instance.OnTeleportUserStartNotifyEvent += OnResponseTeleportUserStartNotify;
			PacketReceiver.Instance.OnFieldMoveNotifyEvent         += OnResponseTeleportFinishNotify;
			PacketReceiver.Instance.OnUsePortalResponseEvent       =  OnResponseUsePortal;
			NetworkManager.Instance.SocketDisconnected             += OnSocketDisconnected;
			NetworkManager.Instance.OnDisconnected                 += OnDisconnect;
			NetworkManager.Instance.OnNetworkError                 += OnNetworkError;

			Network.GameLogic.PacketReceiver.Instance.ObjectInteractionEnterFailNotify += OnEnterFail;
			RegisterLogicTypeProcessor();
			LoadSpaceTemplate();
			
			if (GeneralData.General != null)
				Client.SetWebRequestTimeout(GeneralData.General.ResponsePending, GeneralData.General.ResponseTimeout);
			Client.OnPendingEvent    += OnPendingWebRequest;
			Client.OnTimeoutEvent    += OnTimeoutWebRequest;
			Client.OnTimerClearEvent += OnTimerClear;
		}

		private void LoadSpaceTemplate()
		{
			var spaceData = TableDataManager.Instance.Get<TableSpaceTemplate>();
			_spaceTemplates = spaceData.Datas;
		}

		private void OnSocketDisconnected()
		{
			C2VDebug.LogWarning($"on socket disconnected");
			UIManager.Instance.ShowPopupConfirm(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
			                                    Localization.Instance.GetString("UI_User_Logout_Popup_Desc"),
			                                    allowCloseArea: false,
			                                    onShowAction: guiView =>
			                                    {
				                                    void OnClosedAction(GUIView view)
				                                    {
					                                    guiView.OnClosedEvent -= OnClosedAction;
					                                    LoadingManager.Instance.ChangeScene<SceneLogin>();
				                                    }
				                                    guiView.OnClosedEvent += OnClosedAction;
			                                    });
		}

		private void OnDisconnect()
		{
			CurrentMapId = -1;
			LoginManager.Instance.Disconnect();
		}

		private void OnNetworkError(string message)
		{
			// UIManager.Instance.ShowPopupCommon(message);
		}

		public void Dispose()
		{
			_logicTypeProcessorDict.Clear();
			if (Network.GameLogic.PacketReceiver.InstanceExists)
				Network.GameLogic.PacketReceiver.Instance.ObjectInteractionEnterFailNotify -= OnEnterFail;

			CurrentMapId = 0;

			if (NetworkManager.InstanceExists)
			{
				NetworkManager.Instance.SocketDisconnected -= OnSocketDisconnected;
				NetworkManager.Instance.OnDisconnected     -= OnDisconnect;
				NetworkManager.Instance.OnNetworkError     -= OnNetworkError;
			}
			
			Client.OnPendingEvent    -= OnPendingWebRequest;
			Client.OnTimeoutEvent    -= OnTimeoutWebRequest;
			Client.OnTimerClearEvent -= OnTimerClear;
		}

		/// <summary>
		/// 외부에서 강제로 실행 시킬 일이 있을 때.
		/// </summary>
		/// <param name="logicType"></param>
		public void ForceRunLogicTypeProcessor(eLogicType logicType)
		{
			switch (logicType)
			{
				case eLogicType.CHAIR:
					C2VDebug.LogError("Cannot force chair command");
					break;
				case eLogicType.ELEVATOR:
				case eLogicType.EXIT__MEETING:
				case eLogicType.ENTER__MEETING:
				case eLogicType.BOARD__READ:
				default:
					if (_logicTypeProcessorDict.TryGetValue(logicType, out var logicTypeProcessor))
					{
						logicTypeProcessor.OnInteraction(new TriggerInEventParameter()
						{
							SourcePacket = new StandInTriggerNotify()
							{
								LogicType = (int)logicType
							}
						});
					}
					break;
			}
		}

		public T GetLogicTypeProcessor<T>() where T : BaseLogicTypeProcessor
		{
			foreach (var processor in _logicTypeProcessorDict.Values)
			{
				if (processor.GetType() == typeof(T))
				{
					return (T)processor;
				}
			}

			return null;
		}


		private void RegisterLogicTypeProcessor()
		{
			_logicTypeProcessorDict.Clear();

			var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(LogicTypeAttribute)));

			foreach (var type in types)
			{
				var logicTypeAttribute = type.GetCustomAttribute<LogicTypeAttribute>();

				var logicTypeProcessor = Activator.CreateInstance(type) as BaseLogicTypeProcessor;
				logicTypeProcessor.LogicType = logicTypeAttribute.LogicType;

				AddLogicTypeProcessor(logicTypeAttribute.LogicType, logicTypeProcessor);

				var template = InteractionManager.Instance.GetActionType(logicTypeAttribute.LogicType);
				if (template != null)
				{
					logicTypeProcessor.ActionType = template.ActionType;
					logicTypeProcessor.SoundEffectPath = template.SoundRes;
				}
			}
		}

		/// <summary>
		/// 트리거에서 벗어 날 때
		/// </summary>
		/// <param name="response"></param>
		public void OnResponseGetOffTriggerNotify(Protocols.GameLogic.GetOffTriggerNotify response)
		{
			OnTriggerExit(new TriggerOutEventParameter()
			{
				SourcePacket = response
			});
		}

		/// <summary>
		/// 트리거에 걸렸을 때
		/// </summary>
		/// <param name="response"></param>
		public void OnResponseStandInTriggerNotify(Protocols.GameLogic.StandInTriggerNotify response)
		{
			OnTriggerEnter(new TriggerInEventParameter()
			{
				SourcePacket = response
			});
		}

		public void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			eLogicType targetLogicType = zone.Callback[callbackIndex].LogicType;

			if (_logicTypeProcessorDict.TryGetValue(targetLogicType, out var logicTypeProcessor))
			{
				logicTypeProcessor.OnZoneEnter(zone, callbackIndex);
			}
		}

		public void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			var targetLogicType = zone.Callback[callbackIndex].LogicType;

			if (_logicTypeProcessorDict.TryGetValue(targetLogicType, out var logicTypeProcessor))
			{
				logicTypeProcessor.OnZoneExit(zone, callbackIndex);
			}
		}

		public void OnTriggerClick(TriggerEventParameter parameter)
		{
			eLogicType targetLogicType;
			targetLogicType = (eLogicType)parameter.SourceTrigger.Callback[parameter.CallbackIndex].Function;

			if (_logicTypeProcessorDict.TryGetValue(targetLogicType, out var logicTypeProcessor))
			{
				logicTypeProcessor.OnTriggerClick(parameter);
			}
		}

		public void OnTriggerEnter(TriggerInEventParameter parameter)
		{
			eLogicType targetLogicType;
			if (parameter.SourcePacket != null)
			{
				targetLogicType = (eLogicType)parameter.SourcePacket.LogicType;
			}
			else
			{
				targetLogicType = (eLogicType)parameter.SourceTrigger.Callback[parameter.CallbackIndex].Function;
			}

			if (_logicTypeProcessorDict.TryGetValue(targetLogicType, out var logicTypeProcessor))
			{
				logicTypeProcessor.OnTriggerEnter(parameter);
			}
		}

		public void OnTriggerExit(TriggerOutEventParameter parameter)
		{
			eLogicType targetLogicType;
			if (parameter.SourcePacket != null)
			{
				targetLogicType = (eLogicType)parameter.SourcePacket.LogicType;
			}
			else
			{
				targetLogicType = (eLogicType)parameter.SourceTrigger.Callback[parameter.CallbackIndex].Function;
			}

			if (_logicTypeProcessorDict.TryGetValue(targetLogicType, out var logicTypeProcessor))
			{
				logicTypeProcessor.OnTriggerExit(parameter);
			}
		}

		public void OnEnterFail(ObjectInteractionEnterFailNotify failNotify)
		{
			eLogicType targetLogicType = (eLogicType)failNotify.InteractionId;

			if (_logicTypeProcessorDict.TryGetValue(targetLogicType, out var logicTypeProcessor))
			{
				logicTypeProcessor.OnEnterFail(failNotify);
			}
		}

		private void OnResponseTeleportUserStartNotify(Protocols.WorldState.TeleportUserStartNotify teleportUserStartNotify)
		{
			UserDirector.Instance.WaitingForWarpEffect(false, () => OnFieldChange(teleportUserStartNotify.MapId)).Forget();
		}

		/// <summary>
		/// 필드 내 이동 후 서버로부터의 Notify
		/// </summary>
		/// <param name="teleportToUserFinishNotify"></param>
		private void OnResponseTeleportFinishNotify(Protocols.WorldState.TeleportToUserFinishNotify teleportToUserFinishNotify)
		{
			C2VDebug.LogCategory("OrganizationVoiceTalk", $"Notify TeleportToUserFinishNotify packet");

			var userFunction = UserDirector.Instance;
			userFunction.WaitingForWarpEffect(false, () =>
			{
				User.Instance.CharacterObject.transform.position = _teleportTargetPosition;
				User.Instance.CharacterObject.ForceSetCurrentPositionToState();

				userFunction.WaitingForWarpEffect(true, () =>
				{
					User.Instance.CharacterObject.ForceSetUpdateEnable(true);
					// Commander.Instance.MoveObjectCompleted(); // 서버 요청으로 클라의 모든 연출이 종료되었음을 알림
					Commander.Instance.TeleportActionCompletionNotify();
				}).Forget();
			}).Forget();
		}

		/// <summary>
		/// 텔레 포트 목적지 설정
		/// </summary>
		/// <param name="usePortalResponse"></param>
		private void OnResponseUsePortal(Protocols.GameLogic.UsePortalResponse usePortalResponse)
		{
			TargetFieldID = usePortalResponse.FieldId;
		}

		private void OnFieldChange(long templateId)
		{
			_onFieldChange?.Invoke();

			LastMapId = CurrentMapId;
			CurrentMapId = templateId;

			var sceneProperty = GetSceneProperty(templateId);
			if (sceneProperty == null)
			{
				C2VDebug.LogError($"Invalid scene property : {templateId}");
				return;
			}

			LoadingManager.Instance.ChangeScene<SceneSpace>(sceneProperty);
		}

		private SceneProperty GetSceneProperty(long templateId)
		{
			if (templateId == 1) // (FIXME) 임시 광장
			{
				return SceneManager.Instance.WorldSceneProperty;
			}

			if (_spaceTemplates == null)
			{
				C2VDebug.LogError("SpaceTemplate data not loaded");
				return null;
			}

			if (!_spaceTemplates.TryGetValue(templateId, out SpaceTemplate templateData))
			{
				C2VDebug.LogError($"Invalid space template id : {templateId}");
				return null;
			}

			return SceneProperty.Convert(templateData);
		}

		private void AddLogicTypeProcessor(eLogicType logicType, BaseLogicTypeProcessor logicTypeProcessor)
		{
			if (_logicTypeProcessorDict.ContainsKey(logicType))
			{
				return;
			}

			_logicTypeProcessorDict.Add(logicType, logicTypeProcessor);
		}


		private void RemoveLogicTypeProcessor(eLogicType logicType)
		{
			if (!_logicTypeProcessorDict.ContainsKey(logicType))
			{
				return;
			}

			_logicTypeProcessorDict.Remove(logicType);
		}

		private Action _onFieldChange;
		public event Action OnFieldChangeEvent
		{
			add
			{
				_onFieldChange -= value;
				_onFieldChange += value;
			}
			remove => _onFieldChange -= value;
		}

#region Timeout
		private void OnPendingWebRequest()
		{
			C2VDebug.LogWarning($"on pending");
			UIManager.Instance.ShowWaitingResponsePopup();
		}

		private void OnTimeoutWebRequest()
		{
			C2VDebug.LogWarning($"on timeout");
			UIManager.Instance.HideWaitingResponsePopup();
			PlayerController.Instance.SetStopAndCannotMove(false);
			User.Instance.RestoreStandBy();
			UIManager.Instance.ShowPopupConfirm(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
			                                    Localization.Instance.GetString("UI_Error_Timeout_Popup_Desc"),
			                                    allowCloseArea: false,
			                                    onShowAction: guiView =>
			                                    {
				                                    void OnClosedAction(GUIView view)
				                                    {
					                                    guiView.OnClosedEvent -= OnClosedAction;
					                                    LoadingManager.Instance.ChangeScene<SceneLogin>();
				                                    }
				                                    guiView.OnClosedEvent += OnClosedAction;
				                                    NetworkManager.Instance.Disconnect(true);
			                                    });
		}

		private void OnTimerClear()
		{
			UIManager.Instance.HideWaitingResponsePopup();
		}
#endregion Timeout

#region CommonErrorMessage
		private string NoResponseErrorContext => Localization.Instance.GetString("UI_Error_Server_Popup01");
		private string RequestErrorContext    => Localization.Instance.GetString("UI_PatchServer_Popup_Desc_RequestFail");
		private string CommonErrorTitle       => Localization.Instance.GetString("UI_Title_Popup_Title_Notice");
		private string CommonErrorContext     => Localization.Instance.GetString("UI_Error_Server_Popup02");
		private string CommonErrorButton      => Localization.Instance.GetString("UI_Title_Popup_Btn_Exit");
		private string TimeoutErrorContext    => Localization.Instance.GetString("UI_Error_Timeout_Popup_Desc");

		public string GetProtocolErrorContext(Protocols.ErrorCode errorCode)
		{
			if (errorCode == 0)
			{
				return $"{CommonErrorContext}\n[ErrorCode : 0({eNetErrorSourceType.WORLD.ToShortWord()})]";
			}
			else
			{
				var value = Localization.Instance.GetErrorString((int)errorCode);
				if (string.IsNullOrWhiteSpace(value))
				{
					return $"{CommonErrorContext}\n[ErrorCode : {(int)errorCode}({eNetErrorSourceType.WORLD.ToShortWord()})]";
				}
				else
				{
					value = value?.Replace("\\n", "\n");
					return $"{value}\n[ErrorCode : {(int)errorCode}({eNetErrorSourceType.WORLD.ToShortWord()})]";
				}
			}
		}

		public void ShowCommonErrorMessage()
		{
			var context = $"{CommonErrorContext}";
			ShowErrorMessage(context, eErrorMessageType.POPUP);
		}

		public void ShowHiveErrorMessage(string result, bool forceQuit = false)
		{
			var context = $"{result}\n[ErrorType : 0({eNetErrorSourceType.HIVE.ToShortWord()})]";
			ShowErrorMessage(context, eErrorMessageType.POPUP, forceQuit);
		}

		public void ShowHiveErrorMessage(hive.ResultAPI result, bool forceQuit = false)
		{
			if (result == null)
			{
				var context = $"{CommonErrorContext}\n[ErrorType : 0({eNetErrorSourceType.HIVE.ToShortWord()})]";
				ShowErrorMessage(context, eErrorMessageType.POPUP, forceQuit);
			}
			else
			{
				var message = result.message?.ToString();
				var commonContext = (string.IsNullOrWhiteSpace(message))
					? $"[ErrorType : {(int)result.code}]"
					: $"{result.message} [ErrorType : {(int)result.code}({eNetErrorSourceType.HIVE.ToShortWord()})]";

				var errorMessage = result.errorMessage?.ToString();
				var errorContext = (string.IsNullOrWhiteSpace(errorMessage))
					? $"[ErrorType : {(int)result.errorCode}]"
					: $"{result.errorMessage} [ErrorType : {(int)result.errorCode}({eNetErrorSourceType.HIVE.ToShortWord()})]";

				var context = $"{RequestErrorContext}\n{commonContext}\n{errorContext}";
				ShowErrorMessage(context, eErrorMessageType.POPUP, forceQuit);
			}
		}

		public void ShowHttpErrorMessage(HttpStatusCode statusCode)
		{
			if (statusCode == HttpStatusCode.RequestTimeout)
			{
				var context = $"{TimeoutErrorContext}\n[StatusCode : {(int)statusCode}({eNetErrorSourceType.HTTP.ToShortWord()})]";
				ShowErrorMessage(context, eErrorMessageType.POPUP);
			}
			else
			{
				var context = $"{NoResponseErrorContext}\n[StatusCode : {(int)statusCode}({eNetErrorSourceType.HTTP.ToShortWord()})]";
				ShowErrorMessage(context, eErrorMessageType.POPUP);
			}
		}

		public void ShowProtocolErrorMessage(Protocols.ErrorCode errorCode)
		{
			ShowErrorMessage(GetProtocolErrorContext(errorCode), eErrorMessageType.POPUP);
		}

		public void ShowWebApiErrorMessage(Components.OfficeHttpResultCode resultCode)
		{
			if (resultCode == 0)
			{
				var context = $"{CommonErrorContext}\n[ResultCode : 0({eNetErrorSourceType.OFFICE.ToShortWord()})]";
				ShowErrorMessage(context, eErrorMessageType.POPUP);
			}
			else
			{
				var value = Localization.Instance.CheckOfficeErrorStringValue((int)resultCode);
				if (value == null)
				{
					var context = $"{CommonErrorContext}\n[ResultCode : {(int)resultCode}]";
					ShowErrorMessage(context, eErrorMessageType.POPUP);
				}
				else
				{
					var text    = Localization.Instance.GetOfficeErrorString((int)resultCode);
					var target  = (string.IsNullOrWhiteSpace(text)) ? CommonErrorContext : text;
					var context = (value.IsShowErrorCode) ? $"{target}\n[ResultCode : {(int)resultCode}({eNetErrorSourceType.OFFICE.ToShortWord()})]" : $"{target}";
					ShowErrorMessage(context, value.Type);
				}
			}
		}

		public void ShowMiceWebApiErrorMessage(MiceWebClient.eMiceHttpErrorCode resultCode)
		{
			if (resultCode == MiceWebClient.eMiceHttpErrorCode.OK) return;
			if (TableDataManager.Instance.Get<TableMiceWebErrorString>().Datas.TryGetValue((int)resultCode, out var result))
			{
				var text    = Localization.Instance.GetMiceWebErrorString((int)resultCode);
				var target  = (string.IsNullOrWhiteSpace(text)) ? CommonErrorContext : text;
				var context = (result.IsShowErrorCode) ? $"{target}\n[ResultCode : {(int)resultCode}({eNetErrorSourceType.MICE_WEB.ToShortWord()})]" : $"{target}";
				ShowErrorMessage(context, result.Type);
			}
			else
			{
				var context = $"{CommonErrorContext}\n[ResultCode : {(int)resultCode}({eNetErrorSourceType.MICE_WEB.ToShortWord()})]";
				ShowErrorMessage(context, eErrorMessageType.POPUP);
			}
		}
		public void ShowMiceErrorMessage(Protocols.Mice.EnterRequestResult resultCode)
		{
			if (resultCode == Protocols.Mice.EnterRequestResult.Success) return;
			if (TableDataManager.Instance.Get<TableMiceErrorString>().Datas.TryGetValue((int)resultCode, out var result))
			{
				var text    = Localization.Instance.GetMiceErrorString((int)resultCode);
				var target  = (string.IsNullOrWhiteSpace(text)) ? CommonErrorContext : text;
				var context = (result.IsShowErrorCode) ? $"{target}\n[ResultCode : {(int)resultCode}({eNetErrorSourceType.MICE.ToShortWord()})]" : $"{target}";
				ShowErrorMessage(context, result.Type);
			}
			else
			{
				var context = $"{CommonErrorContext}\n[ResultCode : {(int)resultCode}({eNetErrorSourceType.MICE.ToShortWord()})]";
				ShowErrorMessage(context, eErrorMessageType.POPUP);
			}
		}

		private void ShowErrorMessage(string context, eErrorMessageType type, bool forceQuit = false)
		{
			UIManager.InstanceOrNull?.HideWaitingResponsePopup();
			if (forceQuit)
			{
				UIManager.InstanceOrNull?.ShowPopupConfirm(CommonErrorTitle, context, null, CommonErrorButton,
				                                           false, false, (guiView) =>
				                                           {
					                                           guiView.OnClosedEvent += (_) =>
					                                           {
#if UNITY_EDITOR
						                                           Com2VerseEditor.EditorApplicationUtil.ExitPlayMode();
#else
																   UnityEngine.Application.Quit();
#endif
					                                           };
				                                           });
				C2VDebug.LogError(context);
			}
			else
			{
				switch (type)
				{
					case eErrorMessageType.POPUP:
						UIManager.InstanceOrNull?.ShowPopupCommon(context);
						C2VDebug.LogError(context);
						break;
					case eErrorMessageType.TOAST_NORMAL:
						UIManager.InstanceOrNull?.SendToastMessage(context);
						C2VDebug.LogError(context);
						break;
					case eErrorMessageType.TOAST_WARNING:
						UIManager.InstanceOrNull?.SendToastMessage(context, 3f, UIManager.eToastMessageType.WARNING);
						C2VDebug.LogError(context);
						break;
					default:
						C2VDebug.Log(context);
						break;
				}
			}
		}
#endregion
	}
}
