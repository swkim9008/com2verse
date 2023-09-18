// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	Commander_TextBoard.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-17 오전 11:42
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Protocols.GameLogic;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public void RequestTextBoardUpdate(long boardSequence, string text, int alignment, int textColor, int backgroundImage)
		{
			UpdateTextBoardRequest updateTextBoardRequest = new()
			{
				BoardId = boardSequence,
				Message = text,
				Alignment = alignment,
				Color = textColor,
				Background = backgroundImage
			};
			LogPacketSend(updateTextBoardRequest.ToString());
			NetworkManager.Instance.Send(updateTextBoardRequest, MessageTypes.UpdateTextBoardRequest);
		}
	}
}
