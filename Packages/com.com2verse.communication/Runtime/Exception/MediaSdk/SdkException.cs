/*===============================================================
* Product:		Com2Verse
* File Name:	SdkException.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 14:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication.MediaSdk
{
	public class SdkException : Exception
	{
		public SdkException() { }
		public SdkException(string message) : base(message) { }
		public SdkException(string message, Exception innerException) : base(message, innerException) { }
	}
}
