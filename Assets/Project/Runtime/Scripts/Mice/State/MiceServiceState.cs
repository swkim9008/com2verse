/*===============================================================
* Product:		Com2Verse
* File Name:	MiceServiceState.cs
* Developer:	ikyoung
* Date:			2023-07-11 13:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
	public enum eMiceServiceState
	{
		NONE,
		SESSION_IDLE,
		SESSION_PLAYING,
		SESSION_QnA,
		PLAYING_CUTSCENE,
	}
	
	public abstract class MiceServiceState
	{
		public eMiceServiceState ServiceStateType { get; protected set; }

		public virtual void Prepare()
		{
		}

		public virtual void OnStart(eMiceServiceState prevStateType)
		{
		}
		public virtual void OnStop()
		{
		}
		public virtual bool MiceUIShouldVisible()
		{
			return true;
		}
		public virtual bool CanPlayingLectureVideo()
		{
			return false;
		}
	}
}
