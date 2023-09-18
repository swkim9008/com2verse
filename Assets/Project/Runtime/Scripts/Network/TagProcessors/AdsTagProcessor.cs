/*===============================================================
* Product:		Com2Verse
* File Name:	AdsTagProcessor.cs
* Developer:	haminjeong
* Date:			2023-05-19 16:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Network
{
	[TagObjectType(eObjectType.SCREEN)]
	public sealed class AdsTagProcessor : BaseTagProcessor
	{
		private static readonly string URLListKey        = "URLList";
		private static readonly string ExposurePeriodKey = "Period";

		private AdsObject _adsObject;

		public override void Initialize()
		{
			SetDelegates(URLListKey, (value, mapObject) =>
			{
				AdsObject adsObject = mapObject.GetComponent<AdsObject>();
				if (adsObject.IsReferenceNull()) return;
				_adsObject = adsObject;
				URLList pairs = JsonUtility.FromJson<URLList>(value);
				if (pairs == null) return;
				_adsObject.InitURL(pairs);
			});
			SetDelegates(ExposurePeriodKey, (value, mapObject) =>
			{
				AdsObject adsObject = mapObject.GetComponent<AdsObject>();
				if (adsObject.IsReferenceNull()) return;
				_adsObject = adsObject;
				DisplayTime times = JsonUtility.FromJson<DisplayTime>(value);
				if (times == null) return;
				_adsObject.InitTimes(times.StartTime, times.EndTime);
			});
		}
	}
}
