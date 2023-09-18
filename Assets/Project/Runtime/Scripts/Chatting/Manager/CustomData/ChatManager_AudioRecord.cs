/*===============================================================
* Product:		Com2Verse
* File Name:	ChatManager_AudioRecord.cs
* Developer:	ksw
* Date:			2023-06-20 17:41
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using JetBrains.Annotations;

namespace Com2Verse.Chat
{
	public sealed partial class ChatManager
	{
		public event Action<long> OnAudioRecord;

		private void ProcessCustomDataAudioRecord(CustomDataType type, long sender, long receiver, [CanBeNull] object data)
		{
			switch (type)
			{
				case CustomDataType.AUDIO_RECORD:
					OnAudioRecord?.Invoke(sender);
					break;
			}
		}
	}
}
