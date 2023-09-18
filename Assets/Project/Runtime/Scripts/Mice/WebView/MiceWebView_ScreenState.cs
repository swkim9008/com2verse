/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_ScreenState.cs
* Developer:	sprite
* Date:			2023-07-19 10:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using Com2Verse.Network;
using System.Linq;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // Screen State (eScreenState)
    {
        public enum eScreenType
        {
            DONTCARE,
            MAIN,
            SUB
        }

        public enum eScreenState
        {
            NONE,
            STREAMING,
            VOD,
            YOUTUBE,
            STANDBY_IMAGE,
            WEB_PAGE,
            SUB_MONITOR,    // 외부 입력으로 영상 표시.
        }

        public eScreenState CurrentState { get; private set; } = eScreenState.NONE;
        private eScreenState _reservedState = eScreenState.NONE;

        public bool IsYouTubeState => this.CurrentState == eScreenState.YOUTUBE;
        public bool IsVideoState => this.CurrentState == eScreenState.VOD || this.CurrentState == eScreenState.STREAMING || this.CurrentState == eScreenState.YOUTUBE;

        /// <summary>
        /// 화면 확대(또는 상세 정보) 표시 가능한 상태인지 여부
        /// </summary>
        private bool _isAbleToZoomableState => this.CurrentState != eScreenState.NONE && this.CurrentState != eScreenState.STANDBY_IMAGE;

        public async UniTask SetScreenState(eScreenState state, string parameters, object extraParameter = null, CancellationToken cancellationToken = default)
        {
            if (_isSplitScreenController && state == eScreenState.SUB_MONITOR)
            {
                this.Log($"SUB_MONITOR state not allowed!");
                return;
            }

            if (_reservedState != eScreenState.NONE)
            {
                this.Log($"Busy ({_reservedState}) (Ignore State={state}, Source='{parameters})");
                return;
            }

            _reservedState = state;

            this.Log($"Begin (State={state}, Source='{parameters}')");

            try
            {
                //await this.InitWebView();
                
                // 동영상 플레이 상태 일 경우, 플레이 가능 할 때까지 대기...
                if (!this.CanPlayingLectureVideo && (_reservedState == eScreenState.STREAMING || _reservedState == eScreenState.VOD || _reservedState == eScreenState.YOUTUBE))
                {
                    this.Log($"(CanPlayingLectureVideo) Wait for playable state... (State={state}, Source='{parameters}')");

                    await UniTask.WaitUntil(() => this.CanPlayingLectureVideo);

                    this.Log($"(CanPlayingLectureVideo) Wait done. (State={state}, Source='{parameters}')");
                }

                await this.InvokeScreenStateHandler(state, parameters, extraParameter, cancellationToken);
            }
            finally
            {
                this.Log($"Done (State={state}, Source='{parameters}')");

                var lastState = this.CurrentState;
                this.CurrentState = _reservedState;
                _reservedState = eScreenState.NONE;

                if (this.IsVideoState)
                {
                    MiceWebView.CurrentVideoPlayer = this;
                }
                else if (this == MiceWebView.CurrentVideoPlayer)
                {
                    MiceWebView.CurrentVideoPlayer = null;
                }

#if UNITY_EDITOR
                this.ForceUpdateTestState(this.CurrentState);
#endif

                this.Log($"{lastState} --> {this.CurrentState}");
            }
        }
    }
}
