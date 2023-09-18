/*===============================================================
* Product:		Com2Verse
* File Name:	UserStateInfo.cs
* Developer:	ksw
* Date:			2022-11-08 15:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Network;
using Google.Protobuf;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Protocols.GameLogic;

namespace Com2Verse.UserState
{
	public sealed class Info : Singleton<Info>, IDisposable
	{
		public enum eUserState
		{
			OFF_LINE,
			ON_LINE,
			BUSY,
		}

#region Variables
		public static readonly eUserState DefaultState = eUserState.OFF_LINE;
		private Dictionary<long, eUserState> _userStateMap;
		private event Action<long, eUserState> UserStateUpdated;
#endregion // Variables

#region Properties
		public event Action<long, eUserState> OnUserStateUpdated
		{
			add
			{
				UserStateUpdated -= value;
				UserStateUpdated += value;
			}
			remove => UserStateUpdated -= value;
		}
#endregion // Properties

#region Initialize
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private Info()
		{
			_userStateMap = new();
		}
#endregion // Initialize

#region Network
		public void OnLogOnOffUserInfoResponse(IMessage message)
		{
			var response = message as LogOnOffUserInfoResponse;

			if (response != null)
				UpdateUserStates(response.LogOnOffUserInfos);
		}

		// TODO : 코드 중복 리펙토링 확인 필요
		public void SendLogOnOffUserInfoRequest(IEnumerable<long> accountIds)
		{
			if (!accountIds.Any()) return;
			var request = new LogOnOffUserInfoRequest {Users = {accountIds}};
			SendRequest(request);
		}
		public void SendLogOnOffUserInfoRequest(IList<long> accountIds)
		{
			if (accountIds.Count == 0) return;

			var request = new LogOnOffUserInfoRequest {Users = {accountIds}};
			SendRequest(request);
		}

		public void SendLogOnOffUserInfoRequest(params long[] accoundIds)
		{
			if (accoundIds.Length == 0) return;

			var request = new LogOnOffUserInfoRequest {Users = {accoundIds}};
			SendRequest(request);
		}

		// public void SendLogOnOffUserInfoRequest(IList<EmployeeNoType> employeeNos)
		// {
		// 	if (employeeNos.Count == 0) return;
		//
		// 	var accountIds = employeeNos.Select(employeeNo => Organization.DataManager.Instance.GetEmployeeByEmployeeNo(employeeNo).AccountID);
		// 	var request = new LogOnOffUserInfoRequest {Users = {accountIds}};
		// 	SendRequest(request);
		// }
		// public void SendLogOnOffUserInfoRequest(params EmployeeNoType[] employeeNos)
		// {
		// 	if (employeeNos.Length == 0) return;
		//
		// 	var accountIds = employeeNos.Select(employeeNo => Organization.DataManager.Instance.GetEmployeeByEmployeeNo(employeeNo).AccountID);
		// 	var request = new LogOnOffUserInfoRequest {Users = {accountIds}};
		// 	SendRequest(request);
		// }

		private void SendRequest(LogOnOffUserInfoRequest request)
		{
			RemoveInvalidAccountId();
			NetworkManager.Instance.Send(request, MessageTypes.LogOnOffUserInfoRequest);
			void RemoveInvalidAccountId()
			{
				while (request.Users.Remove(0)) ;
			}
		}
#endregion // Network

#region User State
		// public eUserState GetUserState(EmployeeNoType employeeNo)
		// {
		// 	var employee = Organization.DataManager.Instance.GetMember(employeeNo);
		// 	return employee == null ? DefaultState : GetUserState(employee.AccountID);
		// }

		public eUserState GetUserState(long accountId) => HasUserState(accountId) ? _userStateMap[accountId] : DefaultState;
		public bool HasUserState(long accountId) => _userStateMap.ContainsKey(accountId);
		private void UpdateUserStates(RepeatedField<LogOnOffUserInfo> results)
		{
			foreach (var info in results)
			{
				var state = GetLogonState(info.IsLogOn);
				if (_userStateMap.ContainsKey(info.AccountId))
					_userStateMap[info.AccountId] = state;
				else
					_userStateMap.Add(info.AccountId, state);
				UserStateUpdated?.Invoke(info.AccountId, state);
			}
		}
		private eUserState GetLogonState(bool isLogon) => isLogon ? eUserState.ON_LINE : eUserState.OFF_LINE;
#endregion // User State

// #region Log
// 		private void Log(LogOnOffUserInfoRequest request)
// 		{
// 			StringBuilder sb = new StringBuilder($"[USER STATE] SEND REQUEST ({request.Users.Count})\n");
// 			foreach (var accountId in request.Users)
// 				sb.AppendLine(accountId.ToString());
// 			C2VDebug.Log(sb.ToString());
// 		}
//
// 		private void Log(RepeatedField<LogOnOffUserInfo> results)
// 		{
// 			StringBuilder sb = new StringBuilder($"[USER STATE] ON RESPONSE ({results.Count})\n");
// 			foreach (var info in results)
// 				sb.AppendLine($"{info.AccountId} : LogOn = {info.IsLogOn}");
// 			C2VDebug.Log(sb.ToString());
// 		}
// #endregion // Log

#region Dispose
		public void Dispose()
		{
			_userStateMap?.Clear();
			_userStateMap = null;
		}
#endregion // Dispose
	}
}
