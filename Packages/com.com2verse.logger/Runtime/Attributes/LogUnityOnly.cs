/*===============================================================
 * Product:		Com2Verse
 * File Name:	LogUnityOnly.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-13 14:54
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.Logger
{
	[AttributeUsage(AttributeTargets.Method)]
	public class LogUnityOnly : Attribute {}
}
