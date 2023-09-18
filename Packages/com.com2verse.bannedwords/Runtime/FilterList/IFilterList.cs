/*===============================================================
* Product:		Com2Verse
* File Name:	IFilterList.cs
* Developer:	jhkim
* Date:			2023-07-14 20:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;

namespace Com2Verse.BannedWords
{
	public interface IFilterList
	{
		public void Add(string lang, IEnumerable<BannedWordsInfo.WordInfo> infos);
		public void Clear();
	}
}
