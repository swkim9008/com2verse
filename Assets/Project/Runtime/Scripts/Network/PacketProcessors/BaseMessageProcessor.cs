/*===============================================================
* Product:		Com2Verse
* File Name:	BaseMessageProcessor.cs
* Developer:	haminjeong
* Date:			2023-01-05 14:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.UI;
using Google.Protobuf;

namespace Com2Verse.Network
{
	public abstract class BaseMessageProcessor : IMessageProcessor
	{
		private readonly Dictionary<int, Action<IMessage>> _messageHandlers = new();
		private readonly Dictionary<int, Func<ArraySegment<byte>, IMessage>> _messageParsers = new();

		public abstract void Initialize();

		protected void SetMessageProcessCallback(int command, Func<ArraySegment<byte>, IMessage> parser, Action<IMessage> handler)
		{
			_messageParsers.Add(command, parser);
			_messageHandlers.Add(command, handler);
		}

		public virtual void ErrorProcess(Protocols.Channels channel, int command, Protocols.ErrorCode errorCode)
		{
			UIManager.Instance.HideWaitingResponsePopup();
			NetworkUIManager.Instance.ShowProtocolErrorMessage(errorCode);
			switch (errorCode)
			{
				case Protocols.ErrorCode.DbError:
					break;
				case Protocols.ErrorCode.DbException:
					break;
				case Protocols.ErrorCode.OverUserCount:
					break;
				case Protocols.ErrorCode.AlreadyExistsRoom:
					break;
				default:
					throw new ApplicationException($"Channel {channel.ToString()}, Command {command}, ErrorCode {errorCode}");
			}
		}

		Dictionary<int, Action<IMessage>> IMessageProcessor.MessageHandlers => _messageHandlers;

		Dictionary<int, Func<ArraySegment<byte>, IMessage>> IMessageProcessor.MessageParsers => _messageParsers;
	}
}
