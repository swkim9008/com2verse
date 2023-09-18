/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIConferencePhotoShoot.cs
* Developer:	sprite
* Date:			2023-06-12 14:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using System;
using Com2Verse.UI;
using System.IO;
using System.Threading;

namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceUIConferencePhotoShootViewModel : MiceViewModel
	{
        /// <summary>
        /// View 리소스
        /// </summary>
        public enum PopupAssets
        {
            UI_Conference_PhotoShoot
        }

        /// <summary>
        /// 자동 닫기 지연 시간.
        /// </summary>
        const int HIDE_DELAY_MSEC = 1500;

#region Variables
        private Texture _photoShootTexture;
        private Color _photoShootColor;
        private bool _isUIVisible;
        private CancellationTokenSource _ctsPhotoShoot = null;
#endregion  // Variables

#region Properties
        public Texture PhotoShootTexture
        {
            get => _photoShootTexture;
            set => SetProperty(ref _photoShootTexture, value);
        }
        public Color PhotoShootColor
        {
            get => _photoShootColor;
            set => SetProperty(ref _photoShootColor, value);
        }
        public bool IsUIVisible
        {
            get => _isUIVisible;
            set => SetProperty(ref _isUIVisible, value);
        }

        public CommandHandler PhotoShootButton { get; private set; }
        #endregion  // Properties

        public MiceUIConferencePhotoShootViewModel()
        {
            this.PhotoShootButton = new CommandHandler(() => this.PhotoShoot().Forget());
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            this.IsUIVisible = true;
        }

        private async UniTask<bool> PhotoShoot()
        {
            if (_guiView != null && _guiView && _guiView.VisibleState == GUIView.eVisibleState.OPENING)
            {
                C2VDebug.LogCategory("Conference PhotoShoot", $"In Transition... Skip!");
                return false;
            }

            if (_ctsPhotoShoot != null) return false;

            bool result = false;

            try
            {
                _ctsPhotoShoot = new CancellationTokenSource();
                var token = _ctsPhotoShoot.Token;

                var biCapture = await MiceBICapture.GetOrInstantiate();

                // Take a screenshots...
                var generatedTexture = MiceUIConferencePhotoShootViewModel.Capture(biCapture.Camera);

                // Refresh UI...
                this.SetPicture(generatedTexture);
                
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: token);

                // Save...
                MiceUIConferencePhotoShootViewModel.SaveTexture(generatedTexture);

                result = true;

                Data.Localization.eKey.MICE_UI_SessionHall_Photo_Msg_Save.ShowAsToast();

                OpenInFileBrowser.Open(Utils.Path.Screenshots);

                this.IsUIVisible = false;

                await MiceUIConferencePhotoShootViewModel.HideView(HIDE_DELAY_MSEC, cancellationToken: token);

                this.RemovePicture(generatedTexture);
            }
            catch (Exception e)
            {
                C2VDebug.LogWarningCategory("Conference PhotoShoot", $"Error Occurred : {e.Message}");
                result = false;
            }
            finally
            {
                _ctsPhotoShoot?.Dispose();
                _ctsPhotoShoot = null;

                MiceBICapture.Remove();
            }

            return result;
        }

        public static Texture2D Capture(Camera camera, int width, int height, Camera uiCamera = null)
        {
            UnityEngine.Assertions.Assert.IsTrue(camera != null && camera);

            Texture2D newTex;

            var rt = RenderTexture.GetTemporary(width, height);
            {
                MiceUIConferencePhotoShootViewModel.Render(camera, rt, true);

                if (uiCamera != null && uiCamera)
                {
                    MiceUIConferencePhotoShootViewModel.Render(uiCamera, rt, false);
                }

                var oldRT = RenderTexture.active;
                RenderTexture.active = rt;
                {
                    newTex = new Texture2D(width, height);
                    newTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    newTex.Apply();
                }
                RenderTexture.active = oldRT;
            }
            RenderTexture.ReleaseTemporary(rt);

            return newTex;
        }

        public static Texture2D Capture() => MiceUIConferencePhotoShootViewModel.Capture(Camera.main, Screen.width, Screen.height);
        public static Texture2D Capture(Camera uiCamera) => MiceUIConferencePhotoShootViewModel.Capture(Camera.main, Screen.width, Screen.height, uiCamera);

        public static void Render(Camera cam, RenderTexture renderTexture, bool clearBeforeCapture, params string[] exceptLayers)
        {
            var oldCM = cam.cullingMask;

            int exceptLayerMask = 0;

            if (exceptLayers != null && exceptLayers.Length > 0)
            {
                for (int i = 0, cnt = exceptLayers.Length; i < cnt; i++)
                {
                    exceptLayerMask |= LayerMask.GetMask(exceptLayers[i]);
                }
            }

            cam.cullingMask &= ~exceptLayerMask;

            var oldTT = cam.targetTexture;
            cam.targetTexture = renderTexture;
            {
                var oldRT = RenderTexture.active;
                RenderTexture.active = cam.targetTexture;
                {
                    // Clear RenderTexture...
                    GL.Clear(clearBeforeCapture, clearBeforeCapture, Color.clear);

                    cam.Render();
                }
                RenderTexture.active = oldRT;
            }
            cam.targetTexture = oldTT;

            cam.cullingMask = oldCM;
        }


        private void SetPicture(Texture2D texture)
        {
            this.PhotoShootColor = Color.white;
            this.PhotoShootTexture = texture;
        }

        private void RemovePicture(Texture2D texture)
        {
            this.PhotoShootTexture = null;
            this.PhotoShootColor = new Color(0, 0, 0, 0);

            if (texture != null && texture) GameObject.Destroy(texture);
        }

