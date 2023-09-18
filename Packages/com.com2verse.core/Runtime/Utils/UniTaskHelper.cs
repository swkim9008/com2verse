/*===============================================================
* Product:		Com2Verse
* File Name:	UniTaskHelper.cs
* Developer:	urun4m0r1
* Date:			2022-07-04 16:30
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Utils
{
	public static class UniTaskHelper
	{
#region ContextSwitch
		/// <inheritdoc cref="RunOnMainThread(Cysharp.Threading.Tasks.UniTask)"/>
		public static async UniTask<bool> InvokeOnMainThread(Action? action)
		{
			return await action.RunOnMainThread();
		}

		/// <inheritdoc cref="RunOnMainThread(Cysharp.Threading.Tasks.UniTask)"/>
		public static async UniTask<bool> RunOnMainThread(this Action? action)
		{
			return await action.RunOnMainThread(CoroutineManager.Instance.GlobalCancellationTokenSource);
		}

		/// <summary>
		/// Boxing을 발생시키지 않고 메인 스레드에서 작업을 수행하는 UniTask를 반환합니다.<br/>
		/// 중간에 취소할 수 없습니다.<br/>
		/// 실행 시점의 SynchronizationContext를 유지합니다.<br/>
		/// </summary>
		/// <remarks>
		/// <b>주의</b>: 해당 확장 메서드를 이용시 반드시 <see cref="UniTask.Defer"/> 로 UniTask를 감싸줘야 합니다.
		/// </remarks>
		/// <returns>
		/// <b>true</b>: 성공적으로 메인 스레드에서 작업이 완료된 경우<br/>
		/// <b>false</b>: 앱이 종료되는 도중 등 비정상적인 타이밍에 호출.
		/// </returns>
		public static async UniTask<bool> RunOnMainThread(this UniTask task)
		{
			return await task.RunOnMainThread(CoroutineManager.Instance.GlobalCancellationTokenSource);
		}

		/// <inheritdoc cref="RunOnMainThread(Cysharp.Threading.Tasks.UniTask,System.Threading.CancellationTokenSource?)"/>
		public static async UniTask<bool> InvokeOnMainThread(Action? action, CancellationTokenSource? tokenSource)
		{
			return await action.RunOnMainThread(tokenSource);
		}

		/// <inheritdoc cref="RunOnMainThread(Cysharp.Threading.Tasks.UniTask,System.Threading.CancellationTokenSource?)"/>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> RunOnMainThread(this Action? action, CancellationTokenSource? tokenSource)
		{
			var context = SynchronizationContext.Current;
			if (!await TrySwitchToMainThread(context, tokenSource))
				return false;

			action?.Invoke();

			await TrySwitchToSynchronizationContext(context);
			return true;
		}

		/// <summary>
		/// Boxing을 발생시키지 않고 메인 스레드에서 작업을 수행하는 UniTask를 반환합니다.<br/>
		/// 중간에 CancellationToken을 사용하여 취소할 수 있습니다.<br/>
		/// 실행 시점의 SynchronizationContext를 유지합니다.
		/// </summary>
		/// <remarks>
		/// <b>주의</b>: 해당 확장 메서드를 이용시 반드시 <see cref="UniTask.Defer"/> 로 UniTask를 감싸줘야 합니다.
		/// </remarks>
		/// <returns>
		/// <b>true</b>: 성공적으로 메인 스레드에서 작업이 완료된 경우<br/>
		/// <b>false</b>: Cancel(), Dispose()가 호출되어 취소된 경우, tokenSource가 null인 경우.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> RunOnMainThread(this UniTask task, CancellationTokenSource? tokenSource)
		{
			var context = SynchronizationContext.Current;
			if (!await TrySwitchToMainThread(context, tokenSource))
				return false;

			await task;

			await TrySwitchToSynchronizationContext(context);
			return true;
		}

		/// <summary>
		/// 안전하게 Context를 메인 스레드로 전환하는 UniTask를 반환합니다.<br/>
		/// 중간에 CancellationToken을 사용하여 취소할 수 있습니다.<br/>
		/// 전환 실패시 이전 Context로 되돌아갑니다.
		/// </summary>
		/// <returns>
		/// <b>true</b>: 성공적으로 메인 스레드로 전환된 경우<br/>
		/// <b>false</b>: Cancel(), Dispose()가 호출되어 취소된 경우, tokenSource가 null인 경우.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> TrySwitchToMainThread(SynchronizationContext? context, CancellationTokenSource? tokenSource)
		{
			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			await UniTask.SwitchToMainThread(PlayerLoopTiming.Update, tokenSource.Token);

			if (tokenSource == null || tokenSource.IsCancellationRequested)
			{
				await TrySwitchToSynchronizationContext(context);
				return false;
			}

			return true;
		}

		/// <summary>
		/// 안전하게 Context를 주어진 스레드로 전환하는 UniTask를 반환합니다.
		/// </summary>
		/// <returns>
		/// <b>true</b>: 성공적으로 주어진 스레드로 전환된 경우<br/>
		/// <b>false</b>: context가 null인 경우.
		/// </returns>
		public static async UniTask<bool> TrySwitchToSynchronizationContext(SynchronizationContext? context)
		{
			if (context == null)
				return false;

			await UniTask.SwitchToSynchronizationContext(context);
			return true;
		}
#endregion // ContextSwitch

#region Delay
		/// <summary>
		/// 조건이 true가 될때까지 기다리는 UniTask를 반환합니다.<br/>
		/// 중간에 취소할 수 없습니다.<br/>
		/// </summary>
		/// <returns>
		/// <b>true</b>: 값이 true로 변한 경우<br/>
		/// <b>false</b>: 앱이 종료되는 도중 등 비정상적인 타이밍에 호출.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return await WaitUntil(predicate, CoroutineManager.Instance.GlobalCancellationTokenSource, timing);
		}

		/// <summary>
		/// 조건이 true가 될때까지 기다리는 UniTask를 반환합니다.<br/>
		/// 중간에 CancellationToken을 사용하여 취소할 수 있습니다.<br/>
		/// </summary>
		/// <returns>
		/// <b>true</b>: 값이 true로 변한 경우<br/>
		/// <b>false</b>: 앱이 종료되는 도중 등 비정상적인 타이밍에 호출.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> WaitUntil(Func<bool> predicate, CancellationTokenSource? tokenSource, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			await UniTask.WaitUntil(predicate, timing, tokenSource.Token).SuppressCancellationThrow();

			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			return true;
		}

		/// <summary>
		/// 일정 시간동안 딜레이를 주는 UniTask를 반환합니다.<br/>
		/// 중간에 취소할 수 없습니다.<br/>
		/// </summary>
		/// <returns>
		/// <b>true</b>: 성공적으로 딜레이가 적용된 경우<br/>
		/// <b>false</b>: 앱이 종료되는 도중 등 비정상적인 타이밍에 호출.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> Delay(int delayMs, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return await Delay(delayMs, CoroutineManager.Instance.GlobalCancellationTokenSource, timing);
		}

		/// <summary>
		/// 일정 시간동안 딜레이를 주는 UniTask를 반환합니다.<br/>
		/// 중간에 CancellationToken을 사용하여 취소할 수 있습니다.<br/>
		/// </summary>
		/// <returns>
		/// <b>true</b>: 성공적으로 딜레이가 적용된 경우<br/>
		/// <b>false</b>: Cancel(), Dispose()가 호출되어 취소된 경우, tokenSource가 null인 경우.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> Delay(int delayMs, CancellationTokenSource? tokenSource, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			await UniTask.Delay(delayMs, DelayType.UnscaledDeltaTime, timing, tokenSource.Token).SuppressCancellationThrow();

			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			return true;
		}

		/// <summary>
		/// 일정 프레임동안 딜레이를 주는 UniTask를 반환합니다.<br/>
		/// 중간에 취소할 수 없습니다.<br/>
		/// </summary>
		/// <returns>
		/// <b>true</b>: 성공적으로 딜레이가 적용된 경우<br/>
		/// <b>false</b>: 앱이 종료되는 도중 등 비정상적인 타이밍에 호출.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> DelayFrame(int delayFrame, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return await DelayFrame(delayFrame, CoroutineManager.Instance.GlobalCancellationTokenSource, timing);
		}

		/// <summary>
		/// 일정 프레임동안 딜레이를 주는 UniTask를 반환합니다.<br/>
		/// 중간에 CancellationToken을 사용하여 취소할 수 있습니다.
		/// </summary>
		/// <returns>
		/// <b>true</b>: 성공적으로 딜레이가 적용된 경우<br/>
		/// <b>false</b>: Cancel(), Dispose()가 호출되어 취소된 경우, tokenSource가 null인 경우.
		/// </returns>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
		public static async UniTask<bool> DelayFrame(int delayFrame, CancellationTokenSource? tokenSource, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			await UniTask.DelayFrame(delayFrame, timing, tokenSource.Token).SuppressCancellationThrow();

			if (tokenSource == null || tokenSource.IsCancellationRequested)
				return false;

			return true;
		}
#endregion // Delay
	}
}
