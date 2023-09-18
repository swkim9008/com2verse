/*===============================================================
 * Product:		Com2Verse
 * File Name:	ChannelConnectionTest.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-13 21:23
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections;
using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Com2VerseTests.Communication.Runtime
{
	public class ChannelConnectionTest
	{
		[SetUp]    public void Setup()    => CommunicationTestSetup.Setup();
		[TearDown] public void TearDown() => CommunicationTestSetup.TearDown();

		[UnityTest]
		public IEnumerator Test()
		{
			var channelManager = ChannelManager.Instance;

			var channels = new List<IChannel>();
			channelManager.ChannelAdded   += c => channels.Add(c);
			channelManager.ChannelRemoved += c => channels.Remove(c);

			channelManager.AddDebugChannel(new User(1), "test", "");
			channelManager.JoinAllChannels();

			yield return Convert(UniTask.WaitUntil(() => channels.Count > 0));
			Assert.IsTrue(channels.Count == 1);

			var channel = channels[0];
			Assert.IsNotNull(channel!);

			var connector = channel.Connector;
			yield return Convert(UniTask.WaitUntil(() => connector.State == eConnectionState.CONNECTED));
			Assert.IsTrue(connector.State == eConnectionState.CONNECTED);

			channelManager.RemoveDebugChannel();
			yield return Convert(UniTask.WaitUntil(() => channels.Count == 0));

			Assert.IsTrue(channels.Count == 0);
		}

		private static IEnumerator Convert(UniTask task)
		{
			var timeout = new System.TimeSpan(0, 0, 10);
			yield return task.Timeout(timeout).ToCoroutine();
		}
	}
}
