/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecrodNavigator.cs
* Developer:	ydh
* Date:			2023-03-20 17:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.AudioRecord
{
	public sealed class AudioRecordNavigator : MonoBehaviour
	{
		public static void OpenAudioRecrod()
		{
			AudioRecordManager.Instance.OpenAudioRecord("");
		}
	}
}