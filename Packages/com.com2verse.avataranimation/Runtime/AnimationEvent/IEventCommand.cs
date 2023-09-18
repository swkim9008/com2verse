/*===============================================================
* Product:		Com2Verse
* File Name:	IEventCommand.cs
* Developer:	eugene9721
* Date:			2022-11-17 10:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.AvatarAnimation
{
	/// <summary>
	/// 애니메이션 클립 이벤트를 처리하기 위한 인터페이스.
	/// 애니메이션 이벤트가 실행될 프레임에 지연이 발생한 경우
	/// 해당 이벤트가 실행되지 않을 수 있으니 주의.
	/// 해당 문서 참조: <a href="https://forum.unity.com/threads/animation-event-cant-work-in-low-frame-rate-help.58302/">"Animation Event can't work in low frame rate...Help....."</a>
	/// </summary>
	public interface IEventCommand
	{
		[UsedImplicitly] void PlayFX(string assetName);

		[UsedImplicitly] void PlaySound(string assetName);
	}
}
