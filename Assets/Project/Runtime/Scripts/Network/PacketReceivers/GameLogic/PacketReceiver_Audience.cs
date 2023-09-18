// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver_Audience.cs
//  * Developer:	yangsehoon
//  * Date:		2023-07-07 오전 10:33
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Logger;
using Protocols.GameLogic;

namespace Com2Verse.Network.GameLogic
{
	public partial class PacketReceiver
	{
		public event Action<EnterAudioMuxMicAreaNotify>   EnterAudioMuxMicAreaNotify;
		public event Action<EnterAudioMuxCrowdAreaNotify> EnterAudioMuxCrowdAreaNotify;

		public void RaiseEnterAudioMuxMicAreaNotify(EnterAudioMuxMicAreaNotify response)
		{
			C2VDebug.LogCategory("Audience", $"EnterAudioMuxMicAreaNotify {response.RoomId}");
			EnterAudioMuxMicAreaNotify?.Invoke(response);
		}

		public void RaiseEnterAudioMuxCrowdAreaNotify(EnterAudioMuxCrowdAreaNotify response)
		{
			C2VDebug.LogCategory("Audience", $"EnterAudioMuxCrowdAreaNotify {response.RoomId}");
			EnterAudioMuxCrowdAreaNotify?.Invoke(response);
		}
		
		private void DisposeAudience()
		{
			EnterAudioMuxMicAreaNotify   = null;
			EnterAudioMuxCrowdAreaNotify = null;
		}
	}
}
