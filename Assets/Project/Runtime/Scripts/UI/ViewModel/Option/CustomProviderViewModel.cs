/*===============================================================
* Product:		Com2Verse
* File Name:	CustomProviderViewModel.cs
* Developer:	mikeyid77
* Date:			2023-05-12 16:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Network;

namespace Com2Verse.UI
{
	public sealed class CustomProviderViewModel : ViewModelBase
	{
		private Sprite _icon;
		private string _name;
		private bool _idpToggle = true;
		private bool _alertToggle;
		public CommandHandler IdpConnectButtonClicked { get; }
		public CommandHandler AlertChangeButtonClicked { get; }

		public CustomProviderViewModel()
		{
			IdpConnectButtonClicked = new CommandHandler(OnIdpConnectButtonClicked);
			AlertChangeButtonClicked = new CommandHandler(OnAlertChangeButtonClicked);
		}

		public int ServiceId { get; set; }
		public string ServiceUserId { get; set; }
		
		public Sprite Icon
		{
			get => _icon;
			set => SetProperty(ref _icon, value);
		}
		
		public string Name
		{
			//set => Icon = SpriteAtlasManager.Instance.GetSprite("Atlas_MyPad", value);
			
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public bool IdpToggle
		{
			get => _idpToggle;
			set => SetProperty(ref _idpToggle, value);
		}

		public bool AlertToggle
		{
			get => _alertToggle;
			set => SetProperty(ref _alertToggle, value);
		}

		private void OnIdpConnectButtonClicked()
		{
			if (IdpToggle)
			{
				// 커스텀 IdP 연동 해제
				// LoginManager.Instance.DisconnectCustomIdp(ServiceId, ServiceUserId,
				// 	(result) => { IdpToggle = !result; });
			}
			else
			{
				// IdP 연동
				// TODO : 다른 IdP를 구분할 방법이 필요
				IdpToggle = false;
				//LoginManager.Instance.RequestServiceLogin(ServiceId, (result) => IdpToggle = result);
			}
			
		}

		private void OnAlertChangeButtonClicked()
		{
			AlertToggle = !AlertToggle;
			
			// TODO : 서비스 알림 설정 처리 필요
			// ...
		}
	}
}