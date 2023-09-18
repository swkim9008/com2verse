/*===============================================================
 * Product:		Com2Verse
 * File Name:	DummyUserTest.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-20 10:44
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Com2Verse.Communication;
using Com2Verse.Communication.Cheat;
using Com2Verse.Logger;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com2VerseTests.Communication.Runtime
{
	public class DummyUserTest
	{
		public sealed class DummyUserTestData : IDisposable
		{
			public IViewModelUser User { get; }

			public TestData InitialTestData { get; }

			public bool IsAudibleChanged { get; set; }
			public bool IsVisibleChanged { get; set; }
			public bool IsLevelChanged   { get; set; }
			public bool IsTextureChanged { get; set; }

			public bool IsEverythingChanged
				=> IsAudibleChanged
				&& IsVisibleChanged
				&& IsLevelChanged
				&& IsTextureChanged;

			public DummyUserTestData(ChannelInfo channelInfo)
			{
				User = DummyUser.CreateInstance(channelInfo);

				InitialTestData = GetTestData();
			}

			[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
			public void CheckValueChanged()
			{
				var testData = GetTestData();

				if (InitialTestData.IsAudible != testData.IsAudible) IsAudibleChanged = true;
				if (InitialTestData.IsVisible != testData.IsVisible) IsVisibleChanged = true;
				if (InitialTestData.Level     != testData.Level) IsLevelChanged       = true;
				if (InitialTestData.Texture   != testData.Texture) IsTextureChanged   = true;
			}

			public TestData GetTestData() => new()
			{
				IsAudible = User.Voice!.Input.IsAudible,
				IsVisible = User.Camera!.Input.IsRunning,
				Level     = User.Voice!.Input.Level,
				Texture   = User.Camera!.Texture,
			};

			public void Dispose()
			{
				(User as IDisposable)?.Dispose();
			}
		}

		public readonly struct TestData
		{
			public bool     IsAudible { get; init; }
			public bool     IsVisible { get; init; }
			public float    Level     { get; init; }
			public Texture? Texture   { get; init; }
		}

		public readonly List<DummyUserTestData> DummyUsersTestData = new();

		[SetUp]
		public void Setup()
		{
			CommunicationTestSetup.Setup();
		}

		[TearDown]
		public void TearDown()
		{
			foreach (var data in DummyUsersTestData)
				data.Dispose();

			CommunicationTestSetup.TearDown();
		}

		[UnityTest]
		public IEnumerator Will_Dummy_User_Data_Change()
		{
			// Setup
			var userCount      = 100;
			var testIteration  = 50;
			var testDelay      = 0.1f;
			var minSuccessRate = 0.5f;

			// Given
			var testChannelInfo = new ChannelInfo("TestChannel", default, default, default, default);
			DummyUsersTestData.Clear();
			for (var i = 0; i < userCount; i++)
			{
				C2VDebug.Log($"Create test data {i + 1}/{userCount}...");
				DummyUsersTestData.Add(new DummyUserTestData(testChannelInfo));
			}

			// When
			for (var i = 0; i < testIteration; i++)
			{
				C2VDebug.Log($"Test {i + 1}/{testIteration}...");
				yield return new WaitForSeconds(testDelay);

				foreach (var data in DummyUsersTestData)
					data.CheckValueChanged();
			}

			// Then
			var successCount = DummyUsersTestData.Count(data => data.IsEverythingChanged);
			var successRate  = (float)successCount / userCount;
			C2VDebug.Log($"Success rate: {(successRate * 100):F2}%");
			Assert.GreaterOrEqual(successRate, minSuccessRate);
		}
	}
}
