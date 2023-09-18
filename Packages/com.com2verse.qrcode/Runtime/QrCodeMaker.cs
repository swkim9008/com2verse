/*===============================================================
* Product:		Com2Verse
* File Name:	QrcodeMaker.cs
* Developer:	klizzard
* Date:			2023-04-05 11:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.QrCode.Helpers;
using Com2Verse.QrCode.Schemas;
using JetBrains.Annotations;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

namespace Com2Verse.QrCode
{
	public static class QrCodeMaker
	{
		private const int DEFAULT_WIDTH = 256;
		private const int DEFAULT_HEIGHT = 256;

		private static Color32[] Encode(string textForEncoding, int width, int height)
		{
			var writer = new BarcodeWriter
			{
				Format = BarcodeFormat.QR_CODE,
				Options = new QrCodeEncodingOptions
				{
					Height = height,
					Width = width,
					CharacterSet = "UTF-8",
					Margin = 0
				}
			};
			return writer.Write(textForEncoding);
		}

		private static Texture2D Generate(string textForEncoding, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT)
		{
			var encoded = new Texture2D(width, height, TextureFormat.RGB24, false);
			var color32 = Encode(textForEncoding, encoded.width, encoded.height);
			encoded.SetPixels32(color32);
			encoded.Apply();
			return encoded;
		}

		public static Texture2D Generate([NotNull] VCardSchema vCard, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT)
			=> Generate(DataHelper.GetData(vCard), width, height);
	}
}
