/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationInfo.cs
* Developer:	jhkim
* Date:			2022-07-14 18:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Com2Verse.WebApi.Service;
using Newtonsoft.Json;
using EmployeeNoType = System.String;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.Organization
{
	public struct Constant
	{
		public static readonly Vector3 DefaultPopupPosition = new(Screen.width * .5f, Screen.height * .5f, 0f);
		public static readonly float HierarchyFoldAngle = 180f;
		public static readonly float HierarchyUnFoldAngle = 90f;

		internal static readonly int RequestTimeout = 60000; // 단위 (ms)
		public struct TextKey
		{
			public static readonly string AllMember = "UI_OrganizationChart_Desc_AllMember";
			public static readonly string DuplicateJob = "UI_OrganizationChart_Desc_DuplicateJob";
			public static readonly string GroupInviteButtonText = "UI_OrganizationChart_Btn_GroupInvite";
			public static readonly string MeetingInviteButtonText = "UI_MeetingAppReserve_Popup_OrganizationChart_Btn_AddParticipant";
			public static readonly string MakeGroup = "UI_TC_MakeGroup";
		}
	}

	public partial class DataManager : Singleton<DataManager>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private DataManager()
		{
			_onResponseQueue = new();
		}

#region Variables
		private TrieData _trieData;
		private HierarchyData _hierarchyData;
		private Queue<Action<bool>> _onResponseQueue;
		private static bool _isRequested = false;
		private static Dictionary<eOrganizationRefreshType, int> _coolTimeMap = new();
		private static Dictionary<eOrganizationRefreshType, DateTime?> _lastRequestMap = new();
#endregion // Variables

#region Properties
		/// <summary>
		/// AccountID
		/// 최초에 외부에서 값 세팅 필요
		/// </summary>
		public long UserID { get; set; } = 0;

		public long GroupID { get; set; } = -1;
#endregion // Properties

#region Initialize
		public static void Initialize()
		{
			InitTable();
		}

		private static void InitTable()
		{
			_coolTimeMap.Clear();

			var table = TableDataManager.Instance.Get<TableOrganizationRefresh>();
			if (table != null)
			{
				foreach (var key in table.Datas.Keys)
					_coolTimeMap.Add(key, table.Datas[key].CoolTime);
			}
		}
#endregion // Initialize

#region Network
		public static async UniTask<bool> TryOrganizationRefreshAsync(eOrganizationRefreshType type, long groupId)
		{
			if (IsCoolTimeOver(type))
			{
				var result = await SendOrganizationChartRequestAsync(groupId);
				UpdateCoolTime(type);
				return result;
			}
			return false;
		}
		public static void SendOrganizationChartRequest(long groupId, Action<bool> onResponse = null)
		{
			Instance._onResponseQueue.Enqueue(onResponse);
			if (_isRequested) return;

			_isRequested = true;
			RequestOrganizationChartAsync(groupId).Forget();
		}

		public static async UniTask<bool> SendOrganizationChartRequestAsync(long groupId)
		{
			var isPreRequested = false;
			while (_isRequested)
			{
				isPreRequested = true;
				await UniTask.Yield();
			}

			if (isPreRequested) return true;

			var result = false;
			var isResponseReceived = false;
			Instance._onResponseQueue.Enqueue(success =>
			{
				isResponseReceived = true;
				result = success;
			});

			_isRequested = true;
			await RequestOrganizationChartAsync(groupId);

			await UniTask.WaitUntil(() => isResponseReceived);
			return result;
		}
		private static async UniTask RequestOrganizationChartAsync(long groupId)
		{
			try
			{
				var cts = new CancellationTokenSource(Constant.RequestTimeout);
				var request = new Components.OrganizationChartRequest
				{
					GroupId = groupId,
				};
				var jsonResponse = await WebApi.Service.Api.Organization.PostOrganizationChart(request, cts);
				if (jsonResponse is not {StatusCode: HttpStatusCode.OK} || cts.IsCancellationRequested)
				{
					if (cts.IsCancellationRequested)
						C2VDebug.Log($"조직도 요청 타임아웃 초과 ! ({Constant.RequestTimeout * 0.001:00} 초)");
					InvokeAllResponse(false);
					_isRequested = false;
					return;
				}

				var responseData = jsonResponse.Value;
				Instance.Parse(responseData);
				_isRequested = false;
				InvokeAllResponse(true);
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning($"Request Organization Chart Failed\n{e}");
				InvokeAllResponse(false);
				_isRequested = false;
			}

			void InvokeAllResponse(bool success)
			{
				while (Instance._onResponseQueue.Count > 0)
				{
					var onResponse = Instance._onResponseQueue.Dequeue();
					onResponse?.Invoke(success);
				}
			}
		}

		public void SetData(Components.OrganizationChartResponse response)
		{
			Dispose();

			if (response == null) return;

			var group = JsonConvert.DeserializeObject<Components.Group>(response.Group);
			GroupID = group.GroupId;
			_trieData = TrieData.ParseNew(group);
			_hierarchyData = HierarchyData.ParseNew(group);
		}
		private void Parse(Components.OrganizationChartResponseResponseFormat response)
		{
			if (response?.Data == null) return;

			var group = JsonConvert.DeserializeObject<Components.Group>(response.Data.Group);
			GroupID = group.GroupId;
			_trieData = TrieData.ParseNew(group);
			_hierarchyData = HierarchyData.ParseNew(group);
		}
#endregion // Network

#region Public - Common
		public bool IsReady => _trieData != null && _hierarchyData != null;
		public async UniTask<MemberModel> GetMyselfAsync() => await GetMemberAsync(UserID);
		public MemberModel GetMyself() => GetMember(UserID);
#endregion // Public - Common

#region CoolTime
		private static bool IsCoolTimeOver(eOrganizationRefreshType type)
		{
			if (_lastRequestMap.ContainsKey(type) && _lastRequestMap[type].HasValue)
				return _lastRequestMap[type].Value <= DateTime.Now;

			return true;
		}

		private static void UpdateCoolTime(eOrganizationRefreshType type)
		{
			var availableTime = DateTime.Now.AddMilliseconds(_coolTimeMap[type]);
			_lastRequestMap[type] = availableTime;
		}
#endregion // CoolTime

#region Dispose
		public static void DisposeOrganization()
		{
			if (!InstanceExists) return;

			Instance.Dispose();
		}
		public void Dispose()
		{
			ClearData();
		}

		static void ClearData()
		{
			_isRequested = false;
			if (!InstanceExists) return;

			if (Instance.IsReady)
			{
				Instance._trieData?.Dispose();
				Instance._hierarchyData?.Dispose();
				Instance._onResponseQueue?.Clear();
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Clear()
		{
			_coolTimeMap.Clear();
			_lastRequestMap.Clear();
		}
#endregion // Dispose
	}
}
