/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_Test.cs
* Developer:	sprite
* Date:			2023-07-19 10:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
#if UNITY_EDITOR
    public partial class MiceWebView    // Test
    {
        [Header("테스트")]
        [SerializeField] private eScreenState _testState = eScreenState.NONE;
        [SerializeField] private int _testIndex = 0;
        private eScreenState _lastTestState = eScreenState.NONE;
        private int _lastTestIndex = 0;

        private static readonly Dictionary<eScreenState, string[]> TEST_LIST = new()
        {
            [eScreenState.NONE]             = new[] { "about:blank" },
            [eScreenState.STREAMING]        = new[] 
            {
                "https://test-streams.mux.dev/x36xhzz/x36xhzz.m3u8",
                "https://devint-mice-stream.com2verse.com/live/splittest/master.m3u8",
            },
            [eScreenState.VOD]              = new[] { "https://download.blender.org/peach/trailer/trailer_400p.ogg" },
            [eScreenState.YOUTUBE]          = new[]
            {
                "btSO1igRWR8",
                "JX2H_sslPgk",
                "https://www.youtube.com/embed/JX2H_sslPgk?mute=0&autoplay=1&controls=0&disablekb=1&fs=1&loop=1",
                "https://www.youtube.com/watch?v=JX2H_sslPgk&mute=0&autoplay=1&controls=1&disablekb=1&fs=1&loop=0",
                "https://youtu.be/JX2H_sslPgk?mute=0&autoplay=0&controls=0&disablekb=1&fs=1&loop=1",
                "https://www.youtube.com/embed/JX2H_sslPgk",
                "https://www.youtube.com/watch?v=JX2H_sslPgk",
                "https://youtu.be/JX2H_sslPgk",
            },
            [eScreenState.STANDBY_IMAGE]    = new[] { "https://upload.wikimedia.org/wikipedia/commons/c/c4/PM5544_with_non-PAL_signals.png" },
            [eScreenState.WEB_PAGE]         = new[] { "https://okky.kr" },
        };

        partial void PartialTestStart()
        {
            this.RefreshTest();
        }

        private bool RefreshTest()
        {
            if (_isSplitScreenController && _testState == eScreenState.SUB_MONITOR)
            {
                _testState = _lastTestState;
                this.Log($"SUB_MONITOR state not allowed!");
                return false;
            }

            // 프리펩 리소스 일 경우, 동작하지 않도록 한다.
            var prefabAssetType = UnityEditor.PrefabUtility.GetPrefabAssetType(this.gameObject);
            this.Log($"{this.gameObject.name} is {prefabAssetType}");
            if (prefabAssetType != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                this.Log("Skip!");
                return false;
            }

            if (TEST_LIST.TryGetValue(_testState, out var source))
            {
                _testIndex = Mathf.Clamp(_testIndex, 0, source.Length - 1);

                if (_testState != _lastTestState || _testIndex != _lastTestIndex)
                {
                    _lastTestState = _testState;
                    _lastTestIndex = _testIndex;
                    this.SetScreenState(_testState, source[_testIndex]).Forget();
                    return true;
                }
            }

            return false;
        }

        private void ForceUpdateTestState(eScreenState state)
        {
            _testState = state;
            _lastTestState = state;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (_webView != null && _webView)
            {
                _webView.Resolution = _resolution;
            }

            this.RefreshTest();
        }
    }
#endif
}
