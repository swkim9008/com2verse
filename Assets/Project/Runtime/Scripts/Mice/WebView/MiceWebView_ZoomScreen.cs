/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_ZoomScreen.cs
* Developer:	sprite
* Date:			2023-07-19 10:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

//#define TEST_VIDEOCONTROLLER

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // Zoom Screen
    {
        private UI.GUIView _guiViewZoomScreen;
        private RawImage _lastRawImage;

        public bool IsZoomed { get; private set; } = false;

        public async UniTask OpenZoomScreen()
        {
            if (_guiViewZoomScreen != null && _guiViewZoomScreen) return;
            if (_isTutorialCutscenePlaying || _isTeleporting || !_isAbleToZoomableState) return;

            _guiViewZoomScreen = await MiceUIConferenceScreenZoomViewModel
                .ShowView(this.OnShowZoomScreen, this.OnHideZoomScreen);
        }

        private void OnShowZoomScreen(Transform transform)
        {
            if (transform == null || !transform) return;

            if (transform.TryGetComponent(out RawImage rawImage))
            {
                this.Log("Set Zoom Screen Material.");

                if (rawImage.material == rawImage.defaultMaterial)
                {
                    rawImage.material = new Material(_surface.Material);
                }
                else
                {
                    _surface.ApplyTo(rawImage.material);
                }

                rawImage.SetVerticesDirty();
                rawImage.SetMaterialDirty();
                rawImage.SetLayoutDirty();

                _lastRawImage = rawImage;
            }

            // 웹뷰 입력 컨트롤을 찾아 변경한다.
            if (!this.IsYouTubeState || YT.YT_USE_CONTROLS)
            {
                this.SetPointerInputDetectorFrom(transform);
            }

            UniTask.Void(async () =>
            {
                this.Log($"CurrentState = {this.CurrentState}");

                if (this.IsVideoState)
                {
                    var result = await this.Video_SetControlsVisible(true);
                    this.Log($"Video_SetControlsVisible(true) = '{result}'");
                    
                    // VOD 일 경우에만 Timeline을 표시한다.
                    await this.Video_SetTimelineVisible(this.CurrentState == eScreenState.VOD);
                }
            });

#if TEST_VIDEOCONTROLLER

            if (!_guiViewZoomScreen.gameObject.TryGetComponent<VideoController>(out var _))
            {
                _guiViewZoomScreen.gameObject.AddComponent<VideoController>();
            }

#endif

            this.IsZoomed = true;
        }

        private void OnHideZoomScreen()
        {
            _guiViewZoomScreen = null;

            // 웹뷰 입력 컨트롤 복구.
            this.SetPointerInputDetector(!this.IsYouTubeState);

            if (_lastRawImage != null && _lastRawImage && _lastRawImage.material != _lastRawImage.defaultMaterial)
            {
                this.Log("Reset Zoom Screen Material.");

                _lastRawImage.material = _lastRawImage.defaultMaterial;
                _lastRawImage.SetVerticesDirty();
                _lastRawImage.SetMaterialDirty();
                _lastRawImage.SetLayoutDirty();

                _lastRawImage = null;
            }
            else
            {
                this.Log("Invalid Zoom Screen Material!");
            }

            UniTask.Void(async () =>
            {
                this.Log($"CurrentState = {this.CurrentState}");

                if (this.IsVideoState)
                {
                    var result = await this.Video_SetControlsVisible(false);
                    this.Log($"Video_SetControlsVisible(false) = '{result}'");
                }
            });

            this.IsZoomed = false;
        }

        public void CloseZoomScreen()
        {
            if (_guiViewZoomScreen == null || !_guiViewZoomScreen) return;

            _guiViewZoomScreen.Hide();
            _guiViewZoomScreen = null;
        }
    }
}

#if UNITY_EDITOR && TEST_VIDEOCONTROLLER
namespace Com2Verse.Mice
{
    public class VideoController : MonoBehaviour
    {
        const float DEFAULT_CONTROL_WIDTH = 220;
        const float DEFAULT_CONTROL_HEIGHT = 60;

        private static readonly string[] POPUP_PLAYBACKRATE_LIST = new[]
        {
            "x0.25",
            "x0.5",
            "x0.75",
            "Normal",
            "x1.25",
            "x1.75",
            "x2",
        };

        private static readonly float[] PLAYBACKRATE_LIST = new[]
        {
            0.25f,
            0.5f,
            0.75f,
            1.0f,
            1.25f,
            1.75f,
            2.0f,
        };

        private MiceWebView _current;
        private ValueUpdater<float> _volumeUpdater = new();
        private ValueUpdater<float> _timeUpdater = new();
        private float _volume = 1.0f;
        private float _currentTime = 0;
        private int _currentPlaybackRateIndex = 3;
        private float _currentPlaybackRate = 1.0f;

