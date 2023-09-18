/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIEnterHallSessionViewModel.cs
* Developer:	seaman2000
* Date:			2023-05-10 13:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.UI;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;
using System;
using Com2Verse.Utils;

namespace Com2Verse
{
    [ViewModelGroup("Mice")]
    public sealed class MiceUIEnterHallSessionSpeakerInfoViewModel : MiceViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_SpeakerInfo";

        private string _speakerName;
        private string _speakerAffiliation;
        private string _speakerDescription;
        private Texture2D _speakerImage;

        private MiceSessionInfo.Speaker _speaker;

        public string SpeakerName
        {
            get => _speakerName;
            set => SetProperty(ref _speakerName, value);
        }
        public string SpeakerAffiliation
        {
            get => _speakerAffiliation;
            set => SetProperty(ref _speakerAffiliation, value);
        }

        public string SpeakerDescription
        {
            get => _speakerDescription;
            set => SetProperty(ref _speakerDescription, value);
        }

        public Texture2D SpeakerImage
        {
            get => _speakerImage;
            set => SetProperty(ref _speakerImage, value);
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

        public void SetData(MiceSessionInfo.Speaker speakerInfo)
        {
            _speaker = speakerInfo;

            this.SpeakerName = speakerInfo.StrName;
            this.SpeakerAffiliation = speakerInfo.StrAffiliation;
            this.SpeakerDescription = speakerInfo.StrDescription;

            UpdateSpekerImage(speakerInfo.PhotoUrl).Forget();
        }

        async UniTask UpdateSpekerImage(string imageURL)
        {
            await UniTask.WaitWhile(() => string.IsNullOrEmpty(imageURL));
            this.SpeakerImage = await TextureCache.Instance.GetOrDownloadTextureAsync(imageURL);
        }
    }
}
