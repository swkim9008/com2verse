/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_SubScreenManager.cs
* Developer:	sprite
* Date:			2023-07-28 11:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // Sub Screen Manager
    {
        static readonly string[] TEST_SPLITSCREEN_JSON =
        {
@"{
    ""screens"" :
    [
        {
            ""screenId"": 0,
            ""offsetX"" : 0.3,
            ""offsetY"" : 0,
            ""scaleX"" : 0.4,
            ""scaleY"" : 1
        },
        {
            ""screenId"": 1,
            ""offsetX"" : 0,
            ""offsetY"" : 0,
            ""scaleX"" : 0.3,
            ""scaleY"" : 0.5
        },
        {
            ""screenId"": 2,
            ""offsetX"" : 0,
            ""offsetY"" : 0.5,
            ""scaleX"" : 0.3,
            ""scaleY"" : 0.5
        },
        {
            ""screenId"": 3,
            ""offsetX"" : 0.7,
            ""offsetY"" : 0,
            ""scaleX"" : 0.3,
            ""scaleY"" : 1
        }
    ]
}",            
@"{
    ""screens"" :
    [
        {
            ""screenId"": 0,
            ""offsetX"" : 0.5,
            ""offsetY"" : 0,
            ""scaleX"" : 0.5,
            ""scaleY"" : 0.5
        },
        {
            ""screenId"": 1,
            ""offsetX"" : 0,
            ""offsetY"" : 0,
            ""scaleX"" : 0.5,
            ""scaleY"" : 0.5
        },
        {
            ""screenId"": 2,
            ""offsetX"" : 0,
            ""offsetY"" : 0.5,
            ""scaleX"" : 0.5,
            ""scaleY"" : 0.5
        },
        {
            ""screenId"": 3,
            ""offsetX"" : 0.5,
            ""offsetY"" : 0.5,
            ""scaleX"" : 0.5,
            ""scaleY"" : 0.5
        }
    ]
}"
        };

        public string GetTestSplitScreenJson(int index)
            => TEST_SPLITSCREEN_JSON[index];

        [Serializable]
        public class TexCoordInfo
        {
            public long screenId;
            public double offsetX;
            public double offsetY;
            public double scaleX;
            public double scaleY;

            public Rect ToRect()
                => new()
                {
                    x = (float)this.offsetX,
                    y = (float)this.offsetY,
                    width = (float)this.scaleX,
                    height = (float)this.scaleY
                };

            public string ToJson()
                => JsonUtility.ToJson(this);
        }

        [Serializable]
        public class SplitScreens
        {
            public TexCoordInfo[] screens;
        }

        [Header("Screen Group")]
        [SerializeField] private string _screenGroupName = string.Empty;
        [SerializeField] private eScreenType _screenType = eScreenType.DONTCARE;
        [SerializeField] private long _subScreenId = 0;

        private bool _isSplitScreenController => _screenType == eScreenType.MAIN;

        private List<MiceWebView> _subScreens;

        private bool TryFindMainScreen(out MiceWebView mainScreen)
        {
            mainScreen = null;

            // 현재 노드(GameObject)와 그 하위 노드에서 찾아보기.
            var webViews = this.GetComponentsInChildren<MiceWebView>();
            if (webViews != null && webViews.Length > 0)
            {
                for (int i = 0, cnt = webViews.Length; i < cnt; i++)
                {
                    var webView = webViews[i];
                    if (webView._screenType != eScreenType.MAIN || this == webView) continue;

                    if (this.CompareGroupName(webView))
                    {
                        mainScreen = webView;
                        return true;
                    }
                }
            }

            // Static MapObject 에서 찾아보기
            foreach (var pair in MapController.Instance.StaticObjects)
            {
                if (pair.Value is not MapObject mapObject) continue;

                if
                (
                    mapObject.TryGetComponent(out MiceWebView miceWebView) &&
                    miceWebView._screenType == eScreenType.MAIN && this.CompareGroupName(miceWebView)
                )
                {
                    mainScreen = miceWebView;
                    return true;
                }
            }

            return false;
        }

        public bool CompareGroupName(MiceWebView webView)
            => (string.IsNullOrEmpty(this._screenGroupName) && string.IsNullOrEmpty(webView._screenGroupName)) ||
               string.Equals(this._screenGroupName, webView._screenGroupName);

        private void AppendToMainScreen()
        {
            if (_screenType != eScreenType.SUB || !this.TryFindMainScreen(out var mainScreen)) return;

            mainScreen.AppendSubScreen(this);
        }

        private void AppendSubScreen(MiceWebView miceWebView)
        {
            _subScreens ??= new(3);
            if (_subScreens.Contains(miceWebView)) return;

            _subScreens.Add(miceWebView);
        }

        private void ResetSplitSurfaces()
        {
            this.ResetSurface();

            if (_isSplitScreenController)
            {
                for (int i = 0, cnt = _subScreens?.Count ?? 0; i < cnt; i++)
                {
                    _subScreens[i].ResetSurface();
                }
            }
        }

        private async UniTask SetSplitSurfaces(string splitScreenJson, Texture texture = null, bool flip = false, bool pointerInput = false)
        {
            bool apply = false;
            Rect texCoord = new(0, 0, 1, 1);

            var texSubScreen = texture != null && texture ? texture : this.WebViewTexture;

            List<UniTask> subScreenTasks = new(3);
            SplitScreens splitInfo;

            try
            {
                splitInfo = JsonUtility.FromJson<SplitScreens>(splitScreenJson);
            }
            catch (Exception e)
            {
                NamedLoggerTag.Sprite.LogError($"<Exception> {e.Message}\r\n{e.StackTrace}");
                splitInfo = null;
            }

            for (int i = 0, cnt = splitInfo?.screens?.Length ?? 0; i < cnt; i++)
            {
                var scr = splitInfo.screens[i];

                if (scr.screenId == 0)
                {
                    apply = true;
                    texCoord = scr.ToRect();
                }
                else
                {
                    if (this._subScreens == null || this._subScreens.Count == 0) break;

                    var subScreen = this._subScreens.FirstOrDefault(e => e._subScreenId == scr.screenId);
                    if (subScreen != null && subScreen)
                    {
                        var json = scr.ToJson();
                        subScreenTasks.Add(UniTask.Defer(() => subScreen.SetScreenState(eScreenState.SUB_MONITOR, json, extraParameter: texSubScreen)));
                    }
                }
            }

            this.SetSurface(texture, flip, pointerInput, applyTexCoord: apply, texCoord: texCoord);

            for (int i = 0, cnt = subScreenTasks?.Count ?? 0; i < cnt; i++)
            {
                await subScreenTasks[i];
            }
        }
    }
}
