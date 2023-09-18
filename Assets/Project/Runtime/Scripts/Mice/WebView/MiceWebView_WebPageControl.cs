/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_WebPageControl.cs
* Developer:	sprite
* Date:			2023-07-19 10:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // WebPage Control
    {
        //const string YT_JS_FMT = @"(function() {{ var video = document.querySelector('.video-stream'); {0} }})()";

        //private UniTask<string> ExecuteJSForYT(string javaScript)
        //    => ExecuteJavaScript(string.Format(YT_JS_FMT, javaScript));

        public struct JSResult
        {
            const string JS_RESULT_OK = "undefined";

            public bool IsSuccess;
            public string Message;

            public JSResult(string result)
            {
                this.IsSuccess = string.Equals(result, JS_RESULT_OK, StringComparison.OrdinalIgnoreCase);
                this.Message = result;
            }

            public static implicit operator bool(JSResult result) => result.IsSuccess;
            public static implicit operator string(JSResult result) => result.Message;

            public override string ToString() => $"IsSuccess={this.IsSuccess}, Message={this.Message}";

            public static JSResult OK { get; private set; } = new(JS_RESULT_OK);
        }

        private UniTask<JSResult> ExecuteJavaScript(string javaScript)
        {
            // 웹뷰가 준비되어 있는지 여부를 체크한다.
            if (!this.IsWebViewValid || !this.IsWebPageLoaded)
            {
                return UniTask.FromResult(new JSResult("WebView is not ready!!!"));
            }

            return this._webView.WebView.ExecuteJavaScript(javaScript).AsUniTask().ContinueWith(result => new JSResult(result));
        }

        private UniTask<JSResult> ExecuteJavaScriptFromFile(string file, bool readFromStreamingAssets = false)
        {
            if (readFromStreamingAssets)
            {
                file = System.IO.Path.Combine(Application.streamingAssetsPath, file);
            }

            if (!System.IO.File.Exists(file)) return UniTask.FromResult(new JSResult("Not exists!"));

            var js = System.IO.File.ReadAllText(file);
            if (string.IsNullOrEmpty(js)) return UniTask.FromResult(new JSResult("Unable to execute!"));

            return this.ExecuteJavaScript(js);
        }

        /// <summary>
        /// HLS 영상 스트리밍을 중단한다.
        /// </summary>
        public UniTask<JSResult> Video_HLSDetachMedia() => this.ExecuteJavaScript("hls_detachMedia()");

        public UniTask<JSResult> Video_HLSDestroy() => this.ExecuteJavaScript("hls_destroy()");

        /// <summary>
        /// Video 볼륨을 가져온다(0.0 ~ 1.0)
        /// </summary>
        /// <returns></returns>
        public async UniTask<float> Video_GetVolume()
        {
            var strVol = await this.ExecuteJavaScript("getVideoVolume()");

            this.Log($"Video Volume = {strVol}");

            return float.TryParse(strVol, out var volume) ? volume : 0;
        }

        /// <summary>
        /// Video 볼륨을 설정한다(0.0 ~ 1.0)
        /// </summary>
        /// <param name="normalizedValue"></param>
        /// <returns></returns>
        public UniTask<JSResult> Video_SetVolume(float normalizedValue)
        {
            return this.ExecuteJavaScript($"setVideoVolume({normalizedValue})");
        }

        private float _lastVideoVolume = 1.0f;
        private bool _isMuted = false;
        private bool _isPaused = false;

        public bool IsPaused => _isPaused;

        public UniTask<JSResult> Video_Mute(bool value)
        {
            if (_isMuted == value) return UniTask.FromResult(JSResult.OK);

            _isMuted = value;
            this.Log($"Value = {value}, Last Video Volume = {_lastVideoVolume}");

            return this.Video_SetVolume(value ? 0.0f : _lastVideoVolume);
        }

        public async UniTask<JSResult> Video_Play([System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = null)
        {
            var result = await this.ExecuteJavaScript("playVideo()");

            _isPaused = !result.IsSuccess;
            this.Log($"[{callerMemberName}] Video Paused={_isPaused} (Result:'{result.Message}')");

            return result;            
        }

        public async UniTask<JSResult> Video_Stop([System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = null)
        {
            var result = await this.ExecuteJavaScript("pauseVideo()");

            _isPaused = result.IsSuccess;
            this.Log($"[{callerMemberName}] Video Paused={_isPaused} (Result:'{result.Message}')");

            return result;
        }

        public async UniTask<bool> Video_GetIsPaused()
        {
            var strResult = await this.ExecuteJavaScript("getVideoIsPaused()");

            this.Log($"Video is paused = {strResult}");

            return string.Equals(strResult, "true", StringComparison.OrdinalIgnoreCase);
        }

        public async UniTask<MiceWebViewMessage.VideoRes.Resolution> Video_GetResolution()
        {
            MiceWebViewMessage.VideoRes.Resolution res;

            try
            {
                var json = await this.ExecuteJavaScript("getVideoRes()");
                res = JsonUtility.FromJson<MiceWebViewMessage.VideoRes.Resolution>(json);
            }
            catch (Exception e)
            {
                this.Log($"<color=red><Exception></color> {e.Message}");

                res = new MiceWebViewMessage.VideoRes.Resolution() { width = 0, height = 0 };
            }

            return res;
        }

        public async UniTask<float> Video_GetCurrentTime()
        {
            var result = await this.ExecuteJavaScript("getCurrentVideoTime()");

            return float.TryParse(result, out var value) ? value : 0;
        }

        public UniTask<JSResult> Video_SetCurrentTime(float seconds) => this.ExecuteJavaScript($"setCurrentVideoTime({seconds})");

        public UniTask<JSResult> Video_SetCurrentTimeFast(float seconds) => this.ExecuteJavaScript($"setCurrentVideoTimeFast({seconds})");

        public UniTask<JSResult> Video_SetTimelineVisible(bool value)
        {
            if (this.IsYouTubeState) return UniTask.FromResult(new JSResult("Not Allowed!"));

            var control = !value ? "video::-webkit-media-controls-timeline { display: none; }" : "";

            return this
                .ExecuteJavaScript
                (
                    @$"(() => {{
                        var styleElement = document.createElement('style');
                        styleElement.innerText = `{control}`;
                        document.head.appendChild(styleElement);
                    }})()"
                );
        }

        public UniTask<JSResult> Video_SetControlsVisible(bool visible)
        {
            if (this.IsYouTubeState) return UniTask.FromResult(new JSResult("Not Allowed!"));

            return this.ExecuteJavaScript($"setVideoControlsVisible({(visible ? "true" : "false")})");
        }

        private float _lastVideoPlaybackRate = 1.0f;

        public UniTask<JSResult> Video_SetDefaultPlaybackRate(float rate)
            => this.ExecuteJavaScript($"setVideoDefaultPlaybackRate({rate})");

        public UniTask<JSResult> Video_SetPlaybackRate(float rate)
        {
            _lastVideoPlaybackRate = rate;
            return this.ExecuteJavaScript($"setVideoPlaybackRate({rate})");
        }

        public async UniTask<float> Video_GetPlaybackRate()
        {
            var result = await this.ExecuteJavaScript("getVideoPlaybackRate()");

            var rate = float.TryParse(result, out var value) ? value : 1;

            _lastVideoPlaybackRate = rate;

            return rate;
        }
    }
}
