/*===============================================================
* Product:		Com2Verse
* File Name:	PlayableDirectorExtension.cs
* Developer:	wlemon
* Date:			2023-08-24 17:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Com2Verse.Extension
{
	public static class PlayableDirectorExtension
	{
		public static void SetBinding(this PlayableDirector playableDirector, string trackName, Object target)
		{
			var timelineAsset = playableDirector!.playableAsset as TimelineAsset;
			if (timelineAsset.IsUnityNull()) return;

			foreach (var binding in timelineAsset.outputs)
			{
				if (binding.streamName       != trackName) continue;
				if (binding.outputTargetType != target.GetType()) continue;
				playableDirector!.SetGenericBinding(binding.sourceObject, target);
			}
		}

		public static void SetGameObjectBinding(this PlayableDirector playableDirector, string trackName, GameObject target, bool checkChildComponents = true)
		{
			var timelineAsset = playableDirector!.playableAsset as TimelineAsset;
			if (timelineAsset.IsUnityNull()) return;

			var gameObjectType = typeof(GameObject);
			foreach (var binding in timelineAsset.outputs)
			{
				if (binding.streamName != trackName) continue;
				if (binding.outputTargetType == gameObjectType)
				{
					playableDirector!.SetGenericBinding(binding.sourceObject, target);
				}
				else
				{
					var component = checkChildComponents ? target.GetComponentInChildren(binding.outputTargetType) : target.GetComponent(binding.outputTargetType);
					if (!component.IsUnityNull())
					{
						playableDirector!.SetGenericBinding(binding.sourceObject, component);
					}
				}
			}
		}

		public static void RemoveBinding(this PlayableDirector playableDirector, string trackName)
		{
			var timelineAsset = playableDirector!.playableAsset as TimelineAsset;
			if (timelineAsset.IsUnityNull()) return;

			foreach (var binding in timelineAsset.outputs)
			{
				if (binding.streamName != trackName) continue;
				playableDirector!.SetGenericBinding(binding.sourceObject, null);
			}
		}
	}
}
