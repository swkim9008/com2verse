/*===============================================================
 * Product:		Com2Verse
 * File Name:	StackTraceIgnore.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-13 14:53
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.Logger
{
	[AttributeUsage(AttributeTargets.Method)]
	public class StackTraceIgnore : Attribute {}
}
