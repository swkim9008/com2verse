/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceException.cs
* Developer:	urun4m0r1
* Date:			2022-04-05 17:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	public class DeviceException : Exception
	{
		public DeviceException() { }
		public DeviceException(string message) : base(message) { }
		public DeviceException(string message, Exception innerException) : base(message, innerException) { }
	}
}
