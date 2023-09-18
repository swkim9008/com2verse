/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver_AreaChange.cs
* Developer:	ksw
* Date:			2023-05-11 10:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Protocols.GameLogic;

namespace Com2Verse.Network.GameLogic
{
	public partial class PacketReceiver
	{
		public event Action<EnterChattingAreaNotify> EnterChattingAreaNotify;
		public event Action<ExitChattingAreaNotify>  ExitChattingAreaNotify;

		public void RaiseEnterChattingAreaNotify(EnterChattingAreaNotify response)
		{
			C2VDebug.Log("EnterChattingAreaNotify");
			EnterChattingAreaNotify?.Invoke(response);
		}

		public void RaiseExitChattingAreaNotify(ExitChattingAreaNotify response)
		{
			C2VDebug.Log("ExitChattingAreaNotify");
			ExitChattingAreaNotify?.Invoke(response);
		}

		private void DisposeAreaChange()
		{
			EnterChattingAreaNotify = null;
			ExitChattingAreaNotify  = null;
		}
	}
}
