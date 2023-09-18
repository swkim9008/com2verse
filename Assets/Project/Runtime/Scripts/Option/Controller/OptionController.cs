/*===============================================================
* Product:		Com2Verse
* File Name:	OptionController.cs
* Developer:	tlghks1009
* Date:			2022-10-04 15:42
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Com2Verse.Utils;
using JetBrains.Annotations;
using Protocols.CommonLogic;
using UnityEngine;

namespace Com2Verse.Option
{
	public class OptionController : Singleton<OptionController>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private OptionController() { }

		private Action<OptionController> _optionDataLoadOnSplashFinished;
		private Action<OptionController> _optionDataLoadOnLoginFinished;

		private readonly List<BaseMetaverseOption> _optionList = new();

		public Dictionary<eSetting, Setting> SettingTableData { get; private set; }

		public void InitializeAfterSplash()
		{
			LoadTable();
			RegisterOptionAfterSplash();
		}

		public void InitializeAfterLogin()
		{
			RegisterOptionAfterLogin();
			SetAccountSettingResponse();
		}

		public void Dispose()
		{
			foreach (var option in _optionList)
			{
				(option as IDisposable)?.Dispose();
			}

			_optionList.Clear();
		}

		public T GetOption<T>() where T : BaseMetaverseOption
		{
			foreach (var option in _optionList)
			{
				if (typeof(T) == option.GetType())
					return option as T;
			}

			return null;
		}

		public void ApplyAll()
		{
			foreach (var option in _optionList)
			{
				option.Apply();
			}
		}


		public void SaveAll()
		{
			foreach (var option in _optionList)
			{
				option.SaveData();
			}
		}

		private void RegisterOptionAfterSplash()
		{
			_optionList.Add(LoadOption<ResolutionOption>(true));
			_optionList.Add(LoadOption<LanguageOption>(true));
			_optionList.Add(LoadOption<VolumeOption>(true));
			_optionList.Add(LoadOption<ChatOption>(true));

			foreach (var option in _optionList)
			{
				option?.OnInitialize();
			}

			_optionDataLoadOnSplashFinished?.Invoke(this);
		}

		private void RegisterOptionAfterLogin()
		{
			_optionList.Add(LoadOption<GraphicsOption>(false));
			_optionList.Add(LoadOption<ControlOption>(false));
			_optionList.Add(LoadOption<AccountOption>(false));
			_optionList.Add(LoadOption<DeviceOption>(false));

			foreach (var option in _optionList)
			{
				option?.OnInitialize();
			}
			
			_optionDataLoadOnLoginFinished?.Invoke(this);
		}

		private T LoadOption<T>(bool needGlobalSave) where T : BaseMetaverseOption
		{
			var key = typeof(T).Name;
			var jsonData = string.Empty;
			if (needGlobalSave)
			{
				jsonData = LocalSave.TempGlobal.LoadString(key);
			}
			else
			{
				jsonData = LocalSave.Temp.LoadString(key);
			}

			C2VDebug.LogCategory("OptionController", $"{key} Target : {jsonData}");

			if (string.IsNullOrEmpty(jsonData))
			{
				var newOption = Activator.CreateInstance(typeof(T)) as T;
				newOption.NeedGlobalSave = needGlobalSave;
				newOption.SetTableOption();
				C2VDebug.LogCategory("OptionController", $"New Option : {key} (NeedGlobalSave : {needGlobalSave})");
				return newOption;
			}

			var targetOption = JsonUtility.FromJson<T>(jsonData);
			targetOption.NeedGlobalSave = needGlobalSave;
			targetOption.SetTableOption();
			C2VDebug.LogCategory("OptionController", $"Load Option : {key} (NeedGlobalSave : {needGlobalSave})");
			return targetOption;
		}

#if ENABLE_CHEATING
		public void RemoveOptionData()
		{
			foreach (var option in _optionList)
			{
				var key = option.GetType().Name;
				C2VDebug.LogCategory("OptionController", $"Remove Option : {key}");
				if (option.NeedGlobalSave)
				{
					LocalSave.TempGlobal.Delete(key);
				}
				else
				{
					LocalSave.Temp.Delete(key);
				}
			}
		}
#endif

		/// <summary>
		/// 설정 값 초기화를 위한 데이터를 저장하기 위한 테이블 데이터를 로드합니다
		/// </summary>
		private void LoadTable()
		{
			TableSetting tableSetting = TableDataManager.Instance.Get<TableSetting>();

			if (tableSetting?.Datas == null)
				return;

			SettingTableData = tableSetting!.Datas;
		}

#region StoredOption
		public void SetTargetStoredOption()
		{
			PacketReceiver.Instance.SettingValueResponse += ResponseSettingValue;
		}

		private void ResponseSettingValue(SettingValueResponse response)
		{
			PacketReceiver.Instance.SettingValueResponse -= ResponseSettingValue;

			C2VDebug.LogCategory("OptionController", $"{nameof(ResponseSettingValue)} : {response}");
			foreach(var option in _optionList)
				option.SetStoredOption(response);
		}
		
		private void SetAccountSettingResponse()
		{
			PacketReceiver.Instance.AccountSettingResponse += ResponseAccountSetting;
		}

		private void ResponseAccountSetting(AccountSettingResponse response)
		{
			Commander.Instance.RequestNotificationGetList();
		}

		public void RequestStoreAlarmCountOption(int index)
		{
			Commander.Instance.AccountSettingRequest(GetOption<LanguageOption>().LanguageIndex, index);
		}

		public void RequestStoreLanguageOption(int index)
		{
			Commander.Instance.AccountSettingRequest(index, GetOption<AccountOption>().AlarmCountIndex);
		}

		public void RequestInitLanguageOption(int language, int alarm)
		{
			Commander.Instance.AccountSettingRequest(language, alarm);
		}
#endregion

#region OptionEvent
		public event Action<OptionController> OptionDataLoadOnSplashFinished
		{
			add
			{
				_optionDataLoadOnSplashFinished -= value;
				_optionDataLoadOnSplashFinished += value;
			}
			remove => _optionDataLoadOnSplashFinished -= value;
		}

		public event Action<OptionController> OptionDataLoadOnLoginFinished
		{
			add
			{
				_optionDataLoadOnLoginFinished -= value;
				_optionDataLoadOnLoginFinished += value;
			}
			remove => _optionDataLoadOnLoginFinished -= value;
		}
#endregion OptionEvent
	}
}
