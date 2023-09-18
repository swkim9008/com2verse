/*===============================================================
* Product:    Com2Verse
* File Name:  AppInitializer.cs
* Developer:  jehyun
* Date:       2022-02-22 14:58
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;
using UnityEngine;

namespace Com2Verse
{
	/// <summary>
	/// 프로젝트 최초 진입점 관리.
	/// <br/>싱글턴 인스턴스 초기화 방식은 <see cref="SingletonDefine"/> 에서 정의 가능하다.
	/// <br/>다음 문서를 참고.
	/// <br/><a href="https://jira.com2us.com/wiki/pages/viewpage.action?pageId=352176705">Confluence - 앱 초기화 순서 정리</a>
	/// <br/><a href="https://docs.unity3d.com/kr/2021.3/Manual/ExecutionOrder.html">Unity Execution Order</a>
	/// <br/><a href="https://docs.unity3d.com/kr/2021.3/Manual/DomainReloading.html">Domain Reloading</a>
	/// </summary>
	/// <remarks>
	/// Domain reloading 을 활성화 하면 PlayMode 진입이 느려지기 때문에 해당 프로젝트에서는 비활성화 되어있다.
	/// <br/>하지만 직접 정의한  static 필드나, 외부 패키지의 static 필드에 의존하는 값은 직접 초기화 작업이 필요하다.
	/// <br/>따라서 가능하면 Singleton 을 상속받아 자동으로 생명 주기가 관리되게 하는 것을 권장한다.
	/// </remarks>
	public static class AppInitializer
	{
#region Initialize
		static AppInitializer()
		{
			LogMethodInvocation();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void OnSubsystemRegistration()
		{
			LogMethodInvocation();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void OnAfterAssembliesLoaded()
		{
			LogMethodInvocation();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void OnBeforeSplashScreen()
		{
			LogMethodInvocation();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnBeforeSceneLoad()
		{
			LogMethodInvocation();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void OnAfterSceneLoad()
		{
			LogMethodInvocation();

			// BeforeSceneLoad 이전 타이밍에 초기화 진행 시 UniTask, GameObject 생명 주기 관리에 문제가 발생하기 때문에 AfterSceneLoad 시점에 초기화.
			SceneInitializer.CreateInstance();
		}
#endregion // Initialize

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough]
		private static void LogMethodInvocation([CallerMemberName] string? caller = null)
		{
			UnityEngine.Debug.Log(ZString.Format("<b>[{0}] {1}</b>", nameof(AppInitializer), caller));
		}
#endregion // Debug
	}
}
