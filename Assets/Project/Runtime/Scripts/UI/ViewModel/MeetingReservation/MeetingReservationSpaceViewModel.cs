/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationSpaceViewModel.cs
* Developer:	ksw
* Date:			2023-07-24 15:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.AssetSystem;
using Com2Verse.WebApi.Service;

namespace Com2Verse.UI
{
    public sealed class MeetingReservationSpaceViewModel : ViewModelBase
    {
        private bool    _spaceSelected;
        private Texture _spaceTexture;
        private string  _spaceTitle;
        private string  _spaceDescription;

        private long _templateId;
        private int  _maxUserLimit;

        private Action<long, int> _onSelected;

        public CommandHandler CommandSpaceSelected { get; }
        
        public MeetingReservationSpaceViewModel()
        {
            CommandSpaceSelected = new CommandHandler(OnSpaceSelected);
        }

        public void Initialize(Components.MeetingTemplate template, Action<long, int> onSelected, bool selected = false)
        {
            var handle = C2VAddressables.LoadAssetAsync<Texture>(NetworkUIManager.Instance.SpaceTemplates[template.MeetingTemplateId].ImgRes);
            handle.OnCompleted += (internalHandle) =>
            {
                SpaceTexture     = internalHandle.Result;
                SpaceTitle       = Localization.Instance.GetString(NetworkUIManager.Instance.SpaceTemplates[template.MeetingTemplateId].Title);
                SpaceDescription = Localization.Instance.GetString(NetworkUIManager.Instance.SpaceTemplates[template.MeetingTemplateId].Description);
                SpaceSelected    = selected;
                handle.Release();
            };

            _templateId   = template.MeetingTemplateId;
            _maxUserLimit = template.MaxUserLimit;
            _onSelected   = onSelected;
            if (selected)
                OnSpaceSelected();
        }

#region Property
        public Texture SpaceTexture
        {
            get => _spaceTexture;
            set => SetProperty(ref _spaceTexture, value);
        }

        public bool SpaceSelected
        {
            get => _spaceSelected;
            set => SetProperty(ref _spaceSelected, value);
        }

        public string SpaceTitle
        {
            get => _spaceTitle;
            set => SetProperty(ref _spaceTitle, value);
        }

        public string SpaceDescription
        {
            get => _spaceDescription;
            set => SetProperty(ref _spaceDescription, value);
        }
#endregion

        private void OnSpaceSelected()
        {
            _onSelected?.Invoke(_templateId, _maxUserLimit);
            SpaceSelected = true;
        }
    }
}
