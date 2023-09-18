/*===============================================================
* Product:		Com2Verse
* File Name:	RequestOption.cs
* Developer:	jhkim
* Date:			2023-07-20 17:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.HttpHelper
{
	public class RequestOption
	{
		public bool HasTimeout = true;
		public bool HasPending = true;

		public static RequestOption Default { get; } = new RequestOption
		{
			HasTimeout = true,
			HasPending = true,
		};

		public static RequestOption NoTimeout { get; } = new RequestOption
		{
			HasTimeout = false,
			HasPending = false,
		};
	}
}
