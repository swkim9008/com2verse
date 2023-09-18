/*===============================================================
 * Product:		Com2Verse
 * File Name:	IRemoteTrackPublishRequester.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-09 20:14
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <summary>
	/// TODO:
	/// <br/>
	/// SmallTalk 에서는 View가 아닌 오직 거리에 의해 Request/Refuse 를 결정하기떄문에<br/>
	/// 영상이 시야에 보이지 않게 되어도 상대는 계속 Publish를 유지하는 상태다<br/>
	/// 범위 안에있는 유저여도 영상이 보이지 않는다면 최적화가 필요하기 때문에 해당 인터페이스 혹은 RemoteUser 구조 개선이 필요하다.<br/>
	/// 시야/거리/인원수 등 다양한 요인이 존재할 수 있다.
	/// </summary>
	public interface IRemoteTrackPublishRequester
	{
		string GetDebugInfo();
	}
}
