/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotManager_Data.cs
* Developer:	seaman2000
* Date:			2023-08-01 09:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Com2Verse.Mice
{
    public sealed partial class RedDotManager // Data
    {
        [Serializable]
        public class RedDotData
        {
            public enum Key
            {
                None,
                MiceApp_Notice,
                MiceApp_Ticket,
                MiceApp,
                MyPad,
            }

            public enum TriggerKey
            {
                None,
                ShowNotice,
                ShowMyPackage,
                ShowMyPad,
                EnterMice,
            }

            // UI에 표시해야할 키
            public Key key;

            // 이벤트 발동 키
            public TriggerKey[] triggerKeys;

            // 체크해야할 조건 키 (하나라도 true 면 활성화)
            public CheckerKey[] checkKeys;

            // 다른 데이타의 조건이 체크되어야 하는 경우 (하나라도 true 면 활성화)
            public Key[] checkOtherKeys;

            public bool HasTrigger(TriggerKey trigger)
            {
                return triggerKeys.Contains(trigger);
            }
        }

        private Dictionary<RedDotData.Key, RedDotData> _redDotDatas;

        public RedDotData GetDataRow(RedDotData.Key key)
        {
            return _redDotDatas?[key] ?? default;
        }

        private void InitData()
        {
            if (_redDotDatas != null) return;
            _redDotDatas = new Dictionary<RedDotData.Key, RedDotData>();

            // TODO : 추후 테이블로 정리해야 함
            const string jsonData = @"
            [
                {
                    'key': 'MiceApp_Notice',
                    'triggerKeys': [ 'ShowNotice' ],
                    'checkKeys': [ 'HasNewNotice' ],
                    'checkOtherKeys' : [ 'None' ]
                },
                {
                    'key': 'MiceApp_Ticket',
                    'triggerKeys': [ 'ShowMyPackage' ],
                    'checkKeys': [ 'HasNewMyPackage' ],
                    'checkOtherKeys' : [ 'None' ]
                },
                {
                    'key': 'MiceApp',
                    'triggerKeys': [ 'ShowMyPad' ],
                    'checkKeys': [ 'None' ],
                    'checkOtherKeys' : [ 'MiceApp_Notice', 'MiceApp_Ticket' ]
                },
                {
                    'key': 'MyPad',
                    'triggerKeys': [ 'EnterMice' ],
                    'checkKeys': [ 'None' ],
                    'checkOtherKeys' : [ 'MiceApp' ]
                },
            ]";

            var datas = JsonConvert.DeserializeObject<RedDotData[]>(jsonData);
            foreach (var entry in datas)
                _redDotDatas.Add(entry.key, entry);
        }
    }
}