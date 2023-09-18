// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TextBoardTagProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-18 오전 11:17
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Globalization;
using Com2Verse.Data;
using Com2Verse.Network;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com2Verse.UI
{
	[UsedImplicitly]
	[TagObjectType(eObjectType.TEXT_BOARD)]
	public sealed class TextBoardTagProcessor : BaseTagProcessor
	{
		private const string TextBoardContentTagKey = "TextBoard";
		
		public override void Initialize()
		{
			SetDelegates(TextBoardContentTagKey, (value, mapObject) =>
			{
				var textBoard = mapObject.gameObject.GetComponent<TextBoard>();
				var textBoardTag = JsonConvert.DeserializeObject<TextBoardTagModel>(value);
				
				textBoard.TargetBoardSeq = textBoardTag.BoardId;

				var lastUpdatedTime = System.DateTime.Parse(textBoardTag.LastUpdatedDate, CultureInfo.InvariantCulture).ToLocalTime();

				textBoard.SetText(textBoardTag.Text, textBoardTag.Author, textBoardTag.Alignment, textBoardTag.TextColor, textBoardTag.BackgroundImageIndex, lastUpdatedTime);
			});
		}
	}
}
