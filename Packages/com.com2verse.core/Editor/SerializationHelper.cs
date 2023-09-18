/*===============================================================
* Product:		Com2Verse
* File Name:	SerializationHelper.cs
* Developer:	urun4m0r1
* Date:			2022-07-18 13:23
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2VerseEditor.Utils
{
	public static class SerializationHelper
	{
		public static string GetBackingFieldName(this string propertyName)
		{
			return IsMemberProperty(propertyName) ? propertyName : $"<{propertyName}>k__BackingField";
		}

		public static string GetPropertyName(this string fieldName)
		{
			return IsMemberProperty(fieldName) ? fieldName.Substring(1, fieldName.Length - 17) : fieldName;
		}

		public static bool IsMemberProperty(this string memberName)
		{
			return memberName.StartsWith('<') && memberName.EndsWith(">k__BackingField", StringComparison.Ordinal);
		}
	}
}
