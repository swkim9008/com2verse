/*===============================================================
* Product:		Com2Verse
* File Name:	MiceKioskTagProcessor.cs
* Developer:	ikyoung
* Date:			2023-08-07 16:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class KioskTag
	{
		public string kioskWebUrl;
	}
	
	[TagObjectType(eObjectType.KIOSK)]
	public sealed class KioskTagProcessor : BaseTagProcessor
	{
		public static readonly string TagKey        = "KioskTag";

		public override void Initialize()
		{
			SetDelegates(TagKey, (value, mapObject) =>
			{
				var kiosk = mapObject.GetComponent<KioskObject>();
				if (kiosk.IsReferenceNull()) return;
				
				var tagValue = JsonUtility.FromJson<KioskTag>(value);
				if (tagValue != null)
				{
					kiosk.UpdateTagValue<KioskTag>(tagValue);
				}
			});
		}
	}
}
