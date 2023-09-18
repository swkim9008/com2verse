/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelException.cs
* Developer:	urun4m0r1
* Date:			2022-04-06 18:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	public class ChannelException : Exception
	{
		public ChannelException() { }
		public ChannelException(string message) : base(message) { }
		public ChannelException(string message, Exception innerException) : base(message, innerException) { }
	}
}
