/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEffectBGM.cs
* Developer:	wlemon
* Date:			2023-07-19 19:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Sound;
using UnityEngine.AddressableAssets;

namespace Com2Verse.Mice
{
	public class MiceEffectBGM : MiceEffectObject
	{
		[SerializeField]
		private AssetReference _asset;

		public override void Play(double offset)
		{
			SoundManager.Instance.PlayBGM(_asset, 2.0f, 0.0f);
		}

		public override void Stop()
		{
			SoundManager.Instance.StopBGM();
		}
	}
}
