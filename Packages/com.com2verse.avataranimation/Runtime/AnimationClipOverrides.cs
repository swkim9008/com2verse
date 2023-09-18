﻿/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationClipOverrides.cs
* Developer:	haminjeong
* Date:			2022-10-16 17:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse.AvatarAnimation
{
	public sealed class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
	{
		public AnimationClipOverrides(int capacity) : base(capacity) { }

		public AnimationClip this[string name]
		{
			get { return Find(x => x.Key.name.Equals(name)).Value; }
			set
			{
				var index = FindIndex(x => x.Key.name.Equals(name));
				if (index != -1)
					this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
			}
		}
	}
}
