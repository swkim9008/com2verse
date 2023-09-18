/*===============================================================
* Product:		Com2Verse
* File Name:	BooleanFilter.cs
* Developer:	urun4m0r1
* Date:			2022-08-31 19:31
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Utils
{
	public enum eBooleanFilterType
	{
		IGNORE,
		ANY,
		TRUE,
		FALSE,
	}

	public static class BooleanFilter
	{
		public static bool Apply(bool target, eBooleanFilterType filter) => filter switch
		{
			eBooleanFilterType.IGNORE => false,
			eBooleanFilterType.ANY    => true,
			eBooleanFilterType.TRUE   => target,
			eBooleanFilterType.FALSE  => !target,
			_                         => false,
		};
	}
}
