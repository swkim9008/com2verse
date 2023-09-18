/*===============================================================
* Product:		Com2Verse
* File Name:	DynamicAdsTagProcessor.cs
* Developer:	haminjeong
* Date:			2023-06-07 20:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Network
{
	[TagObjectType(eObjectType.DYNAMIC_SCREEN)]
	public sealed class DynamicAdsTagProcessor : BaseTagProcessor
	{
		private static readonly string URLListKey        = "DynamicURLList";
		private static readonly string ExposurePeriodKey = "Period";

		private DynamicAdsObject _adsObject;

		public override void Initialize()
		{
			SetDelegates(URLListKey, (value, mapObject) =>
			{
				DynamicAdsObject adsObject = mapObject.GetComponent<DynamicAdsObject>();
				if (adsObject.IsReferenceNull()) return;
				_adsObject = adsObject;
				DynamicURLList pairs = JsonUtility.FromJson<DynamicURLList>(value);
				if (pairs == null) return;
				_adsObject.InitURL(pairs);
			});
			SetDelegates(ExposurePeriodKey, (value, mapObject) =>
			{
				DynamicAdsObject adsObject = mapObject.GetComponent<DynamicAdsObject>();
				if (adsObject.IsReferenceNull()) return;
				_adsObject = adsObject;
				DisplayTime times = JsonUtility.FromJson<DisplayTime>(value);
				if (times == null) return;
				_adsObject.InitTimes(times.StartTime, times.EndTime);
			});
		}
	}
}
