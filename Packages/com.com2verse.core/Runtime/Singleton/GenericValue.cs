/*===============================================================
* Product:		Com2Verse
* File Name:	GenericValue.cs
* Developer:	urun4m0r1
* Date:			2022-10-20 11:10
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse
{
	/// <summary>
	/// Use this struct to solve generic classes static field conflict.
	/// <a href="https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1000">CA1000</a>
	/// </summary>
	public struct GenericValue<TClass, TValue>
	{
		public GenericValue(TValue? value)
		{
			Value = value;
		}

		public TValue? Value { get; set; }
	}
}