        private void OnEnable()
        {
            this.VideoPlayerIsValid();

            _volumeUpdater
                .Start
                (
                    () => _volume,
                    value => MiceWebView.CurrentVideoPlayer.Video_SetVolume(value),
                    this.GetCancellationTokenOnDestroy()
                );

            _timeUpdater
                .Start
                (
                    () => _currentTime,
                    value => MiceWebView.CurrentVideoPlayer.Video_SetCurrentTimeFast(value),
                    this.GetCancellationTokenOnDestroy()
                );

            this.UpdateVolume();
            this.UpdateCurrentTime();
            this.UpdatePlaybackRate();
        }

        private void OnDisable()
        {
            _current = null;

            _volumeUpdater.Stop();
            _timeUpdater.Stop();
        }

        private void UpdateVolume()
        {
            if (MiceWebView.CurrentVideoPlayer == null || !MiceWebView.CurrentVideoPlayer) return;

            _volume = MiceWebView.CurrentVideoPlayer.CurrentVideoVolume;
        }

        private void UpdateCurrentTime()
        {
            if (MiceWebView.CurrentVideoPlayer == null || !MiceWebView.CurrentVideoPlayer) return;

            _currentTime = MiceWebView.CurrentVideoPlayer.CurrentVideoTime;
        }

        private void UpdatePlaybackRate()
        {
            if (MiceWebView.CurrentVideoPlayer == null || !MiceWebView.CurrentVideoPlayer) return;


            var value = MiceWebView.CurrentVideoPlayer.CurrentVideoPlaybackRate;

            var resValue = SnapValue(value, out var idx);

            Logger.C2VDebug.Log($"[VideoController] (SnapValue) {value} => ({resValue}, {idx})");

            (_currentPlaybackRateIndex, _currentPlaybackRate) = MiceWebView.CurrentVideoPlayer.CurrentVideoPlaybackRate switch
            {
                <= 0.375f   => (0, 0.25f),
                <= 0.625f   => (1, 0.5f),
                <= 0.875f   => (2, 0.75f),
                <= 1.125f   => (3, 1.0f),
                <= 1.5f     => (4, 1.25f),
                <= 1.875f   => (5, 1.75f),
                > 1.875f    => (6, 2.0f),
                _ => (3, 1.0f),
            };

            Logger.C2VDebug.Log($"[VideoController] (Switch Pattern) {value} => ({_currentPlaybackRate}, {_currentPlaybackRateIndex})");

            static float SnapValue(float value, out int index)
            {
                index = 3;

                for (int i = 0, cnt = PLAYBACKRATE_LIST.Length; i < cnt; i++)
                {
                    if (value <= PLAYBACKRATE_LIST[i])
                    {
                        index = i;
                        return Mathf.Abs(Mathf.Round(value - PLAYBACKRATE_LIST[i]));
                    }
                }

                return 1.0f;
            }
        }

        private bool VideoPlayerIsValid()
        {
            if (MiceWebView.CurrentVideoPlayer == null || !MiceWebView.CurrentVideoPlayer)
            {
                _current = null;
                return false;
            }

            if (_current == null || _current != MiceWebView.CurrentVideoPlayer)
            {
                _current = MiceWebView.CurrentVideoPlayer;
                _playState = MiceWebView.CurrentVideoPlayer.IsPaused ? PlayState.Paused : PlayState.Playing;
            }

            return true;
        }

        private void OnGUI()
        {
            if (!this.VideoPlayerIsValid()) return;

            if (_styleButton == null)
            {
                _styleButton = new GUIStyle(GUI.skin.button);
                _styleButton.fontSize = 20;
                _styleButton.fontStyle = FontStyle.Bold;
                _styleButton.alignment = TextAnchor.MiddleCenter;
                _styleButton.wordWrap = true;
            }

            if (_styleLabel == null)
            {
                _styleLabel = new GUIStyle(GUI.skin.label);
                _styleButton.fontSize = 20;
                _styleButton.fontStyle = FontStyle.Bold;
                _styleButton.alignment = TextAnchor.MiddleCenter;
            }

            using (new GUILayout.AreaScope(new Rect(10, 10, 1880, 1040)))
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(200);
                using (new GUILayout.HorizontalScope())
                {
                    DrawButton(_displayPlayState, width: 60, onClick: this.OnTogglePlayPause);
                    DrawVolumeSlider();
                }

                DrawPlayTimeAndTimeLine();
                DrawPlaybackRate();
            }
        }

        public enum PlayState
        {
            Paused,
            Playing,
        }

