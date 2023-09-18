/*===============================================================
* Product:		Com2Verse
* File Name:	MiceParticipantInfo_Search.cs
* Developer:	sprite
* Date:			2023-04-26 11:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Com2Verse.Mice
{
    public partial class MiceParticipantInfo    // Search.
    {
        /// <summary>
        /// 한 화면에 표시되는 최대 컨텐츠 갯수(또는 더보기 시, 추가 컨텐츠 갯수)
        /// </summary>
        public static readonly int INTERVAL_CONTENTS_COUNT = 30;

        /// <summary>
        /// 목록 표시 타입
        /// </summary>
        public enum eModeType
        {
            /// <summary>
            /// 일반 모드(모든 컨텐츠 표시)
            /// </summary>
            NORMAL_MODE,
            /// <summary>
            /// 검색 모드(검색 결과만 표시)
            /// </summary>
            SEARCH_MODE,
        }

        /// <summary>
        /// 검색 상태.
        /// </summary>
        public enum eSearchModeState
        {
            NONE,

            /// <summary>
            /// 결과 있음.
            /// </summary>
            OK,
            /// <summary>
            /// 결과 없음.
            /// </summary>
            NO_RESULTS,
            /// <summary>
            /// 두 글자 이상 입력.
            /// </summary>
            MORE_TWO_LETTERS,
        }

        public event Action OnDataChange;

        public eModeType ModeType => _modeType;
        public eSearchModeState SearchState => _searchState;
        public int ContentsCount { get; private set; } = 0;
        public int MaxContentsCount { get; private set; } = 0;
        public bool HasMoreContents => this.MaxContentsCount > 0 && this.ContentsCount < this.MaxContentsCount;
        public bool IsNormalMode => _modeType == eModeType.NORMAL_MODE;
        public bool IsSearchMode => _modeType == eModeType.SEARCH_MODE;

        private List<Item> _searchData;
        private string _lastSearchText = string.Empty;
        private eModeType _modeType = eModeType.NORMAL_MODE;
        private eSearchModeState _searchState = eSearchModeState.NONE;

        private partial void InitSearch()
        {
            _modeType = eModeType.NORMAL_MODE;
            _searchState = eSearchModeState.NONE;

            this.ContentsCount = 0;
            this.MaxContentsCount = 0;
        }

        private void ChangeMode(eModeType mode)
        {
            _modeType = mode;
            C2VDebug.Log($"[MiceParticipantInfo] {_modeType}");

            this.ChangeSearchState(eSearchModeState.NONE);
        }

        private void ChangeSearchState(eSearchModeState state)
        {
            _searchState = state;
            C2VDebug.Log($"[MiceParticipantInfo] Search State: {_searchState}");
        }

        public async UniTask SearchBy(string text)
        {
            C2VDebug.Log($"[MiceParticipantInfo.SearchBy()] text = {text}");

            if (string.Equals(_lastSearchText, text))
            {
                C2VDebug.Log($"[MiceParticipantInfo.SearchBy()] Skip.");
                return;
            }

            this.ChangeMode(eModeType.SEARCH_MODE);

            _lastSearchText = text;

            if (text != null) text = text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                this.ChangeMode(eModeType.NORMAL_MODE);

                this.RefreshContentsCount();

                this.OnDataChange?.Invoke();
                return;
            }

            void PrepareSearchData()
            {
                if (_searchData == null)
                {
                    _searchData = new List<Item>();
                }
                else
                {
                    _searchData.Clear();
                }
            }

            if (text.Length < 2)
            {
                this.ChangeSearchState(eSearchModeState.MORE_TWO_LETTERS);

                PrepareSearchData();
            }
            else
            {
                var encodedText = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(text));
                var result = await MiceWebClient.Participant.ParticipantGet_MiceType_RoomId_Query(MiceService.Instance.CurMiceType, MiceService.Instance.CurRoomID, encodedText);
                if (!result)
                {
                    PrepareSearchData();

                }
                else
                {
                    _searchData = result.Data.Select(e => new Item(e)).ToList();
                }

#if UNITY_EDITOR || ENV_DEV
                // 에디터상에서 로그인하지 않은 경우, 캐시데이터에서 검색한다.(기능 테스트용)
                if (string.IsNullOrEmpty(Network.User.Instance.CurrentUserData.AccessToken) && (_searchData == null || _searchData.Count == 0))
                {
                    this.SearchFromCachedData(text);
                }
#endif

                this.ChangeSearchState(_searchData.Count == 0 ? eSearchModeState.NO_RESULTS : eSearchModeState.OK);
            }

            this.RefreshContentsCount();

            this.OnDataChange?.Invoke();
        }

#if UNITY_EDITOR || ENV_DEV
        internal void SearchFromCachedData(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            bool[] searchResults = Enumerable.Repeat(false, _data.Count).ToArray();

            Parallel.For
            (
                0, _data.Count,
                i => searchResults[i] = _data[i].NickName.Contains(text)
            );

            _searchData = Enumerable
                    .Range(0, searchResults.Length)
                    .Where(i => searchResults[i])
                    .Select(i => _data[i])
                    .ToList();
        }
#endif

        private void RefreshContentsCount()
        {
            this.MaxContentsCount = _modeType switch
            {
                eModeType.SEARCH_MODE => _searchData.Count,
                _ => _data.Count
            };

            this.ContentsCount = Mathf.Min(INTERVAL_CONTENTS_COUNT, this.MaxContentsCount);
        }

        public void More(int lastContentsCount = -1)
        {
            if (lastContentsCount >= 0) this.ContentsCount = lastContentsCount;

            this.ContentsCount = Mathf.Min(this.ContentsCount + INTERVAL_CONTENTS_COUNT, this.MaxContentsCount);

            this.OnDataChange?.Invoke();
        }
    }
}
