/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseDirector.cs
* Developer:	eugene9721
* Date:			2022-12-07 12:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.Playables;

namespace Com2Verse.Director
{
	[RequireComponent(typeof(PlayableDirector))]
	public abstract class MetaverseDirector : MonoBehaviour
	{
		protected PlayableAsset? PlayableAsset
		{
			get
			{
				if (PlayableDirector.IsUnityNull()) return null;
				return PlayableDirector!.playableAsset;
			}
		}

#region Properties
		public PlayableDirector? PlayableDirector { get; private set; }
#endregion Properties

#region MonoBehaviour
		protected virtual void Awake()
		{
			PlayableDirector = GetComponent<PlayableDirector>();
		}
#endregion MonoBehaviour

#region DirectorMethods
		public virtual bool Play(IDirectorMessage? message = null)
		{
			if (PlayableDirector.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "PlayableDirector is null");
				return false;
			}

			PlayableDirector!.Stop();
			PlayableDirector.Play(PlayableAsset);
			return true;
		}

		public virtual bool Stop()
		{
			if (PlayableDirector.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "PlayableDirector is null");
				return false;
			}

			PlayableDirector!.Stop();
			return true;
		}

		public virtual bool Pause()
		{
			if (PlayableDirector.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "PlayableDirector is null");
				return false;
			}

			PlayableDirector!.Pause();
			return true;
		}

		public virtual bool Resume()
		{
			if (PlayableDirector.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(Director), "PlayableDirector is null");
				return false;
			}

			PlayableDirector!.Resume();
			return true;
		}
#endregion DirectorMethods
	}
}
