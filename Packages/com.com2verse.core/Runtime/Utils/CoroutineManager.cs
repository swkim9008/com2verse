/*===============================================================
* Product:		Com2Verse
* File Name:	CoroutineManager.cs
* Developer:	urun4m0r1
* Date:			2022-05-30 10:13
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Threading;

namespace Com2Verse.Utils
{
	public sealed class CoroutineManager : MonoSingleton<CoroutineManager>
	{
		public CancellationTokenSource? GlobalCancellationTokenSource { get; private set; } = new();

		protected override void OnDestroyInvoked()         => DisposeAppTokenSource();
		protected override void OnApplicationQuitInvoked() => DisposeAppTokenSource();

		private void DisposeAppTokenSource()
		{
			GlobalCancellationTokenSource?.Cancel();
			GlobalCancellationTokenSource?.Dispose();
			GlobalCancellationTokenSource = null;
		}
	}
}
