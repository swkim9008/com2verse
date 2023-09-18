/*===============================================================
* Product:		Com2Verse
* File Name:	DisableURPDebugUpdater.cs
* Developer:	urun4m0r1
* Date:			2022-05-13 17:24
* History:		
* Documents:	https://forum.unity.com/threads/errors-with-the-urp-debug-manager.987795/
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

#if !UNITY_EDITOR
using UnityEngine.Rendering;
#endif

namespace Com2Verse
{
	/// <inheritdoc />
	/// <summary>
	/// 이 파일은 특정 URP 버전에서 Windows Standalone 개발 빌드 실행시 대량으로 발생되는 로깅 오류를 억제합니다.
	/// 해당 스크립트를 앱 초기화시 아무 GameObject에 할당하면 됩니다.
	/// Unity 2022.3.1f1 까지 해당 오류가 발생되는것으로 확인되었습니다.
	/// </summary>
	public sealed class DisableUrpDebugUpdater : MonoBehaviour
	{
		private void Awake()
		{
#if !UNITY_EDITOR
		if (DebugManager.instance != null) DebugManager.instance.enableRuntimeUI = false;
#endif
		}
	}
}
