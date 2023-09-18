/*===============================================================
* Product:		Com2Verse
* File Name:	IMessageProcessor.cs
* Developer:	haminjeong
* Date:			2022-06-14 13:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Google.Protobuf;

namespace Com2Verse.Network
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ChannelAttribute : Attribute
	{
		public Protocols.Channels Channel { get; }

		public ChannelAttribute(Protocols.Channels channel) => Channel = channel;
	}

	public interface IMessageProcessor
	{
		protected Dictionary<int, Action<IMessage>> MessageHandlers { get; }
		protected Dictionary<int, Func<ArraySegment<byte>, IMessage>> MessageParsers { get; }


		/// <summary>
		/// 메시지 핸들러를 등록(SetMessageProcessCallback)하는 메소드들을 담는 메소드
		/// </summary>
		public void Initialize();

		/// <summary>
		/// 메시지 핸들러를 command에 따라 검색하여 실행하는 메소드. NetworkManager에서 호출된다.
		/// </summary>
		/// <param name="channel">메시지 Channel</param>
		/// <param name="command">메시지 Command</param>
		/// <param name="message">메시지 페이로드(Protobuf)</param>
		public void Process(Protocols.Channels channel, int command, IMessage message)
		{
			if (!MessageHandlers.TryGetValue(command, out var action))
			{
				C2VDebug.LogError($"Invalid message: {channel.ToString()} - {command}");
				return;
			}

			action?.Invoke(message);
		}

		/// <summary>
		/// 메시지 파서를 command에 따라 검색하여 실행하는 메소드. NetworkManager에서 호출된다.
		/// </summary>
		/// <param name="command">메시지 Command</param>
		/// <param name="payload">파싱 전 메시지 페이로드(byte[])</param>
		/// <returns>파싱된 메시지</returns>
		public IMessage Parse(int command, ArraySegment<byte> payload)
		{
			if (!MessageParsers.TryGetValue(command, out var func))
			{
				C2VDebug.LogError($"Invalid message: {command}");
				return null;
			}

			return func(payload);
		}

		/// <summary>
		/// 메시지 에러코드가 Success가 아니면 실행되는 메소드. NetworkManager에서 호출된다.
		/// </summary>
		/// <param name="channel">메시지 Channel</param>
		/// <param name="command">메시지 Command</param>
		/// <param name="errorCode">메시지 ErrorCode</param>
		public void ErrorProcess(Protocols.Channels channel, int command, Protocols.ErrorCode errorCode);
	}
}
