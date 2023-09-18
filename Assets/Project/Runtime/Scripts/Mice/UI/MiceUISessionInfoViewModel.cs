/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSessionInfoViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-02 16:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using System;
using Com2Verse.UI;
using Com2Verse.Project.InputSystem;
using Com2Verse.InputSystem;
using UnityEngine;
using Com2Verse.Utils;
using Com2Verse.ScreenShare;
using UnityEngine.Pool;


namespace Com2Verse.Mice
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceUISessionInfoViewModel : MiceViewModel  // Main
    {
        private static readonly string UI_ASSET = "UI_SessionInfo";

        private Texture2D _bannerImage;
        private String _sessionName;
        private String _sessionSchedule;
        private string _sessionDescription;
        private string _sessionBannerLinkUrl;
        private Collection<MiceUISessionInfoSpeakerViewModel> _speakerCollection = new();
        private bool _hasSpeaker;

        public CommandHandler MiceUISessionInfoViewModel_BannerLinkButton { get; private set; }

        public Texture2D BannerImage
        {
            get => _bannerImage;
            set => SetProperty(ref _bannerImage, value);
        }

        public string SessionName
        {
            get => _sessionName;
            set => SetProperty(ref _sessionName, value);
        }

        public string SessionSchedule
        {
            get => _sessionSchedule;
            set => SetProperty(ref _sessionSchedule, value);
        }

        public string SessionDescription
        {
            get => _sessionDescription;
            set => SetProperty(ref _sessionDescription, value);
        }

        public bool HasSpeaker
        {
            get => _hasSpeaker;
            set => SetProperty(ref _hasSpeaker, value);
        }

        public Collection<MiceUISessionInfoSpeakerViewModel> SpeakerCollection
        {
            get => _speakerCollection;
            set
            {
                _speakerCollection = value;
                base.InvokePropertyValueChanged(nameof(SpeakerCollection), SpeakerCollection);
            }
        }


        public static async UniTask<GUIView> ShowView(Action<GUIView> onShow = null, Action<GUIView> onHide = null)
        {
            GUIView view = await UI_ASSET.AsGUIView();

            void OnOpenedEvent(GUIView view)
            {
                onShow?.Invoke(view);
            }

            void OnClosedEvent(GUIView view)
            {
                onHide?.Invoke(view);

                view.OnOpenedEvent -= OnOpenedEvent;
                view.OnClosedEvent -= OnClosedEvent;
            }

            view.OnOpenedEvent += OnOpenedEvent;
            view.OnClosedEvent += OnClosedEvent;


            view.Show();

            return view;
        }

        public MiceUISessionInfoViewModel()
        {
            this.MiceUISessionInfoViewModel_BannerLinkButton = new CommandHandler(OnClickBanner);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
        }

        private void OnClickBanner()
        {
            if (string.IsNullOrEmpty(_sessionBannerLinkUrl)) return;

            Application.OpenURL(_sessionBannerLinkUrl);
            //UIManager.Instance.ShowPopupWebView(true, new Vector2(1300, 800), _sessionBannerLinkUrl);
        }

        async UniTask UpdateCoverImage(string bannerURL)
        {
            await UniTask.WaitWhile(() => string.IsNullOrEmpty(bannerURL));
            this.BannerImage = await TextureCache.Instance.GetOrDownloadTextureAsync(bannerURL);
        }

        public void SyncData()
        {
            var sessionInfo = MiceService.Instance.GetCurrentSessionInfo();
            if (sessionInfo == null)
            {
                SyncDummy();
                return;
            }

            this.SessionName = $"{sessionInfo.StrName}";
            this.SessionSchedule = $"{sessionInfo.StrStartDataTime} ~ {sessionInfo.StrEndDataTime}";
            this.SessionDescription = $"{sessionInfo.StrDescription}";

            _sessionBannerLinkUrl = sessionInfo.StrBannerLinkUrl;

            // Banner는 Event 공용이다.
            UpdateCoverImage(sessionInfo.StrBannerImageURL).Forget(); //"http://34.64.230.210/MediaSamples/img002.jpg"

            // speaker
            this.HasSpeaker = sessionInfo.Speakers.Count > 0;

            this.SpeakerCollection.Reset();
            foreach (var entry in sessionInfo.Speakers)
            {
                var item = new MiceUISessionInfoSpeakerViewModel();
                item.SetSpeakerInfo(entry);
                this.SpeakerCollection.AddItem(item);
            }
        }


        public void SyncDummy()
        {
            var sessionInfo = MiceService.Instance.GetCurrentSessionInfo();
            if (sessionInfo != null) return;

            this.SessionName = $"테스트 세션 네임";
            this.SessionSchedule = $"어제 ~ 오늘";
            this.SessionDescription = $"테스트 세션 네임 세션 자세한 설명";

            UpdateCoverImage("http://34.64.230.210/MediaSamples/img002.jpg").Forget();

            // speaker
            this.HasSpeaker = sessionInfo.Speakers.Count > 0;

            this.SpeakerCollection.Reset();
            for (int loop = 0, max = 3; loop < max; ++loop)
            {
                var item = new MiceUISessionInfoSpeakerViewModel();
                this.SpeakerCollection.AddItem(item);
                item.SetSpeakerDummyInfo(loop + 1);
            }
        }
    }
}

