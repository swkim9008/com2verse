/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver_Chat.cs
* Developer:	eugene9721
* Date:			2023-05-26 18:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Protocols.CommonLogic;

namespace Com2Verse.UI
{
	public partial class PacketReceiver
	{
		public event Action<WhisperChattingResponse> WhisperChattingResponse;
		public event Action<WhisperChattingNotify>   WhisperChattingNotify;

		public void RaiseWhisperChattingResponse(WhisperChattingResponse response)
		{
			WhisperChattingResponse?.Invoke(response);
		}

		public void RaiseWhisperChattingNotify(WhisperChattingNotify response)
		{
			WhisperChattingNotify?.Invoke(response);
		}

		private void DisposeChat()
		{
			WhisperChattingResponse = null;
			WhisperChattingNotify   = null;
		}
	}
}
