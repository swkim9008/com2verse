/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObject_Conference.cs
* Developer:	wlemon
* Date:			2023-08-04 10:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Network
{
	public partial class ActiveObject
	{
		private eConferenceObjectType _conferenceObjectType = eConferenceObjectType.NONE;

		public eConferenceObjectType ConferenceObjectType => _conferenceObjectType;

		public void RefreshConferenceObjectType()
		{
			var tagValue = GetStringFromTags(TagDefine.Key.ConferenceObjectType);
			if (tagValue == null)
			{
				_conferenceObjectType = eConferenceObjectType.NONE;
				return;
			}

			switch (tagValue)
			{
				case TagDefine.Value.Listener:
					_conferenceObjectType = eConferenceObjectType.LISTENER;
					break;
				case TagDefine.Value.Speaker:
					_conferenceObjectType = eConferenceObjectType.SPEAKER;
					break;
				default:
					_conferenceObjectType = eConferenceObjectType.NONE;
					break;
			}
		}
	}
}
