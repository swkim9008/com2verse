/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver_Mice.cs
* Developer:	ikyoung
* Date:			2023-04-12 13:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Google.Protobuf;

namespace Com2Verse.UI
{
	public partial class PacketReceiver
	{
		private Action<Protocols.Mice.EnterLobbyResponse>  _onEnterMiceLobbyResponseEvent;
		private Action<Protocols.Mice.EnterLoungeResponse> _onEnterMiceLoungeResponseEvent;
		private Action<Protocols.Mice.EnterFreeLoungeResponse> _onEnterMiceFreeLoungeResponseEvent;
		private Action<Protocols.Mice.EnterHallResponse> _onEnterMiceHallResponseEvent;
		private Action<Protocols.Mice.MiceRoomNotify> _onMiceRoomNotifyEvent;
		private Action<Protocols.Mice.ForcedMiceTeleportNotify> _onForcedMiceTeleportNotifyEvent;
		
		public void OnEnterMiceLobbyResponse(IMessage message)
		{
			if (message is Protocols.Mice.EnterLobbyResponse response)
			{
				C2VDebug.Log($"Response EnterLobbyResponse. :{response}");
				_onEnterMiceLobbyResponseEvent?.Invoke(response);
			}
		}
		public void OnEnterMiceLoungeResponse(IMessage message)
		{
			if (message is Protocols.Mice.EnterLoungeResponse response)
			{
				C2VDebug.Log($"Response EnterLoungeResponse. :{response} {response.EnterRequestResult.ToString()}");
				_onEnterMiceLoungeResponseEvent?.Invoke(response);
			}
		}
		public void OnEnterMiceFreeLoungeResponse(IMessage message)
		{
			if (message is Protocols.Mice.EnterFreeLoungeResponse response)
			{
				C2VDebug.Log($"Response EnterFreeLoungeResponse. :{response} {response.EnterRequestResult.ToString()}");
				_onEnterMiceFreeLoungeResponseEvent?.Invoke(response);
			}
		}
		public void OnEnterMiceHallResponse(IMessage message)
		{
			if (message is Protocols.Mice.EnterHallResponse response)
			{
				C2VDebug.Log($"Response EnterHallResponse. :{response} {response.EnterRequestResult.ToString()}");
				_onEnterMiceHallResponseEvent?.Invoke(response);
			}
		}
		
		public void OnMiceRoomNotify(IMessage message)
		{
			if (message is Protocols.Mice.MiceRoomNotify response)
			{
				C2VDebug.Log($"Response OnMiceRoomNotify. :{response}");
				_onMiceRoomNotifyEvent?.Invoke(response);
			}
		}
		
		public void OnForcedMiceTeleportNotify(IMessage message)
		{
			if (message is Protocols.Mice.ForcedMiceTeleportNotify response)
			{
				C2VDebug.Log($"Response OnForcedMiceTeleportNotify. :{response}");
				_onForcedMiceTeleportNotifyEvent?.Invoke(response);
			}
		}
		public event Action<Protocols.Mice.EnterLobbyResponse> OnEnterMiceLobbyResponseEvent
		{
			add
			{
				_onEnterMiceLobbyResponseEvent -= value;
				_onEnterMiceLobbyResponseEvent += value;
			}
			remove => _onEnterMiceLobbyResponseEvent -= value;
		}
		public event Action<Protocols.Mice.EnterLoungeResponse> OnEnterMiceLoungeResponseEvent
		{
			add
			{
				_onEnterMiceLoungeResponseEvent -= value;
				_onEnterMiceLoungeResponseEvent += value;
			}
			remove => _onEnterMiceLoungeResponseEvent -= value;
		}
		
		public event Action<Protocols.Mice.EnterFreeLoungeResponse> OnEnterMiceFreeLoungeResponseEvent
		{
			add
			{
				_onEnterMiceFreeLoungeResponseEvent -= value;
				_onEnterMiceFreeLoungeResponseEvent += value;
			}
			remove => _onEnterMiceFreeLoungeResponseEvent -= value;
		}
		
		public event Action<Protocols.Mice.EnterHallResponse> OnEnterMiceHallResponseEvent
		{
			add
			{
				_onEnterMiceHallResponseEvent -= value;
				_onEnterMiceHallResponseEvent += value;
			}
			remove => _onEnterMiceHallResponseEvent -= value;
		}
		public event Action<Protocols.Mice.MiceRoomNotify> OnMiceRoomNotifyEvent
		{
			add
			{
				_onMiceRoomNotifyEvent -= value;
				_onMiceRoomNotifyEvent += value;
			}
			remove => _onMiceRoomNotifyEvent -= value;
		}
		public event Action<Protocols.Mice.ForcedMiceTeleportNotify> OnForcedMiceTeleportNotifyEvent
		{
			add
			{
				_onForcedMiceTeleportNotifyEvent -= value;
				_onForcedMiceTeleportNotifyEvent += value;
			}
			remove => _onForcedMiceTeleportNotifyEvent -= value;
		}
		private void DisposeMice()
		{
			_onEnterMiceLobbyResponseEvent = null;
			_onEnterMiceLoungeResponseEvent = null;
			_onEnterMiceFreeLoungeResponseEvent = null;
			_onEnterMiceHallResponseEvent = null;
			_onMiceRoomNotifyEvent = null;
			_onForcedMiceTeleportNotifyEvent = null;
		}
	}
}
