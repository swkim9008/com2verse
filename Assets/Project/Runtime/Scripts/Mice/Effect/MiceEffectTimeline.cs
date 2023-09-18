/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEffectTimeline.cs
* Developer:	wlemon
* Date:			2023-07-20 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine.Playables;

namespace Com2Verse.Mice
{
	public sealed class MiceEffectTimeline : MiceEffectObject
	{
		private PlayableDirector _playableDirector;

		private void Awake()
		{
			_playableDirector = GetComponentInChildren<PlayableDirector>();
		}

		public override void Play(double offset)
		{
			if (_playableDirector.IsUnityNull()) return;

			_playableDirector.time = offset;
			_playableDirector.Play();
		}

		public override void Stop()
		{
			if (_playableDirector.IsUnityNull()) return;

			_playableDirector.Stop();
		}
	}
}