        private PlayState _playState = PlayState.Paused;
        private string _displayPlayState => _playState == PlayState.Playing ? "||" : "▶";

        private void OnTogglePlayPause()
        {
            _playState = _playState == PlayState.Paused ? PlayState.Playing : PlayState.Paused;

            switch (_playState)
            {
                case PlayState.Playing: MiceWebView.CurrentVideoPlayer.Video_Play().Forget(); break;
                case PlayState.Paused: MiceWebView.CurrentVideoPlayer.Video_Stop().Forget(); break;
            }            
        }

        #region GUI
        private GUIStyle _styleButton;
        private GUIStyle _styleLabel;

        private void DrawButton(string caption, float width = DEFAULT_CONTROL_WIDTH, float height = DEFAULT_CONTROL_HEIGHT, Action onClick = null)
        {
            if (GUILayout.Button(caption, _styleButton, GUILayout.Width(width), GUILayout.Height(height)) && onClick != null)
            {
                Event.current.Use();
                onClick?.Invoke();
            }
        }

        private void DrawVolumeSlider()
        {
            GUI.changed = false;
            _volume = GUILayout.HorizontalSlider(_volume, 0.0f, 1.0f, GUILayout.Width(DEFAULT_CONTROL_WIDTH), GUILayout.Height(DEFAULT_CONTROL_HEIGHT));
            if (GUI.changed)
            {
                Event.current.Use();

                _volumeUpdater.SetValue(_volume);
            }
        }

        private void DrawPlayTimeAndTimeLine()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"{MiceWebView.CurrentVideoPlayer.DisplayCurrentVideoTime}/{MiceWebView.CurrentVideoPlayer.DisplayCurrentVideoDuration}", _styleLabel, GUILayout.Width(100));

                GUI.changed = false;
                var normalizedTime = GUILayout.HorizontalSlider(MiceWebView.CurrentVideoPlayer.CurrentVideoNormalizedTime, 0.0f, 1.0f, GUILayout.Width(400), GUILayout.Height(DEFAULT_CONTROL_HEIGHT));
                if (GUI.changed)
                {
                    Event.current.Use();

                    _currentTime = normalizedTime * MiceWebView.CurrentVideoPlayer.CurrentVideoDuration;
                    _timeUpdater.SetValue(_currentTime);
                }
            }
        }

        private void DrawPlaybackRate()
        {
            GUI.changed = false;
            var selIndex = UnityEditor.EditorGUILayout.Popup("Playback Rate", _currentPlaybackRateIndex, POPUP_PLAYBACKRATE_LIST, GUILayout.Width(200));
            if (GUI.changed)
            {
                _currentPlaybackRateIndex = selIndex;
                _currentPlaybackRate = PLAYBACKRATE_LIST[_currentPlaybackRateIndex];

                MiceWebView.CurrentVideoPlayer.Video_SetPlaybackRate(_currentPlaybackRate).Forget();
            }
        }

        #endregion
    }

    public class ValueUpdater<T>
    {
        private Stack<T> _valueStack = new();
        private object _lockObj = new();
        private CancellationTokenSource _cts;

        public void SetValue(T value)
        {
            lock (_lockObj)
            {
                _valueStack.Push(value);
            }
        }

        private static void Log(string msg)
            => Logger.C2VDebug.Log($"[ValueUpdater({typeof(T)})] {msg}");

        public void Start(Func<T> currentValue, Func<T, UniTask> valueSetter, CancellationToken cancellationToken = default)
            => this.StartProcess(currentValue, valueSetter, cancellationToken).Forget();

        private async UniTaskVoid StartProcess(Func<T> currentValue, Func<T, UniTask> valueSetter, CancellationToken cancellationToken)
        {
            if (_cts != null)
            {
                Log($"Already running...");
                return;
            }

            try
            {
                Log($"Start...");

                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var token = _cts.Token;

                var curValue = currentValue();

                while (!token.IsCancellationRequested)
                {
                    Log($"Wait for signal...");

                    await UniTask.WaitWhile(() => _valueStack.Count == 0, cancellationToken: token);

                    Log($"Signal received!");

                    T lastValue = curValue;

                    lock (_lockObj)
                    {
                        // 마지막 값만 취하고 버린다.
                        lastValue = _valueStack.Pop();
                        _valueStack.Clear();
                    }

                    if (!EqualityComparer<T>.Default.Equals(lastValue, curValue))
                    {
                        Log($"Change Target Value = {lastValue}");

                        await valueSetter(lastValue);
                    }
                    else
                    {
                        Log($"Skip! ({lastValue})");
                    }

                    curValue = currentValue();
                }
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;

                Log($"Done.");
            }
        }

        public void Stop() => _cts?.Cancel();
    }
}
#endif
