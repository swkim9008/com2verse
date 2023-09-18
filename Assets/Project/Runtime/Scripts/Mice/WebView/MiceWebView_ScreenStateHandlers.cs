/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_ScreenStateHandlers.cs
* Developer:	sprite
* Date:			2023-07-28 10:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using Com2Verse.Utils;
using System.Threading;
using UnityEngine;
using System;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // Screen State (eScreenState) Handlers
    {
        [ScreenState(eScreenState.NONE)]
        private async UniTask OnScreenState_NONE()
        {
            this.Log("Stop Video.");
            await this.Video_Stop();

            if (_isSplitScreenController)
            {
                this.ResetSplitSurfaces();
            }
            else
            {
                this.ResetSurface();
            }
        }

        [ScreenState(eScreenState.WEB_PAGE)]
        private async UniTask OnScreenState_WEB_PAGE()
        {
            (var _, string parameters, object extraParameter, CancellationToken cancellationToken) = _currentSSParams;

            if (await this.LoadUrl(parameters).WithCancellationToken(cancellationToken))
            {
                if (_isSplitScreenController)
                {
                    await this.SetSplitSurfaces(extraParameter as string, pointerInput: true);
                }
                else
                {
                    this.SetSurface(pointerInput: true);
                }
            }
            else
            {
                this.ResetSurface();
            }
        }

        [ScreenState(eScreenState.STREAMING)]
        [ScreenState(eScreenState.VOD)]
        private async UniTask OnScreenState_Videos()
        {
            (eScreenState state, string parameters, object extraParameter, CancellationToken cancellationToken) = _currentSSParams;

            var isVOD = state == eScreenState.VOD;

            var mediaType = MiceService.MEDIA_TYPE_CUSTOM_HLS;

            if (isVOD)
            {
                var strs = parameters.Split('.');
                var type = strs[strs.Length - 1].ToLower();

                mediaType = type switch
                {
                    "ogg"   => MiceService.MEDIA_TYPE_VIDEO_OGG,
                    "mp4"   => MiceService.MEDIA_TYPE_VIDEO_MP4,
                    "webm"  => MiceService.MEDIA_TYPE_VIDEO_WEBM,
                    _       => MiceService.MEDIA_TYPE_VIDEO_MP4
                };
            }

            var url = MiceService.GetHLSVideoWebPage(parameters, mediaType, showControls: false, isDebug: true);

            this.Log($"({state}) MediaType={mediaType} URL='{url}'");

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

                //this.Log($"Play the video continuously... ({this.CurrentState})");
                //this.Video_Play().Forget();

                this.CheckMiceServiceStateForVideo(true);
            }
            else
            {
                this.ResetSurface();
            }
        }

        [ScreenState(eScreenState.STANDBY_IMAGE)]
        private async UniTask OnScreenState_STANDBY_IMAGE()
        {
            (var _, string parameters, object extraParameter, var _) = _currentSSParams;

            this.Log("Stop Video.");
            await this.Video_Stop();

            bool fail = true;

            this.Log($"Get Picture. ({parameters})");

            if (!string.IsNullOrEmpty(parameters))
            {
                var texImage = await TextureCache.Instance.GetOrDownloadTextureAsync(parameters);

                if (texImage != null && texImage)
                {
                    if (_isSplitScreenController)
                    {
                        await this.SetSplitSurfaces(extraParameter as string, texImage, flip: true);
                    }
                    else
                    {
                        this.SetSurface(texImage, flip: true);
                    }

                    fail = false;
                }
            }

            if (fail)
            {
                this.ResetSurface();
            }
        }

        [ScreenState(eScreenState.SUB_MONITOR)]
        private async UniTask OnScreenState_SUB_MONITOR()
        {
            this.Log("Stop Video.");
            await this.Video_Stop();

            (var _, string parameters, object extraParameter, var _) = _currentSSParams;

            TexCoordInfo info;
            Texture screenTex = extraParameter as Texture;

            try
            {
                info = JsonUtility.FromJson<TexCoordInfo>(parameters);
            }
            catch(Exception e)
            {
                NamedLoggerTag.Sprite.LogError($"<Exception> {e.Message}\r\n{e.StackTrace}");
                info = null;
            }

            this.SetSurface(pointerInput: false, texture: screenTex, applyTexCoord: info != null && screenTex != null, texCoord: info.ToRect());
        }
    }
}
