// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TextBoardTagModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-22 오후 3:35
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Newtonsoft.Json;

namespace Com2Verse.UI
{
	[System.Serializable]
	public class TextBoardTagModel
	{
		[JsonProperty("boardId")] public long BoardId;
		[JsonProperty("text")] public string Text;
		[JsonProperty("author")] public string Author;
		[JsonProperty("alignment")] public int Alignment;
		[JsonProperty("textColor")] public int TextColor;
		[JsonProperty("backgroundImage")] public int BackgroundImageIndex;
		[JsonProperty("date")] public string LastUpdatedDate;
	}
}
