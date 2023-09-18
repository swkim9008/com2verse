/*===============================================================
 * Product:		Com2Verse
 * File Name:	ConnectionControllerExtension.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-27 12:29
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Cysharp.Threading.Tasks;
using static Com2Verse.Communication.eConnectionState;

namespace Com2Verse.Communication
{
	/// <summary>
	/// <see cref="IConnectionController"/>의 확장 메서드를 제공합니다.
	/// </summary>
	public static class ConnectionControllerExtension
	{
		/// <summary>
		/// 비동기로 연결을 시도합니다.
		/// </summary>
		public static void Connect(this IConnectionController controller)
		{
			controller.ConnectAsync().Forget();
		}

		/// <summary>
		/// 비동기로 연결을 끊습니다.
		/// </summary>
		public static void Disconnect(this IConnectionController controller)
		{
			controller.DisconnectAsync().Forget();
		}

		/// <summary>
		/// 비동기로 재접속을 시도합니다.
		/// </summary>
		public static void Reconnect(this IConnectionController controller)
		{
			controller.ReconnectAsync().Forget();
		}

		/// <summary>
		/// 비동기로 연결을 시도합니다.
		/// <br/>이미 연결되어 있는 경우에는 아무런 동작을 하지 않습니다.
		/// </summary>
		public static void TryConnect(this IConnectionController controller)
		{
			if (controller.State is CONNECTED or CONNECTING)
				return;

			controller.Connect();
		}

		/// <summary>
		/// 비동기로 연결을 끊습니다.
		/// <br/>이미 끊어져 있는 경우에는 아무런 동작을 하지 않습니다.
		/// </summary>
		public static void TryDisconnect(this IConnectionController controller)
		{
			if (controller.State is DISCONNECTED or DISCONNECTING)
				return;

			controller.Disconnect();
		}

		/// <summary>
		/// 비동기로 재접속을 시도합니다.
		/// <br/>연결되어 있지 않은 경우에는 아무런 동작을 하지 않습니다.
		/// </summary>
		public static void TryReconnect(this IConnectionController controller)
		{
			if (controller.State is DISCONNECTED or DISCONNECTING)
				return;

			controller.Reconnect();
		}

		/// <summary>
		/// 비동기로 연결을 시도합니다.
		/// <br/>이미 연결되어 있는 경우에는 재접속을 시도합니다.
		/// </summary>
		public static void TryForceConnect(this IConnectionController controller)
		{
			if (controller.State is CONNECTED or CONNECTING)
				controller.Reconnect();
			else
				controller.Connect();
		}

		/// <summary>
		/// 재접속을 시도합니다.
		/// </summary>
		/// <returns>
		/// 재접속에 성공하면 true, 실패하면 false를 반환합니다.
		/// </returns>
		public static async UniTask<bool> ReconnectAsync(this IConnectionController controller)
		{
			if (!await controller.DisconnectAsync())
				return false;

			return await controller.ConnectAsync();
		}
	}
}
