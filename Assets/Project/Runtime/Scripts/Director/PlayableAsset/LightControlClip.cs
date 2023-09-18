/*===============================================================
* Product:		Com2Verse
* File Name:	LightControlClip.cs
* Developer:	eugene9721
* Date:			2022-12-07 10:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Com2Verse.Director
{
	[Serializable]
	public sealed class LightControlClip : PlayableAsset, ITimelineClipAsset
	{
		[SerializeField]
		private LightControlBehaviour _template = new LightControlBehaviour();

		public ClipCaps clipCaps => ClipCaps.Blending;

		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<LightControlBehaviour>.Create(graph, _template);
			return playable;
		}
	}
}
