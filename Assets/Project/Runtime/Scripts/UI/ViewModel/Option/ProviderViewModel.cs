/*===============================================================
* Product:		Com2Verse
* File Name:	ProviderViewModel.cs
* Developer:	mikeyid77
* Date:			2023-04-19 11:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Hive;
using Com2Verse.Logger;
using Com2Verse.Network;
using hive;
using eProviderType = Com2Verse.UI.MetaverseOptionViewModel.eProviderType;

namespace Com2Verse.UI
{
	public sealed class ProviderViewModel : ViewModelBase
	{
		private Dictionary<eProviderType, AuthV4.ProviderType> _idpDictionary = new()
		{
			[eProviderType.Google] = AuthV4.ProviderType.GOOGLE,
			[eProviderType.HIVE] = AuthV4.ProviderType.HIVE,
			[eProviderType.Facebook] = AuthV4.ProviderType.FACEBOOK,
			[eProviderType.Apple] = AuthV4.ProviderType.SIGNIN_APPLE,
		};

		private eProviderType _currentType;
		private string        _idpName     = string.Empty;
		private bool          _isConnected = false;
		private bool          _canInteract = false;
		private float         _canvasAlpha = 1f;

		public CommandHandler ProviderConnectButtonClicked { get; }

		public eProviderType CurrentType
		{
			get => _currentType;
			set
			{
				if (_currentType == value) return;

				_currentType = value;
				base.InvokePropertyValueChanged(nameof(CurrentType), CurrentType);
			}
		}

		public string IdpName
		{
			get => _idpName;
			set
			{
				_idpName = value;
				base.InvokePropertyValueChanged(nameof(IdpName), IdpName);
			}
		}

		public bool IsConnected
		{
			get => _isConnected;
			set
			{
				if (_isConnected == value) return;

				_isConnected = value;
				base.InvokePropertyValueChanged(nameof(IsConnected), IsConnected);
			}
		}

		public bool CanInteract
		{
			get => _canInteract;
			set
			{
				_canInteract = value;
				CanvasAlpha  = (value) ? 1f : 0.5f;
				base.InvokePropertyValueChanged(nameof(CanInteract), CanInteract);
			}
		}

		public float CanvasAlpha
		{
			get => _canvasAlpha;
			set
			{
				_canvasAlpha = value;
				base.InvokePropertyValueChanged(nameof(CanvasAlpha), CanvasAlpha);
			}
		}

		public ProviderViewModel() { }

		public ProviderViewModel(eProviderType type)
		{
			if (!_idpDictionary.ContainsKey(type)) return;

			CurrentType = type;
			IdpName = type.ToString();
			ProviderConnectButtonClicked = new CommandHandler(OnProviderConnectButtonClicked);

			HiveSDKHelper.SetIdp(_idpDictionary[CurrentType],
			                     (typeExists, typeConnected) =>
			                     {
				                     CanInteract = typeExists;
				                     IsConnected = typeConnected;
			                     });
		}

		private void OnProviderConnectButtonClicked()
		{
			UIManager.Instance.ShowWaitingResponsePopup(0f);
			if (IsConnected)
			{
				HiveSDKHelper.DisconnectIdp(_idpDictionary[CurrentType],
				                            (result) =>
				                            {
					                            UIManager.Instance.HideWaitingResponsePopup();
					                            IsConnected = (result) ? !IsConnected : IsConnected;
				                            });
			}
			else
			{
				HiveSDKHelper.ConnectIdp(_idpDictionary[CurrentType],
				                         (result) =>
				                         {
					                         UIManager.Instance.HideWaitingResponsePopup();
					                         IsConnected = (result) ? !IsConnected : IsConnected;
				                         });
			}
		}
	}
}
