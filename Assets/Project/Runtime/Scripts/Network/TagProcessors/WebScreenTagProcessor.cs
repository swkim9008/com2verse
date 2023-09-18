/*===============================================================
* Product:		Com2Verse
* File Name:	WebScreenTagProcessor.cs
* Developer:	ikyoung
* Date:			2023-07-18 19:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using System;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class WebScreenTag
	{
		public string DisplayingPageUrl;
		public string MoreInfoPageUrl;
	}

	[TagObjectType(eObjectType.WEB_SCREEN)]
	public sealed class WebScreenTagProcessor : EasyTagProcessor<WebScreenTag>
	{
    }
}
