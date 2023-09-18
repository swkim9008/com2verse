/*===============================================================
* Product:		Com2Verse
* File Name:	DynamicAdsObject.cs
* Developer:	haminjeong
* Date:			2023-06-07 10:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class DynamicURLList
	{
		public List<DynamicURLPair> URLPairs;
	}

	[Serializable]
	public sealed class DynamicURLPair
	{
		public int    AdsType;
		public string LinkAddress;
		public string DisplayURL;
	}
	
	public sealed class DynamicAdsObject : AdsObject
	{
		/// <summary>
		/// DynamicURLList 자료형으로 태그로 부터 읽어온 광고데이터를 입력합니다.
		/// </summary>
		/// <param name="urlList">url 정보들이 담긴 데이터</param>
		public void InitURL(DynamicURLList urlList)
		{
			AdsDatas?.Clear();
			foreach (var pair in urlList?.URLPairs)
			{
				AdsData data = new(pair.DisplayURL, pair.LinkAddress, (eAdsType)pair.AdsType);
				AdsDatas?.Add(data);
			}
			if (AdsDatas.Count == 1) AdsDatas[0].DisplayTime = long.MaxValue;

			InitData();
		}
	}
}
