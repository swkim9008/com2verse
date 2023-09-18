// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	Commander_Maze.cs
//  * Developer:	haminjeong
//  * Date:		2023-06-27 오후 12:36
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Contents;
using Com2Verse.PlayerControl;

namespace Com2Verse.Network
{
	public partial class Commander
	{
		public void StartMaze(string guid, long time)
		{
			Protocols.GameLogic.EnterMazeRequest request = new()
			{
				SessionKey = guid,
				Totalmiliseconds = time,
			};
			NetworkManager.Instance.Send(request, Protocols.GameLogic.MessageTypes.EnterMazeRequest);
		}

		public void ResultMaze(string guid, long time, Action onTimeoutAction)
		{
			Protocols.GameLogic.ExitMazeRequest request = new()
			{
				SessionKey = guid,
				Totalmiliseconds = time,
			};
			NetworkManager.Instance.Send(request,
			                             Protocols.GameLogic.MessageTypes.ExitMazeRequest,
			                             Protocols.Channels.GameLogic,
			                             (int)Protocols.GameLogic.MessageTypes.ExitMazeResponse,
			                             10000, onTimeoutAction);
		}

		public void EscapeMaze()
		{
			PlayContentsManager.Instance.ContentsEnd();
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
			Protocols.GameLogic.EscapeMazeRequest request = new();
			NetworkManager.Instance.Send(request, Protocols.GameLogic.MessageTypes.EscapeMazeRequest);
		}
	}
}
