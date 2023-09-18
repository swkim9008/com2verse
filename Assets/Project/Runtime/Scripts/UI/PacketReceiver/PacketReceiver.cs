/*===============================================================
* Product:		Com2Verse
* File Name:	Responser.cs
* Developer:	tlghks1009
* Date:			2022-06-08 18:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Tutorial;
using Google.Protobuf;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public partial class PacketReceiver : Singleton<PacketReceiver>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private PacketReceiver() { }

		private Action<Protocols.GameLogic.LoginCom2verseResponse>      _onLoginResponseEvent;
		private Action<Protocols.OfficeMessenger.LoginOfficeResponse>   _onLoginOfficeResponseEvent;
		private Action<Protocols.GameLogic.LogOutResponse>              _onLogoutEvent;
		private Action<Protocols.GameLogic.CreateAvatarResponse>        _onCreateAvatarResponseEvent;
		private Action<Protocols.CommonLogic.UpdateAvatarResponse>      _onUpdateAvatarResponseEvent;
		private Action<Protocols.GameLogic.EnterWorldResponse>          _onEnterWorldResponseEvent;
		private Action<Protocols.GameLogic.EnterPlazaResponse>          _onEnterPlazaResponseEvent;
		private Action<Protocols.GameLogic.StandInTriggerNotify>        _onStandInTriggerNotifyEvent;
		private Action<Protocols.GameLogic.GetOffTriggerNotify>         _onGetOffTriggerNotifyEvent;
		private Action<Protocols.GameLogic.UsePortalResponse>           _onUsePortalResponseEvent;
		private Action<Protocols.CommonLogic.UseChairResponse>          _onUseChairResponseEvent;
		private Action<Protocols.GameLogic.CheckNicknameResponse>       _onCheckNicknameResponseEvent;
		private Action<Protocols.WorldState.TeleportUserStartNotify>    _onTeleportUserStartNotify;
		private Action<Protocols.WorldState.TeleportToUserFinishNotify> _onTeleportFinishNotifyEvent;

		public void OnLoginCom2VerseResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.LoginCom2verseResponse loginResponse)
			{
				C2VDebug.Log("Response loginResponse.");
				NetworkManager.Instance.ResendMessage = null;
				NetworkManager.Instance.StartPingPong();
				User.Instance.OnLoginComplete();
				_onLoginResponseEvent?.Invoke(loginResponse);
			}
		}

		public void OnLoginOfficeResponse(IMessage message)
		{
			if (message is Protocols.OfficeMessenger.LoginOfficeResponse loginResponse)
			{
				C2VDebug.Log("Response OfficeLoginResponse.");
				NetworkManager.Instance.ResendMessage = null;
				if (!User.Instance.Standby)
				{
					NetworkManager.Instance.StartPingPong();
					User.Instance.OnLoginComplete();
				}
				_onLoginOfficeResponseEvent?.Invoke(loginResponse);
			}
		}

		public void OnLogoutResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.LogOutResponse logoutResponse)
			{
				C2VDebug.Log("Response LogOutResponse.");
				_onLogoutEvent?.Invoke(logoutResponse);
			}
		}

		public void OnCreateAvatarResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.CreateAvatarResponse createAvatarResponse)
			{
				C2VDebug.Log($"Response CreateAvatar : Result : {createAvatarResponse.Result}");
				_onCreateAvatarResponseEvent?.Invoke(createAvatarResponse);
			}
		}

		public void OnUpdateAvatarResponse(IMessage message)
		{
			if (message is Protocols.CommonLogic.UpdateAvatarResponse updateAvatarResponse)
			{
				C2VDebug.Log($"Response UpdateAvatar. Result : {updateAvatarResponse.Result}");
				_onUpdateAvatarResponseEvent?.Invoke(updateAvatarResponse);
			}
		}

		public void OnEnterWorldResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.EnterWorldResponse enterWorldResponse)
			{
				C2VDebug.Log($"Response EnterWorld. Result : {enterWorldResponse.Result}");
				_onEnterWorldResponseEvent?.Invoke(enterWorldResponse);
			}
		}

		public void OnEnterPlazaResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.EnterPlazaResponse enterPlazaResponse)
			{
				C2VDebug.Log($"Response EnterPlaza. Result : {enterPlazaResponse.Result}");
				_onEnterPlazaResponseEvent?.Invoke(enterPlazaResponse);
			}
		}

		public void OnStandInTriggerNotify(IMessage message)
		{
			if (message is Protocols.GameLogic.StandInTriggerNotify standInTriggerNotify)
			{
				C2VDebug.Log($"Response StandInPortalNotify. : {standInTriggerNotify}");
				_onStandInTriggerNotifyEvent?.Invoke(standInTriggerNotify);
			}
		}

		public void OnGetOffTriggerNotify(IMessage message)
		{
			if (message is Protocols.GameLogic.GetOffTriggerNotify getOffTriggerNotify)
			{
				C2VDebug.Log($"Response GetOffPortalNotify. {getOffTriggerNotify}");
				_onGetOffTriggerNotifyEvent?.Invoke(getOffTriggerNotify);
			}
		}

		public void OnUsePortalResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.UsePortalResponse usePortalResponse)
			{
				C2VDebug.Log("Response UsePortalResponse.");
				_onUsePortalResponseEvent?.Invoke(usePortalResponse);
			}
		}

		public void OnUseChairResponse(IMessage message)
		{
			if (message is Protocols.CommonLogic.UseChairResponse useChairResponse)
			{
				C2VDebug.Log("Response UseChairResponse.");
				_onUseChairResponseEvent?.Invoke(useChairResponse);
			}
		}

		public void OnCheckNicknameResponse(IMessage message)
		{
			if (message is Protocols.GameLogic.CheckNicknameResponse checkNicknameResponse)
			{
				C2VDebug.Log("Response CheckNicknameResponse.");
				_onCheckNicknameResponseEvent?.Invoke(checkNicknameResponse);
			}
		}

		public void OnTeleportUserStartNotify(IMessage message)
		{
			if (message is Protocols.WorldState.TeleportUserStartNotify teleportUserStartNotify)
			{
				C2VDebug.Log($"Response TeleportUserStartNotify. :{teleportUserStartNotify}");
				_onTeleportUserStartNotify?.Invoke(teleportUserStartNotify);
				TutorialManager.InstanceOrNull?.StopTutorial();
			}
		}

		public void OnTeleportFinishNotify(IMessage message)
		{
			if (message is Protocols.WorldState.TeleportToUserFinishNotify teleportToUserFinishNotify)
			{
				C2VDebug.Log($"Response TeleportToUserFinishNotify. :{teleportToUserFinishNotify}");
				_onTeleportFinishNotifyEvent?.Invoke(teleportToUserFinishNotify);
			}
		}
		
		public void OnMyPadAdvertisementNotify(IMessage message)
		{
			if (message is Protocols.GameLogic.MyPadAdvertisementNotify myPadAdvertisementNotify)
			{
				C2VDebug.Log($"Response MyPadAdvertisementNotify");
				MyPadManager.Instance.SetAdvertisement(myPadAdvertisementNotify.MyPadAdvertisements);
			}
		}

        #region ResponseEvents
        public event Action<Protocols.GameLogic.LoginCom2verseResponse> OnLoginResponseEvent
		{
			add
			{
				_onLoginResponseEvent -= value;
				_onLoginResponseEvent += value;
			}
			remove => _onLoginResponseEvent -= value;
		}

		public event Action<Protocols.OfficeMessenger.LoginOfficeResponse> OnLoginOfficeResponseEvent
		{
			add
			{
				_onLoginOfficeResponseEvent -= value;
				_onLoginOfficeResponseEvent += value;
			}
			remove => _onLoginOfficeResponseEvent -= value;
		}

		public event Action<Protocols.GameLogic.LogOutResponse> OnLogoutResponseEvent
		{
			add
			{
				_onLogoutEvent -= value;
				_onLogoutEvent += value;
			}
			remove => _onLogoutEvent -= value;
		}

		public event Action<Protocols.GameLogic.CreateAvatarResponse> OnCreateAvatarResponseEvent
		{
			add
			{
				_onCreateAvatarResponseEvent -= value;
				_onCreateAvatarResponseEvent += value;
			}
			remove => _onCreateAvatarResponseEvent -= value;
		}


		public event Action<Protocols.CommonLogic.UpdateAvatarResponse> OnUpdateAvatarResponseEvent
		{
			add
			{
				_onUpdateAvatarResponseEvent -= value;
				_onUpdateAvatarResponseEvent += value;
			}
			remove => _onUpdateAvatarResponseEvent -= value;
		}

		public event Action<Protocols.GameLogic.EnterWorldResponse> OnEnterWorldResponseEvent
		{
			add
			{
				_onEnterWorldResponseEvent -= value;
				_onEnterWorldResponseEvent += value;
			}
			remove => _onEnterWorldResponseEvent -= value;
		}

		public event Action<Protocols.GameLogic.EnterPlazaResponse> OnEnterPlazaResponseEvent
		{
			add
			{
				_onEnterPlazaResponseEvent -= value;
				_onEnterPlazaResponseEvent += value;
			}
			remove => _onEnterPlazaResponseEvent -= value;
		}

        public Action<Protocols.GameLogic.StandInTriggerNotify> OnStandInTriggerNotifyEvent
		{
			set { _onStandInTriggerNotifyEvent = value; }
			get => _onStandInTriggerNotifyEvent;
		}

		public Action<Protocols.GameLogic.GetOffTriggerNotify> OnGetOffTriggerNotifyEvent
		{
			set { _onGetOffTriggerNotifyEvent = value; }
			get => _onGetOffTriggerNotifyEvent;
		}

		public Action<Protocols.GameLogic.UsePortalResponse> OnUsePortalResponseEvent
		{
			set { _onUsePortalResponseEvent = value; }
			get => _onUsePortalResponseEvent;
		}

		public event Action<Protocols.CommonLogic.UseChairResponse> OnUseChairResponseEvent
		{
			add
			{
				_onUseChairResponseEvent -= value;
				_onUseChairResponseEvent += value;
			}
			remove => _onUseChairResponseEvent -= value;
		}

		public event Action<Protocols.GameLogic.CheckNicknameResponse> OnCheckNicknameResponseEvent
		{
			add
			{
				_onCheckNicknameResponseEvent -= value;
				_onCheckNicknameResponseEvent += value;
			}
			remove => _onCheckNicknameResponseEvent -= value;
		}

		public event Action<Protocols.WorldState.TeleportUserStartNotify> OnTeleportUserStartNotifyEvent
		{
			add
			{
				_onTeleportUserStartNotify -= value;
				_onTeleportUserStartNotify += value;
			}
			remove => _onTeleportUserStartNotify -= value;
		}

		public event Action<Protocols.WorldState.TeleportToUserFinishNotify> OnFieldMoveNotifyEvent
		{
			add
			{
				_onTeleportFinishNotifyEvent -= value;
				_onTeleportFinishNotifyEvent += value;
			}
			remove => _onTeleportFinishNotifyEvent -= value;
		}
#endregion ResponseEvents

		public void Dispose()
		{
			_onLoginResponseEvent         = null;
			_onLoginOfficeResponseEvent   = null;
			_onLogoutEvent                = null;
			_onCreateAvatarResponseEvent  = null;
			_onUpdateAvatarResponseEvent  = null;
			_onEnterWorldResponseEvent    = null;
			_onEnterPlazaResponseEvent    = null;
			_onStandInTriggerNotifyEvent  = null;
			_onGetOffTriggerNotifyEvent   = null;
			_onUsePortalResponseEvent     = null;
			_onUseChairResponseEvent      = null;
			_onCheckNicknameResponseEvent = null;
			DisposeMice();
			DisposeChat();
		}
	}
}
