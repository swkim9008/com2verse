/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotManager.cs
* Developer:	seaman2000
* Date:			2023-08-01 09:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using JetBrains.Annotations;
using System;
using System.Linq;
using Com2Verse.Extension;

namespace Com2Verse.Mice
{
    public sealed partial class RedDotManager : Singleton<RedDotManager>
    {
        [UsedImplicitly]
        private RedDotManager()
        {
            InitData();
            InitCheckerFunc();
            InitCounterFunc();
        }

        enum CheckState
        {
            NotYet,
            True,
            False
        }

        private Dictionary<CheckerKey, CheckState> _checkerStamp;
        private CheckerKey[] _checkKeys;

        private void ClearCheckerStamp()
        {
            if (_checkerStamp == null)
            {
                _checkerStamp = new Dictionary<CheckerKey, CheckState>();
                _checkKeys = (CheckerKey[])Enum.GetValues(typeof(CheckerKey));

                foreach (var checkKey in _checkKeys)
                    _checkerStamp.Add(checkKey, CheckState.NotYet);
            }
            else
            {
                foreach (var checkKey in _checkKeys)
                    _checkerStamp[checkKey] = CheckState.NotYet;
            }
        }

        public static void SetTrigger(RedDotData.TriggerKey triggerKey)
        {
            Instance._SetTrigger(triggerKey);
        }

        private void _SetTrigger(RedDotData.TriggerKey triggerKey)
        {
            ClearCheckerStamp();

            // 해당 트리거를 가진 오브젝트들을 가져온다.
            var findedObj = _redDotObjs.FindAll(a => a.RedDotData?.HasTrigger(triggerKey) ?? false);
            foreach (var obj in findedObj)
            {
                // 내 조건을 체크한다.
                obj.UpdateRedDot(CheckWithMarking(obj.RedDotData));
            }
        }

        public void StartWithMarking(IRedDotNotification redDotNoti, bool isInitStamp = false)
        {
            redDotNoti.UpdateRedDot(redDotNoti.RedDotData != null &&
                                    CheckWithMarking(redDotNoti.RedDotData, isInitStamp));
        }

        private bool CheckWithMarking(RedDotData redDotData, bool isInitStamp = false)
        {
            var validKeys = _redDotDatas.Values.Select(e => e.key).ToList();
            return CheckWithMarking(redDotData, ref validKeys, isInitStamp);
        }

        private bool CheckWithMarking(RedDotData redDotData, ref List<RedDotData.Key> validKeys,
            bool isInitStamp = false)
        {
            if (isInitStamp)
            {
                ClearCheckerStamp();
            }

            if (redDotData == null || redDotData.key == RedDotData.Key.None)
                return false;

            validKeys.Remove(redDotData.key);

            // 다른 레드닷의 조건을 체크한다.
            foreach (var key in redDotData.checkOtherKeys)
            {
                if (validKeys.Any(e => e == key) && CheckWithMarking(_redDotDatas[key], ref validKeys))
                    return true;
            }

            // 해당 오브젝트들의 조건을 체크한다.
            foreach (var check in redDotData.checkKeys)
            {
                if (_checkerStamp[check] == CheckState.NotYet)
                    _checkerStamp[check] = _checkerFuncMap[check](null) ? CheckState.True : CheckState.False;

                if (_checkerStamp[check] == CheckState.True)
                    return true;
            }

            return false;
        }

        public int CheckWithCounting(RedDotData redDotData)
        {
            if (redDotData == null || redDotData.key == RedDotData.Key.None)
                return 0;

            return _counterFuncMap[redDotData.key]();
        }
    }

    public sealed partial class RedDotManager
    {
        private readonly List<IRedDotNotification> _redDotObjs = new();

        public void Register(IRedDotNotification redDotObj)
        {
            _redDotObjs.TryAdd(redDotObj);
        }

        public void Unregister(IRedDotNotification redDot)
        {
            _redDotObjs.TryRemove(redDot);
        }
    }
}