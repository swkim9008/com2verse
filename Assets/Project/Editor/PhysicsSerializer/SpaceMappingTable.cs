/*===============================================================
* Product:		Com2Verse
* File Name:	SpaceMappingTable.cs
* Developer:	haminjeong
* Date:			2023-08-23 14:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com2VerseEditor
{
	[CreateAssetMenu(fileName = "SpaceMappingTable", menuName = "Com2Verse/Create SpaceMappingTable")]
	public sealed class SpaceMappingTable : ScriptableObject
	{
		[Serializable]
		public class MappingInfo
		{
			public long   MapTemplateId;
			public string MappingId;
			public string SceneName;
		}

		public List<MappingInfo> MappingInfoTable = new();
	}
}