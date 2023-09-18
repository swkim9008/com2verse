/*===============================================================
* Product:		Com2Verse
* File Name:	BaseUserData.cs
* Developer:	haminjeong
* Date:			2023-06-28 18:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Director;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Network
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ServiceIDAttribute : Attribute
	{
		public long ServiceID { get; }

		public ServiceIDAttribute(long serviceID)
		{
			ServiceID = serviceID;
		}
	}
	
	[ServiceID(100)]
	public class BaseUserData
	{
		protected static readonly int DefaultTimoutTime = 10000;
		
		public static event Action<string> OnAccessTokenChanged = _ => { };
		
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			_id                  = 0;
			_playerId            = 0;
			_domain              = string.Empty;
			_accessToken         = string.Empty;
			_refreshToken        = string.Empty;
			_objectID            = 0;
			OnAccessTokenChanged = null;
		}
#endif

#region Common Property
		// 공용으로 쓰일 변수는 static으로 설정
		[field: SerializeField, ReadOnly] private static long _id = 0;
		public long ID
		{
			get => _id;
			set
			{
				_id                           = value;
				MapController.Instance.UserID = value;
			}
		}
		[field: SerializeField, ReadOnly] private static ulong  _playerId = 0;
		public ulong PlayerId
		{
			get => _playerId;
			set
			{
				_playerId = value;
				LocalSave.SetId(Convert.ToString(_playerId));
			}
		}

		[field: SerializeField, ReadOnly] private static string _domain = string.Empty;
		public string Domain
		{
			get => _domain;
			set => _domain = value;
		}
		private static string _accessToken = string.Empty;
		public string AccessToken
		{
			get => _accessToken;
			set
			{
				_accessToken = value;
				OnAccessTokenChanged?.Invoke(value);
			}
		}
		[field: SerializeField, ReadOnly] private static string _refreshToken = string.Empty;
		public string RefreshToken
		{
			get => _refreshToken;
			set => _refreshToken = value;
		}
		[field: SerializeField, ReadOnly] private static long _objectID = 0;
		public long ObjectID
		{
			get => _objectID;
			set
			{
				_objectID                           = value;
				MapController.Instance.UserSerialID = value;
			}
		}
#endregion Common Property

#region Inherit Property
		// 상속관계에서 따로 값을 가져야 하는 경우 프로퍼티 유지
		// 서비스에서 하나 이상의 값을 가져야 하는 경우 override
		[field: SerializeField, ReadOnly] public virtual string UserName   { get; set; } = string.Empty;
		[field: SerializeField, ReadOnly] public virtual bool   LoginCheck { get; set; } = false;
#endregion Inherit Property
		
		public virtual async UniTask SendServiceLoginAsync()
		{
			var success = await SendLoginAsync();
			if (success)
			{
				await NetworkManager.Instance.SendIfEncryptActivated(() => User.Instance.DefaultTimeoutProcess());
				await NetworkManager.Instance.SendPairKeyToServer(() => User.Instance.DefaultTimeoutProcess());
				Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, (long)eServiceID.WORLD);
				User.Instance.ChangeUserData((long)eServiceID.WORLD);
				C2VDebug.Log("SEND COM2VERSE LOGIN REQUEST END");
				StartCom2VerseLoginProcess();
				C2VDebug.Log("SEND COM2VERSE LOGIN PROCESS");
			}
		}

		protected async UniTask<bool> SendLoginAsync()
		{
			C2VDebug.Log("SEND LOGIN REQUEST");
			UserDirector.Instance.NeedEnteringDirecting = true;

			var expiredTime = MetaverseWatch.Time + DefaultTimoutTime;
			while (!NetworkManager.Instance.IsConnected)
			{
				if (MetaverseWatch.Realtime > expiredTime)
				{
					User.Instance.DefaultTimeoutProcess();
					return false;
				}

				await UniTask.Yield();
				if (!NetworkManager.InstanceExists) return false;
			}

			return true;
		}

		private void StartCom2VerseLoginProcess()
		{
			User.Instance.SetConnected();
			Commander.Instance.Com2VerseLogin();
		}
	}
}
