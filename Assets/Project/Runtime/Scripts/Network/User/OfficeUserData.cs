/*===============================================================
* Product:		Com2Verse
* File Name:	OfficeUserData.cs
* Developer:	haminjeong
* Date:			2023-06-28 18:47
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Organization;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Network
{
	[ServiceID(200)]
	public sealed class OfficeUserData : BaseUserData
	{
		[field: SerializeField, ReadOnly] public  string EmployeeID { get; set; } = string.Empty;
		[field: SerializeField, ReadOnly] private string _userName  = string.Empty;
		private bool IsAuthorizedOffice => CurrentScene.ServiceType is eServiceType.OFFICE && CurrentScene.SpaceCode is not (eSpaceCode.LOBBY or eSpaceCode.MODEL_HOUSE) && IsSceneLoaded;
		private bool IsSceneLoaded => SceneManager.InstanceOrNull?.CurrentScene.SceneState is eSceneState.LOADED;
		public override string UserName
		{
			get
			{
				if (IsAuthorizedOffice && ViewModelManager.Instance.GetOrAdd<CommunicationAvatarManagerViewModel>().TryGet(ID, out var organizationViewModel))
				{
					var organizationName = organizationViewModel.UserName;
					if (!string.IsNullOrWhiteSpace(organizationName!))
						return organizationName;
				}

				return _userName;
			}
			set => _userName = value;
		}
		[field: SerializeField, ReadOnly] public string GuestName { get; set; } = string.Empty;

		public override async UniTask SendServiceLoginAsync()
		{
			var success = await SendLoginAsync();
			if (success)
			{
				C2VDebug.Log("SEND SERVICE LOGIN REQUEST END");
				Commander.Instance.ServiceLogin();
				C2VDebug.Log("SEND SERVICE LOGIN PROCESS");
			}
		}
	}
}
