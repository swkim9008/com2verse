/*===============================================================
* Product:		Com2Verse
* File Name:	AccountOption.cs
* Developer:	mikeyid77
* Date:			2023-05-12 15:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Protocols.CommonLogic;

namespace Com2Verse.Option
{
    [MetaverseOption("AccountOption")]
    public sealed class AccountOption : BaseMetaverseOption
    {
        [NonSerialized] private int _alarmCountIndex = 0;

        public int AlarmCountIndex
        {
            get => _alarmCountIndex;
            set
            {
                if (_alarmCountIndex != value)
                {
                    _alarmCountIndex = value;
                    OptionController.Instance.RequestStoreAlarmCountOption(value);
                }
            }
        }

        public override void SetTableOption()
        {
            _alarmCountIndex = Convert.ToInt32(TargetTableData[eSetting.ACCOUNT_SERVICENOTI].Default) - 1;
        }

        public override void SetStoredOption(SettingValueResponse response)
        {
            _alarmCountIndex = response.AlramCount;
        }
    }
}