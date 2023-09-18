/*===============================================================
* Product:		Com2Verse
* File Name:	LightControlTrack.cs
* Developer:	eugene9721
* Date:			2022-12-07 10:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Com2Verse.Director
{
	[TrackColor(0.9454092f, 0.9779412f, 0.3883002f)]
	[TrackBindingType(typeof(Light))]
	[TrackClipType(typeof(LightControlClip))]
	public sealed class LightControlTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<LightControlBehaviour>.Create(graph, inputCount);
		}
	}
}