#region 텍스쳐 저장 관련...
        const string PICTURE_FILE_NAME_FMT      = "ScreenShot_{0}.png";
        const string PICTURE_FILE_DATETIME_SIG  = "yyMMddHHmmssff";

        public static void SaveTexture(Texture2D texture)
        {
            static string CombinePath(params string[] paths) => Path.Combine(paths).Replace('\\', '/');

            var path = Utils.Path.Screenshots;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var file = CombinePath(path, string.Format(PICTURE_FILE_NAME_FMT, DateTime.Now.ToString(PICTURE_FILE_DATETIME_SIG)));
            File.WriteAllBytes(file, texture.EncodeToPNG());
        }
#endregion  // 텍스쳐 저장 관련...
    }

    public sealed partial class MiceUIConferencePhotoShootViewModel // Show/Hide View
    {
        // 이 GUIView 는 Only One이므로, 가능.
        private static Func<GUIView> _hideView;
        private static GUIView _guiView = null;

        public static UniTask<GUIView> ShowView(Action onHide = null)
            => MiceViewModel.ShowView
            (
                PopupAssets.UI_Conference_PhotoShoot,
                onShow: v =>
                {
                    _hideView = v.Hide;
                },
                onHide: _ =>
                {
                    _hideView = null;

                    onHide?.Invoke();

                    _guiView = null;
                },
                onOpening: v =>
                {
                    _guiView = v;
                }
            );

        public static async UniTask HideView(int millisecondsDelay = 0, CancellationToken cancellationToken = default)
        {
            if (millisecondsDelay > 0)
            {
                await UniTask.Delay(millisecondsDelay, true, cancellationToken: cancellationToken);
            }

            _hideView?.Invoke();

            // View가 감춰 질 때 까지 대기...
            await UniTask.WaitUntil(() => _hideView != null, cancellationToken: cancellationToken);
        }

        public static void ToggleView(bool value, Action onHide)
        {
            if (value)
            {
                MiceUIConferencePhotoShootViewModel.ShowView(onHide).Forget();
            }
            else
            {
                MiceUIConferencePhotoShootViewModel.HideView().Forget();
            }
        }
    }
}
  
