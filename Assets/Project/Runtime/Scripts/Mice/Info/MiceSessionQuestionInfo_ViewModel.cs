/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSessionQuestionInfo_ViewModel.cs
* Developer:	sprite
* Date:			2023-05-17 15:27
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
    [ViewModelGroup("Mice")]
    public partial class MiceSessionQuestionInfoItemViewModel : MiceViewModelForInfo<MiceSessionQuestionInfoItemViewModel, MiceSessionQuestionInfo.Item>
    {
        public string QuestionTitle => _data.QuestionTitle;
        public string QuestionDescription => _data.QuestionDescription;
        public int LikeCount => _data.LikeCount;
        public int ViewCount => _data.ViewCount;
        public string CreateDateTime => $"{_data.CreateDateTime.ToString(Data.Localization.eKey.MICE_UI_SessionHall_PreQuestion_Popup_RegistrationDate.ToLocalizationString())}";
        public bool IsMine => _data.IsMine;
        public bool IsLikeClicked => _data.IsLikeClicked;

        public MiceSessionQuestionInfoItemViewModel(MiceSessionQuestionInfo.Item data, params ViewModel[] nestedViewModels)
            : base(data, false, nestedViewModels)
        {
        }
    }

    [ViewModelGroup("Mice")]
    public partial class MiceSessionQuestionInfoViewModel : MiceViewModelForInfo<MiceSessionQuestionInfoViewModel, MiceSessionQuestionInfo>
    {
        public MiceSessionQuestionInfoViewModel(MiceSessionQuestionInfo data)
            : base(data)
        {
            this.Data = new Collection<MiceSessionQuestionInfoItemViewModel>();
            this.Data.SetCapacity(100);

            this.RefreshCollection();

            this._data.DataChanged += this.RefreshCollection;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            this._data.DataChanged -= this.RefreshCollection;
        }

        public Collection<MiceSessionQuestionInfoItemViewModel> Data { get; private set; }

        public void RefreshCollection()
        {
            this.Data.Reset();
            this.Data.AddRange
            (
                 // 내 질문 먼저, 최신 질문 순 정렬.
                 _data.Data
                    .OrderByDescending(e => e.IsMine)
                    .ThenByDescending(e => e.CreateDateTime.Ticks)
                    .Select(e => e.CreateViewModel(this.GenerateNestedViewModels(e)))
                    .ToList()
            );
        }
    }
}
