/*===============================================================
* Product:		Com2Verse
* File Name:	LeafletStandTagProcessor.cs
* Developer:	ikyoung
* Date:			2023-06-13 16:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class LeafletScreenTag
	{
		public List<Leaflet> leaflets = new List<Leaflet>();

		public Leaflet GetLeaflet(int index)
		{
			if (leaflets.Count > index)
			{
				return leaflets[index];
			}
			return null;
		}
	}

	[Serializable]
	public sealed class Leaflet
	{
		public string thumbnailImageUrl;
		public string pdfLinkUrl;
	}
	
	[TagObjectType(eObjectType.LEAFLET_SCREEN)]
	public sealed class LeafletStandTagProcessor : BaseTagProcessor
	{
		public override void Initialize()
		{
			SetDelegates(typeof(LeafletScreenTag).Name, (value, mapObject) =>
			{
				var leafletStand = mapObject.GetComponent<LeafletScreenObject>();
				if (leafletStand.IsReferenceNull()) return;
				
				var tagValue = JsonUtility.FromJson<LeafletScreenTag>(value);
				if (tagValue != null)
				{
					leafletStand.UpdateTagValue<LeafletScreenTag>(tagValue);
					leafletStand.LoadAsync().Forget();
				}
			});
		}
	}
}
