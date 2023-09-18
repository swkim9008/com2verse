/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSessionQuestionInfo.cs
* Developer:	sprite
* Date:			2023-05-17 14:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System;
using QuestionEntity = Com2Verse.Mice.MiceWebClient.Entities.QuestionResult;
using Com2Verse.UI;

namespace Com2Verse.Mice
{
    public partial class MiceSessionQuestionInfo : MiceBaseInfo
    {
        static readonly int DEFAULT_CAPACITY = 10;

        public class Item : MiceBaseInfo
        {
            public MiceSessionQuestionInfoItemViewModel ViewModel { get; private set; }
            public QuestionEntity Entity { get; private set; }

            public string QuestionTitle => this.Entity.QuestionTitle;
            public string QuestionDescription => this.Entity.QuestionDescription;
            public int LikeCount => this.Entity.LikeCount;
            public int ViewCount => this.Entity.ViewCount;
            public DateTime CreateDateTime => this.Entity.CreateDateTime.ToLocalTime();
            public bool IsMine => this.Entity.IsMine;
            public bool IsLikeClicked => this.Entity.IsLikeClicked;

            public Item(QuestionEntity entity)
            {
                this.SetFrom(entity);
            }

            public Item SetFrom(QuestionEntity entity)
            {
                this.Entity = entity;

                return this;
            }

            public MiceSessionQuestionInfoItemViewModel CreateViewModel(params ViewModel[] nestedViewModels)
                => this.ViewModel = new MiceSessionQuestionInfoItemViewModel(this, nestedViewModels);
        }

        public List<Item> Data { get; private set; } = new List<Item>(DEFAULT_CAPACITY);
        public bool IsEmpty => this.Data == null || this.Data.Count == 0;
        /// <summary>
        /// 데이터가 변경되었을 경우 호출된다.
        /// </summary>
        public event Action DataChanged;
    }
}
