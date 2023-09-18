/*===============================================================
 * Product:		Com2Verse
 * File Name:	ILocalMediaTrack.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-15 15:11
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 연결 상태를 제어하는 인터페이스를 제공합니다.
	/// </summary>
	public interface IConnectionController
	{
		/// <summary>
		/// 연결 상태가 변경될 때 발생합니다.
		/// </summary>
		event Action<IConnectionController, eConnectionState>? StateChanged;

		/// <summary>
		/// 연결 상태를 가져옵니다.
		/// </summary>
		eConnectionState State { get; }

		/// <summary>
		/// 연결을 시도합니다.
		/// </summary>
		/// <returns>
		/// 연결에 성공하거나 이미 연결되어 있으면 true, 실패하면 false를 반환합니다.
		/// </returns>
		UniTask<bool> ConnectAsync();

		/// <summary>
		/// 연결을 끊습니다.
		/// </summary>
		/// <returns>
		/// 연결이 끊어지거나 이미 끊어져 있으면 true, 실패하면 false를 반환합니다.
		/// </returns>
		UniTask<bool> DisconnectAsync();

		string GetDebugInfo();
	}
}
