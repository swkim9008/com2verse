/*===============================================================
* Product:		Com2Verse
* File Name:	RegisterGroupPopupViewModel.cs
* Developer:	mikeyid77
* Date:			2023-07-03 13:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.BuildHelper;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class RegisterGroupPopupViewModel : CommonPopupViewModel
	{
		private string         _inviteCode  = string.Empty;
		private bool           _canRegister = false;
		private Action         _forceClose  = null;
		public  CommandHandler CreateSpaceButtonClicked { get; }
		public  CommandHandler RegisterButtonClicked    { get; }

		public string InviteCode
		{
			get => _inviteCode;
			set
			{
				_inviteCode = value;
				CanRegister = !string.IsNullOrEmpty(value);
				InvokePropertyValueChanged(nameof(InviteCode), InviteCode);
			}
		}

		public bool CanRegister
		{
			get => _canRegister;
			set
			{
				_canRegister = value;
				InvokePropertyValueChanged(nameof(CanRegister), CanRegister);
			}
		}

		public RegisterGroupPopupViewModel()
		{
			CreateSpaceButtonClicked = new CommandHandler(OnCreateSpaceButtonClicked);
			RegisterButtonClicked    = new CommandHandler(OnRegisterButtonClicked);
		}

		public void SetInit(Action forceClose)
		{
			InviteCode  = string.Empty;
			_forceClose = forceClose;
		}

		private void OnCreateSpaceButtonClicked()
		{
			var urlTable = TableDataManager.Instance.Get<TableSpaXeUrl>();
			if (urlTable == null)
			{
				C2VDebug.LogErrorCategory("RegisterGroupViewModel", "TableData is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else if (urlTable.Datas == null)
			{
				C2VDebug.LogErrorCategory("RegisterGroupViewModel", "Data is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else
			{
				// TODO : 추후 수정 예정
				var url = string.Empty;
				switch (AppInfo.Instance.Data.Environment)
				{
					case eBuildEnv.QA:
						url = urlTable.Datas[eSpaxeUrlType.REGISTRATION]?.QaUrl;
						break;
					default:
						url = urlTable.Datas[eSpaxeUrlType.REGISTRATION]?.Url;
						break;
				}

				if (string.IsNullOrEmpty(url))
				{
					C2VDebug.LogErrorCategory("RegisterGroupViewModel", "URL is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
				}
				else
				{
					C2VDebug.LogCategory("RegisterGroupViewModel", "Open SPAXE Registration URL");
					Application.OpenURL(url);
				}
			}
		}

		private void OnRegisterButtonClicked()
		{
			LoginManager.Instance.RequestRegisterGroup(InviteCode, OnForceClose);
		}

		private void OnForceClose()
		{
			_forceClose?.Invoke();
		}
	}
}
