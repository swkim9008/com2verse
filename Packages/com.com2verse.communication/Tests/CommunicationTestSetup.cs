/*===============================================================
 * Product:		Com2Verse
 * File Name:	CommunicationTestSetup.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-20 10:59
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AssetSystem;
using Com2Verse.Communication;
using Com2Verse.Logger;
using Com2Verse.Utils;

namespace Com2VerseTests.Communication
{
	public static class CommunicationTestSetup
	{
		public static void Setup()
		{
			C2VDebug.LogWarning("<b><color=yellow>===== Setup for tests =====</color></b>");

			CoroutineManager.SetupForTests();
			AssetSystemManager.SetupForTests();
			ChannelManager.SetupForTests();
			VoiceDetectionManager.SetupForTests();
		}

		public static void TearDown()
		{
			C2VDebug.LogWarning("<b><color=yellow>===== TearDown for tests =====</color></b>");

			CoroutineManager.SetupForTests();
			AssetSystemManager.TearDownForTests();
			ChannelManager.SetupForTests();
			VoiceDetectionManager.TearDownForTests();
		}
	}
}
