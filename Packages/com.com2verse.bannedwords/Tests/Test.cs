using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Com2Verse.BannedWords.Tests
{
    public class Test
    {
        private readonly Dictionary<TestInfo, (string, string)[]> _testCaseMap = new Dictionary<TestInfo, (string, string)[]>
        {
            {
                TestInfo.CreateNew("All", "All", "Sentence"),
                new (string, string)[]
                {
                    ("hello, world hell", "hello, world *"),
                    ("HELL HELl HElL HEll HeLL HeLl HelL Hell hELL hELl hElL hEll heLL heLl helL hell", "* * * * * * * * * * * * * * * *"),
                }
            },
        };

        private AppDefine _appDefine;
        private BannedWordsInfo _bannedWordsInfo;
        private bool _isLoaded = false;

        [UnitySetUp]
        private IEnumerator Initialize()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var (appDefine, available) = await InitAsync();
                _isLoaded = available;
                _appDefine = appDefine;
                if (_isLoaded)
                    _bannedWordsInfo = await BannedWords.LoadAsync(_appDefine);
            });
        }
        private async UniTask<(AppDefine, bool)> InitAsync()
        {
            var appDefine = new AppDefine
            {
                AppId = "com.com2us.aaa",
                Game = "default",
                Revision = "15",
                IsStaging = false,
            };

            var available = await BannedWords.CheckAndUpdateAsync(appDefine);
            return (appDefine, available);
        }

        [UnityTearDown]
        private IEnumerator Terminate()
        {
            yield return null;
        }
        [UnityTest]
        public IEnumerator CheckTestCase()
        {
            if (_isLoaded)
            {
                foreach (var (key, testCases) in _testCaseMap)
                {
                    BannedWords.SetLanguageCode(key.LangCode);
                    BannedWords.SetCountryCode(key.CountryCode);
                    BannedWords.SetUsage(key.Usage);

                    foreach (var (input, expected) in testCases)
                    {
                        var filteredText = BannedWords.ApplyFilter(input, "*");
                        Debug.Log(filteredText);

                        Assert.IsTrue(filteredText == expected);
                    }
                }
            }

            Assert.IsTrue(_isLoaded);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckFilter()
        {
            yield return CheckFilterInternal(word => TestApplyFilter(word, "*", "*"));
        }

        [UnityTest]
        public IEnumerator CheckFilterX2()
        {
            yield return CheckFilterInternal(word => TestApplyFilter($"{word}{word}", "*", "**"));
        }

        [UnityTest]
        public IEnumerator CheckFilterX3()
        {
            yield return CheckFilterInternal(word => TestApplyFilter($"{word}{word}{word}", "*", "***"));
        }
        private IEnumerator CheckFilterInternal(Action<string> onTest)
        {
            if (_isLoaded)
            {
                Stopwatch sw = new Stopwatch();
                foreach (var key in _bannedWordsInfo.WordInfoMap.Keys)
                {
                    sw.Restart();
                    foreach (var wordInfo in _bannedWordsInfo.WordInfoMap[key])
                    {
                        BannedWords.SetLanguageCode(wordInfo.Lang);
                        BannedWords.SetCountryCode(wordInfo.Country);
                        BannedWords.SetUsage(wordInfo.Usage);

                        onTest?.Invoke(wordInfo.Word);
                    }

                    sw.Stop();
                    Debug.Log($"[{key}] 소요 시간 {sw.ElapsedMilliseconds * 0.001f: 00.00}초 (({_bannedWordsInfo.WordInfoMap[key].Count}개)");
                }
            }

            Assert.IsTrue(_isLoaded, "금칙어 로드 실패");
            yield return null;
        }

        private void TestApplyFilter(string text, string replace, string expect)
        {
            var filtered = BannedWords.ApplyFilter(text, replace);
            if (filtered != expect)
            {
                var filtered2 = BannedWords.ApplyFilter(filtered, replace);
                Assert.IsTrue(filtered == filtered2, $"금칙어 처리 실패 {filtered} -> {filtered2}");
            }
        }
#region Data
        private struct TestInfo
        {
            public string LangCode { get; private set; }
            public string CountryCode { get; private set; }
            public string Usage { get; private set; }

            public static TestInfo CreateNew(string lang = "All", string countryCode = "All", string usage = "All")
            {
                var testInfo = new TestInfo
                {
                    LangCode = lang,
                    CountryCode = countryCode,
                    Usage = usage
                };
                return testInfo;
            }
        }
#endregion // Data
    }
}
