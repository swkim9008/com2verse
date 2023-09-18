// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver_ServiceChange.cs
//  * Developer:	haminjeong
//  * Date:		2023-07-01 오후 6:06
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Loading;
using Com2Verse.UI;
using Protocols.CommonLogic;

namespace Com2Verse.Network.CommonLogic
{
	public partial class PacketReceiver
	{
		public event Action<UserConnectableCheckResponse>  UserConnectableCheckResponse;
		public event Action<ConnectQueueResponse>          ConnectQueueResponse;
		public event Action<UserInacceptableWorldNotify>   UserInacceptableWorldNotify;
		public event Action<UserAcceptConnectWorldNotify>  UserAcceptConnectWorldNotify;
		public event Action<EscapeAcceptableCheckResponse> EscapeAcceptableCheckResponse;

		public void RaiseUserConnectableCheckResponse(UserConnectableCheckResponse response)
		{
			UserConnectableCheckResponse?.Invoke(response);
		}

		public void RaiseConnectQueueResponse(ConnectQueueResponse response)
		{
			ConnectQueueResponse?.Invoke(response);
		}

		public void RaiseUserAcceptConnectWorldNotify(UserAcceptConnectWorldNotify response)
		{
			UserAcceptConnectWorldNotify?.Invoke(response);
		}

		public void RaiseUserInacceptableWorldNotify(UserInacceptableWorldNotify response)
		{
			UserInacceptableWorldNotify?.Invoke(response);
		}

		public void RaiseEscapeAcceptableCheckResponse(EscapeAcceptableCheckResponse response)
		{
			EscapeAcceptableCheckResponse?.Invoke(response);
		}

		private void DisposeConnectQueue()
		{
			UserConnectableCheckResponse  = null;
			ConnectQueueResponse          = null;
			UserInacceptableWorldNotify   = null;
			UserAcceptConnectWorldNotify  = null;
			EscapeAcceptableCheckResponse = null;
		}
	}
}
