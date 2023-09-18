/*===============================================================
* Product:		Com2Verse
* File Name:	IRefreshable.cs
* Developer:	urun4m0r1
* Date:			2022-09-02 11:02
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication
{
	public interface IRefreshable
	{
		UniTask Refresh(CancellationTokenSource? tokenSource);

		bool UseAutoRefresh { get; set; }

		int RefreshInterval { get; set; }
	}
}
