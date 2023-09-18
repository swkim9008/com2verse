/*===============================================================
* Product:		Com2Verse
* File Name:	BaseMetaverseOption.cs
* Developer:	tlghks1009
* Date:			2022-10-04 17:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Utils;
using Protocols.CommonLogic;
using UnityEngine;

namespace Com2Verse.Option
{
	public class MetaverseOptionAttribute : Attribute
	{
		public string OptionName { get; } = string.Empty;

		public MetaverseOptionAttribute(string optionName)
		{
			OptionName = optionName;
		}
	}

	public abstract class BaseMetaverseOption
	{
		protected Dictionary<eSetting, Setting> TargetTableData => OptionController.Instance.SettingTableData;

		[NonSerialized] public bool NeedGlobalSave = false;

		public async void SaveData()
		{
			var toJson = JsonUtility.ToJson(this);

			if (NeedGlobalSave)
			{
				await LocalSave.TempGlobal.SaveStringAsync(GetType().Name, toJson);
			}
			else
			{
				await LocalSave.Temp.SaveStringAsync(GetType().Name, toJson);
			}
		}

		public virtual void OnInitialize() { }

		public virtual void Apply() { }

		public virtual void SetTableOption() { }

		public virtual void SetStoredOption(SettingValueResponse response) { }
	}
}
