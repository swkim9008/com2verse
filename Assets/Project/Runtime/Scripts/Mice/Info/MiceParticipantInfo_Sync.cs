/*===============================================================
* Product:		Com2Verse
* File Name:	MiceParticipantInfo_Sync.cs
* Developer:	sprite
* Date:			2023-04-26 11:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ParticipantEntity = Com2Verse.Mice.MiceWebClient.Entities.Participant;

namespace Com2Verse.Mice
{
    public partial class MiceParticipantInfo : IMiceSyncData<ParticipantEntity>   // Server Data Cache and Sync.
    {
        List<ParticipantEntity> IMiceSyncData<ParticipantEntity>.Cache { get => _data?.Select(e => e.Entity).ToList(); set { } }

        internal static bool CompareParticipantEntity(ParticipantEntity a, ParticipantEntity b)
            =>  a.AccountId == b.AccountId &&
                a.IsPublic == b.IsPublic;

        void IMiceSyncData<ParticipantEntity>.OnAdd(ParticipantEntity fromServer)
        {
            _data.Add(new Item(fromServer));
        }

        void IMiceSyncData<ParticipantEntity>.OnUpdate(ParticipantEntity cached)
        {
            var item = _data.FirstOrDefault(e => CompareParticipantEntity(e.Entity, cached));
            if (item != null)
            {
                item.SetFrom(cached);
            }
        }

        void IMiceSyncData<ParticipantEntity>.OnRemove(ParticipantEntity cached)
        {
            var item = _data.FirstOrDefault(e => CompareParticipantEntity(e.Entity, cached));
            if (item != null)
            {
                _data.Remove(item);
            }
        }

        public async UniTask Sync()
        {
            var result = await MiceWebClient.Participant.ParticipantGet_MiceType_RoomId(MiceService.Instance.CurMiceType, MiceService.Instance.CurRoomID);
            if (result)
            {
                this.UpdateCache(result.Data, CompareParticipantEntity);
            }

#if UNITY_EDITOR || ENV_DEV
            // 에디터 상에서 로그인하지 않은 경우, 더미 데이터를 생성한다.(기능 테스트용)
            if (MiceBaseInfo.NeedToCreateDummyData(_data))
            {
                this.UpdateCache(MiceParticipantInfo.GenerateTemporaryData().ToList(), CompareParticipantEntity);
            }
#endif
        }

        void IMiceSyncData<ParticipantEntity>.OnDataChanged()
        {
            this.RefreshContentsCount();
            this.OnDataChange?.Invoke();
        }
    }

#if UNITY_EDITOR || ENV_DEV
    public partial class MiceParticipantInfo // Dummy Data.
    {
        private const int DEFAULT_DUMMY_SIZE = 100;

        private static readonly string FNAME_SAMPLES = "김이박최정강조윤장임주";
        private static readonly string MNAME_SAMPLES = "민서예지도하주윤채현지준";
        private static readonly string LNAME_SAMPLES = "준지윤우원호후하서연아은진";

        public static string GenerateRandomName()
        {
            static string RandomPickOne(string source) => $"{source[UnityEngine.Random.Range(0, source.Length)]}";

            return RandomPickOne(FNAME_SAMPLES) +
                   RandomPickOne(MNAME_SAMPLES) +
                   RandomPickOne(LNAME_SAMPLES);
        }

        public static IEnumerable<ParticipantEntity> GenerateTemporaryData(int size = DEFAULT_DUMMY_SIZE)
            => Enumerable.Range(0, size)
                .Select
                (
                    i => new ParticipantEntity()
                    {
                        AccountId = i,
                        CompanyName = "Com2Vers",
                        Nickname = GenerateRandomName(),
                        PhotoPath = "",
                        IsPublic = UnityEngine.Random.Range(0, 10000) >= 5000
                    }
                );
    }
#endif
}
