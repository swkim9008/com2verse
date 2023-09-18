/*===============================================================
* Product:		Com2Verse
* File Name:	Data.cs
* Developer:	jhkim
* Date:			2023-03-10 18:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.BannedWords
{
	internal sealed class Data : Base
	{
		internal override string MakeFileName(AppDefine appDefine)
		{
			var serverType = appDefine.IsStaging ? "staging" : "real";
			return $"bannedwords_{appDefine.AppId}_{appDefine.Game}_{serverType}";
		}
	}
}
