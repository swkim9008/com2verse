/*===============================================================
 * Product:		Com2Verse
 * File Name:	C2VDebugTests.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-13 16:55
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using NUnit.Framework;

namespace Com2VerseTests.Logger.Editor
{
	public class C2VDebugTests
	{
		[Test]
		public void C2VDebug_Log()
		{
			C2VDebug.Log("Test");
		}
	}
}
