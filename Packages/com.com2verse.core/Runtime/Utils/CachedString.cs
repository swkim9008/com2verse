/*===============================================================
* Product:		Com2Verse
* File Name:	CachedString.cs
* Developer:	urun4m0r1
* Date:			2022-04-22 15:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Utils
{
	public static class CachedString
	{
		private static readonly int      MinValue      = -999;
		private static readonly int      MaxValue      = 999;
		private static readonly int      Length        = MaxValue - MinValue + 1;
		private static readonly string[] CachedStrings = new string[Length];

		static CachedString()
		{
			for (var i = 0; i < Length; ++i)
			{
				CachedStrings[i] = (MinValue + i).ToString();
			}
		}

		/// <summary>
		/// Convert int to string without memory allocation
		/// </summary>
		/// <param name="value">Value that exceeding range
		/// (<see cref="MinValue"/> ~ <see cref="MaxValue"/>)
		/// will cause memory allocation.</param>
		public static string Get(int value) =>
			value < MinValue || value > MaxValue
				? value.ToString()
				: CachedStrings[value - MinValue];
	}
}
