/*===============================================================
* Product:		Com2Verse
* File Name:	MiceParticipantInfo.cs
* Developer:	sprite
* Date:			2023-04-12 16:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.UI;
using ParticipantEntity = Com2Verse.Mice.MiceWebClient.Entities.Participant;

namespace Com2Verse.Mice
{
    public partial class MiceParticipantInfo : MiceBaseInfo
    {
        static readonly int DEFAULT_CAPACITY = 100;

        public class Item : MiceBaseInfo
        {
            public MiceParticipantInfoItemViewModel ViewModel { get; private set; }
            public ParticipantEntity Entity { get; private set; }

            public string NickName => Entity.Nickname;
            public string CompanyName => Entity.CompanyName;
            public bool IsPublic => Entity.IsPublic;
            public string PhotoPath => Entity.PhotoPath;
            public string PhotoThumbnailUrl => Entity.PhotoThumbnailUrl;
            public long AccountId => Entity.AccountId;

            public Item(ParticipantEntity entity)
            {
                this.SetFrom(entity);

                this.ViewModel = null;
            }

            public MiceParticipantInfoItemViewModel CreateViewModel(params ViewModel[] nestedViewModels)
                => this.ViewModel = new MiceParticipantInfoItemViewModel(this, nestedViewModels);

            public Item SetFrom(ParticipantEntity entity)
            {
                this.Entity = entity;
                
                return this;
            }
        }

        public List<Item> Data => _modeType switch
        {
            eModeType.SEARCH_MODE => _searchData,
            _ => _data
        };

        public List<Item> TotalData => _data;

        private List<Item> _data = new List<Item>(DEFAULT_CAPACITY);

        private partial void InitSearch();

        public MiceParticipantInfo()
        {
            this.InitSearch();
        }
    }
}
