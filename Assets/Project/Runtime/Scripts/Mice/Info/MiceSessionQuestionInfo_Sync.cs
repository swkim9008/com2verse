/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSessionQuestionInfo_Sync.cs
* Developer:	sprite
* Date:			2023-05-17 15:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using System;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using QuestionEntity = Com2Verse.Mice.MiceWebClient.Entities.QuestionResult;


namespace Com2Verse.Mice
{
    public partial class MiceSessionQuestionInfo : IMiceSyncData<QuestionEntity>   // Server Data Cache and Sync.
    {
        List<QuestionEntity> IMiceSyncData<QuestionEntity>.Cache { get => this.Data?.Select(e => e.Entity).ToList(); set { } }

        internal static bool CompareQuestionEntity(QuestionEntity a, QuestionEntity b) => a.QuestionSeq == b.QuestionSeq;

        void IMiceSyncData<QuestionEntity>.OnAdd(QuestionEntity fromServer)
        {
            this.Data.Add(new Item(fromServer));
        }

        void IMiceSyncData<QuestionEntity>.OnUpdate(QuestionEntity cached)
        {
            var item = this.Data.FirstOrDefault(e => CompareQuestionEntity(e.Entity, cached));
            if (item != null)
            {
                item.SetFrom(cached);
            }
        }

        void IMiceSyncData<QuestionEntity>.OnRemove(QuestionEntity cached)
        {
            var item = this.Data.FirstOrDefault(e => CompareQuestionEntity(e.Entity, cached));
            if (item != null)
            {
                this.Data.Remove(item);
            }
        }

        public async UniTask Sync()
        {
            var result = await MiceWebClient.Question.QuestionGet_SessionId(MiceService.Instance.SessionID, 0, 100, "date");
            if (result)
            {
                this.UpdateCache(result.Data);
            }

#if UNITY_EDITOR || ENV_DEV
            // 에디터 상에서 로그인하지 않은 경우, 더미 데이터를 생성한다.(기능 테스트용)
            if (MiceBaseInfo.NeedToCreateDummyData(this.Data))
            {
                this.UpdateCache(MiceSessionQuestionInfo.GenerateTemporaryData(100).ToList(), CompareQuestionEntity);
            }
#endif
        }

        public virtual void OnDataChanged() => this.DataChanged?.Invoke();
    }

#if UNITY_EDITOR || ENV_DEV
    public partial class MiceSessionQuestionInfo    // Dummy Data
    {
        private const int DEFAULT_DUMMY_SIZE = 10;

        public static IEnumerable<QuestionEntity> GenerateTemporaryData(int size = DEFAULT_DUMMY_SIZE)
            => Enumerable.Range(0, size)
                .Select
                (
                    i => new QuestionEntity()
                    {
                        QuestionSeq         = i,
                        SessionId           = MiceService.Instance.SessionID,
                        AccountId           = User.Instance.CurrentUserData.ID,
                        CompanyName         = "Com2Vers",
                        PhotoPath           = "",
                        CreateDateTime      = DateTime.Now.AddMinutes(-10 * (size - i)),
                        LikeCount           = UnityEngine.Random.Range(0, 999),
                        ViewCount           = UnityEngine.Random.Range(0, 999),
                        QuestionDescription = Enumerable.Range(0, UnityEngine.Random.Range(3, 500)).Select(_ => "질문").Aggregate((a, b) => $"{a} {b}"),
                        QuestionTitle       = $"질문 제목 ({i:000})",
                        NickName            = User.Instance.CurrentUserData.UserName,
                        IsLikeClicked       = false
                    }
                );
    }
#endif
}


