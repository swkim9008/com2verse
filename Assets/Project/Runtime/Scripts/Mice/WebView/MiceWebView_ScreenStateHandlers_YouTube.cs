/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_ScreenStateHandlers_YouTube.cs
* Developer:	sprite
* Date:			2023-08-23 15:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // Screen State (eScreenState) Handlers - YouTube
    {
        [ScreenState(eScreenState.YOUTUBE)]
        private async UniTask OnScreenState_YouTube()
        {
            (var state, string parameters, object extraParameter, CancellationToken cancellationToken) = _currentSSParams;

            if (!YT.TryParse(parameters, out var videoId, out var queryMap))
            {
                this.LogError($"Not recognized YouTube URL! ({parameters})");

                this.ResetSurface();

                return;
            }

            var url = YT.BuildEmbedUrl(videoId, queryMap);

            this.Log($"({state}) Source='{parameters}'\r\nVideo ID='{videoId}' URL='{url}'");

            // Mice Service 상태가 CutScene 일 경우 끝 날 때까지 대기한다.
            await this.WaitForMiceServiceStateCutScene();

            if (await this.LoadUrl(url).WithCancellationToken(cancellationToken))
            {
                if (_isSplitScreenController)
                {
                    await this.SetSplitSurfaces(extraParameter as string, pointerInput: false);
                }
                else
                {
                    this.SetSurface(pointerInput: false);
                }

                // YouTube Video Event Handling & Manual Looping & Etc...
                var result = await this.ExecuteJavaScriptFromFile("yt_helper.js", true);
                this.Log($"(yt_helper.js) execution result = {result}");

                this.CheckMiceServiceStateForVideo(true);
            }
            else
            {
                this.ResetSurface();
            }
        }

        internal class YT
        {
            /// <summary>
            /// YouTube 동영상 컨트롤을 사용 할 지 여부.
            /// </summary>
            public static bool YT_USE_CONTROLS => true;

            const string QUERY_PARAM_LOOP = "loop";
            const string QUERY_PARAM_PLAYLIST = "playlist";

            static readonly Dictionary<string, string> DefaultQueryMap = new()
            {
                ["mute"]                = "0",
                ["autoplay"]            = "1",
                ["controls"]            = YT.YT_USE_CONTROLS ? "1" : "0",   // 동영상 플레이어 컨트롤 표시 여부 (1: 표시)
                ["disablekb"]           = "0",
                ["fs"]                  = "0",      // Fullscreen 버튼 표시 여부 (1: 표시)
                [QUERY_PARAM_LOOP]      = "1",      // 반복 여부 (1: 반복)(단일 동영상을 반복하려면 playlist=[videoId] 를 추가해야한다)
                ["modestbranding"]      = "1",      // 이 옵션이 1이면 YouTube 로고를 표시하지 않는다.(YouTube에서 보기 버튼)
                [QUERY_PARAM_PLAYLIST]  = "",       // loop=1인 경우, 단일 동영상을 반복하기 위한 playlist 옵션 설정용 값.
            };

            public const string YOUTUBE_EMBED   = "https://www.youtube.com/embed";
            public const string YOUTUBE_WATCH   = "https://www.youtube.com/watch";
            public const string YOUTU_BE        = "https://youtu.be";

            public static string BuildEmbedUrl(string videoId, Dictionary<string, string> queryMap)
                => $"{YOUTUBE_EMBED}/{videoId}{YT.BuildQuery(videoId, queryMap)}";

            public static bool TryParse(string sourceUrl, out string videoId, out Dictionary<string, string> queryMap)
            {
                queryMap = null;
                string sourceQuery = "";

                var posA = sourceUrl.IndexOf(YOUTUBE_EMBED);
                var posB = sourceUrl.IndexOf(YOUTU_BE);
                var posC = sourceUrl.IndexOf(YOUTUBE_WATCH);
                if (posA == -1 && posB == -1 && posC == -1)
                {
                    // 순수하게 YouTube Video ID 만 있는 경우,
                    videoId = sourceUrl.Trim();
                }
                else
                {
                    //
                    // "https://www.youtube.com/embed/~~~~?abc=0&def=1..."
                    // "https://www.youtube.com/watch?v=~~~~&abc=0&def=1..."
                    // "https://youtu.be/~~~~?abc=0&def=1..."
                    //

                    var source = sourceUrl
                        .Replace(YOUTUBE_EMBED, "")
                        .Replace(YOUTU_BE, "")
                        .Replace(YOUTUBE_WATCH, "")
                        .Trim('/');

                    if (posC >= 0)
                    {
                        source = source.Replace("?v=", "");
                    }

                    // 기존 Query 삭제.
                    var pos = source.IndexOf('?');
                    if (pos < 0)
                    {
                        pos = source.IndexOf("&");
                    }
                    if (pos >= 0)
                    {
                        sourceQuery = source[(pos + 1)..];
                        source = source[..pos];
                    }

                    videoId = source.Trim();

                    if (string.IsNullOrEmpty(videoId))
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(sourceQuery) && !string.IsNullOrWhiteSpace(sourceQuery))
                {
                    var src = sourceQuery.Trim('?');
                    var strs = src.Split('&');

                    queryMap = strs
                        .Select
                        (
                            e =>
                            {
                                var pair = e.Split('=');
                                return (Key: pair[0], Value: pair.Length >= 2 ? pair[1] : "");
                            }
                        )
                        .ToDictionary(e => e.Key, e => e.Value);
                }
                else
                {
                    queryMap = new();
                }

                return true;
            }

            public static string BuildQuery(string videoId, Dictionary<string, string> queryMap = null)
            {
                Dictionary<string, string> finalQueryMap = new(DefaultQueryMap);

                // 기본 Query값을 주어진 Query값으로 덮어 쓴다.
                if (queryMap != null)
                {
                    foreach (var pair in queryMap)
                    {
                        if (finalQueryMap.ContainsKey(pair.Key))
                        {
                            finalQueryMap[pair.Key] = pair.Value;
                        }
                        else
                        {
                            finalQueryMap.Add(pair.Key, pair.Value);
                        }
                    }
                }

                // loop=1 인 경우, 단일 동영상 반복을 위한 설정.
                if (finalQueryMap.TryGetValue(QUERY_PARAM_LOOP, out var value) && value == "1")
                {
                    if (!finalQueryMap.ContainsKey(QUERY_PARAM_PLAYLIST))
                    {
                        finalQueryMap.Add(QUERY_PARAM_PLAYLIST, "");
                    }

                    finalQueryMap[QUERY_PARAM_PLAYLIST] = videoId;
                }
                else // 반복이 아니면 playlist 를 삭제한다.
                {
                    if (finalQueryMap.ContainsKey(QUERY_PARAM_PLAYLIST))
                    {
                        finalQueryMap.Remove(QUERY_PARAM_PLAYLIST);
                    }
                }

                var queryParams = finalQueryMap
                    .Select
                    (
                        e => string.IsNullOrWhiteSpace(e.Value) 
                             ? e.Key 
                             : $"{e.Key}={e.Value}"
                    )
                    .Aggregate((a, b) => $"{a}&{b}");

                return string.IsNullOrEmpty(queryParams) ? "" : $"?{queryParams}";
            }
        }
    }
}
