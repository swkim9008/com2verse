/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEffectObject.cs
* Developer:	wlemon
* Date:			2023-07-19 19:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Com2Verse.Mice
{
	public class MiceEffectObject : MonoBehaviour
	{
		[SerializeField]
		private double _ignoreTimeOffset = -1;

		public double IgnoreTimeOffset => _ignoreTimeOffset;

		public virtual void Play(double offset) { }

		public virtual void Stop() { }

		public bool CheckIgnoreTimeOffset(double offset)
		{
			if (_ignoreTimeOffset > 0.0f)
			{
				return offset > _ignoreTimeOffset;
			}

			return false;
		}
	}
}
