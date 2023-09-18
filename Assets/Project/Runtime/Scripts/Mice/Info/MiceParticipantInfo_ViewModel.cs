/*===============================================================
* Product:		Com2Verse
* File Name:	MiceParticipantInfo_ViewModel.cs
* Developer:	sprite
* Date:			2023-04-17 17:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Com2Verse.UI;

namespace Com2Verse.Mice
{
    /// <summary>
    /// <see cref="MiceParticipantInfo.Item"/> 의 <see cref="MiceViewModel"/>(<see cref="ViewModel"/>)
    /// </summary>
    [ViewModelGroup("Mice")]
    public partial class MiceParticipantInfoItemViewModel : MiceViewModelForInfo<MiceParticipantInfoItemViewModel, MiceParticipantInfo.Item>
    {
        public string NickName => _data.NickName;
        public string CompanyName => _data.CompanyName;
        public Texture ProfileImage => _profileImage
            .GetOrDownloadTexture
            (
                _data.PhotoThumbnailUrl, 
                tex =>
                {
                    this.SetProperty(ref _profileImage, tex);
                    InvokePropertyValueChanged(nameof(HasProfileImage), HasProfileImage);
                }
            );
        public bool IsPublic => _data.IsPublic;
        public bool HasProfileImage => _profileImage != null && _profileImage;

        private Texture _profileImage;

        public MiceParticipantInfoItemViewModel(MiceParticipantInfo.Item data, params ViewModel[] nestedViewModels)
            : base(data, false, nestedViewModels)
        {
            _profileImage = null;
        }
    }

    /// <summary>
    /// <see cref="MiceParticipantInfo"/> 의 <see cref="MiceViewModel"/>(<see cref="ViewModel"/>)
    /// </summary>
    [ViewModelGroup("Mice")]
    public partial class MiceParticipantInfoViewModel : MiceViewModelForInfo<MiceParticipantInfoViewModel, MiceParticipantInfo>
    {
        public MiceParticipantInfoViewModel(MiceParticipantInfo data)
            : base(data)
        {
            _data.OnDataChange += this.RefreshInstantDataCollection;

            _instantData = new Collection<MiceParticipantInfoItemViewModel>();
            _instantData.SetCapacity(100);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            _data.OnDataChange -= this.RefreshInstantDataCollection;
        }
    }

    public partial class MiceParticipantInfoViewModel   // Nested ViewModel Generator.
    {
        public override void RegisterNestedViewModels<TInfo>(params Func<TInfo, ViewModel>[] nestedViewModelGenerators)
        {
            base.RegisterNestedViewModels(nestedViewModelGenerators);

            this.RefreshInstantDataCollection();
        }

        public override void ClearNestedViewModels<TInfo>()
        {
            base.ClearNestedViewModels<TInfo>();

            this.RefreshInstantDataCollection();
        }
    }

    public partial class MiceParticipantInfoViewModel
    {
        public Collection<MiceParticipantInfoItemViewModel> InstantData => _instantData;
        public bool IsEmptyContents => InstantData.CollectionCount == 0;
        public bool HasNoResults => _data.ModeType == MiceParticipantInfo.eModeType.SEARCH_MODE && _data.SearchState == MiceParticipantInfo.eSearchModeState.NO_RESULTS;
        public bool MoreTwoLetters => _data.ModeType == MiceParticipantInfo.eModeType.SEARCH_MODE && _data.SearchState == MiceParticipantInfo.eSearchModeState.MORE_TWO_LETTERS;
        public bool IsSearchErrorMessageVisible => HasNoResults || MoreTwoLetters;
        public bool HasMoreContents => _data.HasMoreContents;
        public int ContentsCount => _data.ContentsCount;
        public int MaxContentsCount => _data.MaxContentsCount;
        public bool IsNormalMode => _data.IsNormalMode;
        public bool IsSearchMode => _data.IsSearchMode;

        private Collection<MiceParticipantInfoItemViewModel> _instantData;

        private void RefreshInstantDataCollection()
        {
            _instantData.Reset();
            _instantData.AddRange
            (
                _data.Data.Take(_data.ContentsCount)
                    .Select(e => e.CreateViewModel(this.GenerateNestedViewModels(e)))
                    .ToList()
            );

            C2VDebug.Log($"[MiceParticipantInfoViewModel.RefreshInstantDataCollection()] Count = {_instantData.CollectionCount}");

            InvokePropertyValueChanged(nameof(InstantData), InstantData);
            InvokePropertyValueChanged(nameof(HasNoResults), HasNoResults);
            InvokePropertyValueChanged(nameof(MoreTwoLetters), MoreTwoLetters);
            InvokePropertyValueChanged(nameof(IsSearchErrorMessageVisible), IsSearchErrorMessageVisible);
            InvokePropertyValueChanged(nameof(IsEmptyContents), IsEmptyContents);
            InvokePropertyValueChanged(nameof(HasMoreContents), HasMoreContents);
            InvokePropertyValueChanged(nameof(ContentsCount), ContentsCount);
            InvokePropertyValueChanged(nameof(MaxContentsCount), MaxContentsCount);
            InvokePropertyValueChanged(nameof(IsNormalMode), IsNormalMode);
            InvokePropertyValueChanged(nameof(IsSearchMode), IsSearchMode);
        }
    }
}
