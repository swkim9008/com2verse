/*===============================================================
 * Product:		Com2Verse
 * File Name:	SmallTalkDistance.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-09 18:49
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Com2Verse.Utils;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.Communication;
using User = Com2Verse.Network.User;

namespace Com2Verse.SmallTalk
{
	public sealed class SmallTalkDistance : Singleton<SmallTalkDistance>, IDisposable, IRemoteTrackPublishRequester
	{
		public event Action<bool>? StateChanged;

		public event Action<ChannelObjectInfo, eTrackType, bool>? TrackPublishRequestChanged;

		public string GetDebugInfo() => nameof(SmallTalkDistance);

		public bool IsEnabled { get; private set; }

		public IChannel? SelfChannel { get; private set; }

		public IReadOnlyDictionary<long, ChannelObjectInfo> OtherChannelObjects => _otherChannelObjects;

		private readonly Dictionary<long, ChannelObjectInfo> _otherChannelObjects = new();

		private readonly Dictionary<long, OtherMediaChannelNotify> _otherChannelsConnectionInfo = new();

		public IReadOnlyDictionary<long, ChannelObjectInfo> VoiceRequestingUsers  => _voiceRequestingUsers;
		public IReadOnlyDictionary<long, ChannelObjectInfo> CameraRequestingUsers => _cameraRequestingUsers;

		private readonly Dictionary<long, ChannelObjectInfo> _voiceRequestingUsers  = new();
		private readonly Dictionary<long, ChannelObjectInfo> _cameraRequestingUsers = new();

		private readonly List<ActiveObject> _targetObjectsCache   = new();
		private readonly List<ActiveObject> _removingObjectsCache = new();

		private TableOfficeCommunication? _table;
		private OfficeCommunication?      _smallTalkData;

		private CancellationTokenSource? _distanceCheckToken;

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private SmallTalkDistance() { }

		public void TryInitialize()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				_table = TableDataManager.Instance.Get<TableOfficeCommunication>();
			}
			IsInitializing = false;
			IsInitialized  = true;
		}

		/// <summary>
		/// 스몰토크 기능을 사용할 수 있도록 활성화한다.
		/// </summary>
		public void Enable(int tableIndex)
		{
			if (!IsInitialized)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "SmallTalkData is not loaded yet.");
				return;
			}

			if (IsEnabled)
				return;

			IsEnabled = true;

			_smallTalkData = _table?.Datas?[tableIndex];

			MapController.Instance.OnMapObjectRemove         += OnMapObjectRemove;
			PacketReceiver.Instance.OnUsePortalResponseEvent += OnUsePortalResponse;

			CommunicationManager.Instance.ChangeCommunicationType(eCommunicationType.SMALL_TALK);

			_distanceCheckToken ??= new CancellationTokenSource();
			CheckDistanceAsync(_distanceCheckToken).Forget();

			StateChanged?.Invoke(true);
		}


		/// <summary>
		/// 스몰토크 기능을 사용할 수 없도록 비활성화하고 관련된 리소스를 해제한다.
		/// </summary>
		public void Disable()
		{
			if (!IsEnabled)
				return;

			IsEnabled = false;

			_distanceCheckToken?.Cancel();
			_distanceCheckToken?.Dispose();
			_distanceCheckToken = null;

			LeaveSelfChannel();
			LeaveAllOtherChannels();

			var mapController = MapController.InstanceOrNull;
			if (mapController != null)
				mapController.OnMapObjectRemove -= OnMapObjectRemove;

			var packetReceiver = PacketReceiver.InstanceOrNull;
			if (packetReceiver != null)
				packetReceiver.OnUsePortalResponseEvent -= OnUsePortalResponse;

			BlobManager.InstanceOrNull?.Clear();
			_smallTalkData = null;

			CommunicationManager.InstanceOrNull?.ChangeCommunicationType(eCommunicationType.DEFAULT);

			StateChanged?.Invoke(false);
		}

		/// <summary>
		/// 서버로부터 받은 SelfChannelNotify를 처리한다.
		/// </summary>
		public void OnSelfMediaChannelNotify(SelfMediaChannelNotify? notify)
		{
			if (notify == null)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "JoinChannelResponse is null");
				return;
			}

			if (SelfChannel != null)
			{
				C2VDebug.LogWarningMethod(nameof(SmallTalkDistance), "SelfChannel is already exist");
				return;
			}

			// SelfChannel 은 바로 접속한다.
			JoinSelfChannel(notify);
		}

		/// <summary>
		/// 서버로부터 받은 OtherChannelNotify를 처리한다.
		/// </summary>
		public void OnOtherMediaChannelNotify(OtherMediaChannelNotify? notify)
		{
			if (notify == null)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "JoinChannelResponse is null");
				return;
			}

			var ownerId = notify.OwnerId;
			if (_otherChannelsConnectionInfo.TryGetValue(ownerId, out var otherChannelInfo))
			{
				if (otherChannelInfo?.ChannelId == notify.ChannelId)
				{
					C2VDebug.LogWarningMethod(nameof(SmallTalkDistance), "OtherChannelInfo is already exist");
				}
				else
				{
					C2VDebug.LogMethod(nameof(SmallTalkDistance), "OtherChannelInfo updated.");
					LeaveOtherChannel(ownerId);
					_otherChannelsConnectionInfo[ownerId] = notify;
				}

				return;
			}

			// OtherChannel 은 거리 기반 호출을 위해 저장만 한다.
			_otherChannelsConnectionInfo.Add(ownerId, notify);
		}

		private void OnMapObjectRemove(BaseMapObject value)
		{
			foreach (var otherChannelObject in _otherChannelObjects.Values)
			{
				var activeObject = otherChannelObject.ActiveObject;
				if (value != activeObject)
					continue;

				LeaveOtherChannel(activeObject.OwnerID);
				_otherChannelsConnectionInfo.Remove(activeObject.OwnerID);
				return;
			}
		}

		/// <summary>
		/// 포탈 사용시 스몰토크 기능을 비활성화한다.
		/// </summary>
		private void OnUsePortalResponse(Protocols.GameLogic.UsePortalResponse response)
		{
			// TODO: 포탈을 이용해 Scene이 바뀌지 않을 경우 다시 활성화되어야 한다.
			Disable();
		}

		/// <summary>
		/// Enable 진입 시 일정 간격 주기로 거리를 체크한다.
		/// <br/>연산 부하를 방지하기 위해 <see cref="BaseMapObject.Updated"/> 대신 자체적으로 루프를 돌며 업데이트.
		/// </summary>
		private async UniTaskVoid CheckDistanceAsync(CancellationTokenSource token)
		{
			var selfChannelNotify = await SmallTalkHelper.GetSelfChannel();
			OnSelfMediaChannelNotify(selfChannelNotify);

			while (await UniTaskHelper.Delay(Define.DistanceCheckInterval, token))
			{
				var targetQuery   = GetActiveObjectsInChannelArea();
				var targetObjects = UpdateOtherChannelUsersId(targetQuery);
				LeaveNonTargetChannels(targetObjects);

				UpdatePublishRequestStatus(targetObjects);

				if (_smallTalkData?.UseBlobEffects ?? false)
					UpdateBlobNodes();
			}
		}

		/// <summary>
		/// 맵 안의 자신이 아닌 ActiveObject중 나와의 거리를 기준으로 정렬하여 가장 가까운 오브젝트를 반환하는 반복자.
		/// </summary>
		private IEnumerable<ActiveObject> GetActiveObjectsInChannelArea() =>
			from mapObject in MapController.Instance.ActiveObjects
			let activeObject = mapObject.Value as ActiveObject
			where activeObject != null && !activeObject.IsMine
			where activeObject.ObjectTypeId == 1 // 1: 아바타
			let distance = activeObject.DistanceFromMe
			where _smallTalkData != null && (_smallTalkData.ChannelConnectDistance < 0 || distance <= _smallTalkData.ChannelConnectDistance)
			orderby distance
			select activeObject;

		/// <summary>
		/// <paramref name="targetObjects"/>를 순회하며 접속 정보가 저장되어 있는 경우 OtherChannel에 참여한다.
		/// </summary>
		private List<ActiveObject> UpdateOtherChannelUsersId(IEnumerable<ActiveObject> targetObjects)
		{
			_targetObjectsCache.Clear();
			var channelCount = 0;

			foreach (var activeObject in targetObjects)
			{
				var ownerId = activeObject.OwnerID;

				if (!_otherChannelsConnectionInfo.TryGetValue(ownerId, out var notify))
					continue;

				if (!_otherChannelObjects.ContainsKey(ownerId))
					JoinOtherChannel(notify!, activeObject);

				_targetObjectsCache.Add(activeObject);
				channelCount++;

				if (_smallTalkData == null || _smallTalkData.ChannelPeerLimit < 0)
					continue;

				if (channelCount >= _smallTalkData?.ChannelPeerLimit)
					break;
			}

			return _targetObjectsCache;
		}

		/// <summary>
		/// 기존 채널에 접속중이던 ActiveObject들 중 <paramref name="targetObjects"/>에 포함되지 않은 ActiveObject들을 채널에서 나간다.
		/// </summary>
		private void LeaveNonTargetChannels(IReadOnlyCollection<ActiveObject> targetObjects)
		{
			// CollectionModifiedException 방지를 위해 캐시 사용
			_removingObjectsCache.Clear();

			foreach (var otherChannelObject in _otherChannelObjects.Values)
			{
				var activeObject = otherChannelObject.ActiveObject;

				if (targetObjects.Contains(activeObject))
					continue;

				_removingObjectsCache.Add(activeObject);
			}

			foreach (var removingOwnerId in _removingObjectsCache)
			{
				LeaveOtherChannel(removingOwnerId.OwnerID);
			}
		}

		/// <summary>
		/// Channel의 Host유저에게 거리를 기반으로 트랙 접속 여부를 요청한다.
		/// </summary>
		private void UpdatePublishRequestStatus(IEnumerable<ActiveObject> targetObjects)
		{
			var voiceTracksCount  = 0;
			var cameraTracksCount = 0;

			foreach (var targetObject in targetObjects)
			{
				var uid = targetObject.OwnerID;
				if (!_otherChannelObjects.TryGetValue(uid, out var otherChannelObject))
					return;

				var distance = targetObject.DistanceFromMe;
				UpdateVoicePublishRequestStatus(otherChannelObject, distance, ref voiceTracksCount);
				UpdateCameraPublishRequestStatus(otherChannelObject, distance, ref cameraTracksCount);
			}

			void UpdateVoicePublishRequestStatus(ChannelObjectInfo target, float distance, ref int tracksCount)
			{
				var canAddVoiceTrack      = _smallTalkData != null && (_smallTalkData.VoiceTrackLimit      < 0 || tracksCount < _smallTalkData.VoiceTrackLimit);
				var inVoiceArea           = _smallTalkData != null && (_smallTalkData.VoiceConnectDistance < 0 || distance    <= _smallTalkData.VoiceConnectDistance);
				var willAddPublishRequest = canAddVoiceTrack       && inVoiceArea;

				bool requestStateChanged;
				if (willAddPublishRequest)
				{
					requestStateChanged = _voiceRequestingUsers.TryAdd(target.OwnerId, target);
					tracksCount++;
				}
				else
				{
					requestStateChanged = _voiceRequestingUsers.Remove(target.OwnerId);
				}

				if (requestStateChanged)
					TrackPublishRequestChanged?.Invoke(target, eTrackType.VOICE, willAddPublishRequest);
			}

			void UpdateCameraPublishRequestStatus(ChannelObjectInfo target, float distance, ref int tracksCount)
			{
				var canAddCameraTrack     = _smallTalkData != null && (_smallTalkData.CameraTrackLimit      < 0 || tracksCount < _smallTalkData.CameraTrackLimit);
				var inCameraArea          = _smallTalkData != null && (_smallTalkData.CameraConnectDistance < 0 || distance    <= _smallTalkData.CameraConnectDistance);
				var willAddPublishRequest = canAddCameraTrack      && inCameraArea;

				bool requestStateChanged;

				if (willAddPublishRequest)
				{
					requestStateChanged = _cameraRequestingUsers.TryAdd(target.OwnerId, target);
					tracksCount++;
				}
				else
				{
					requestStateChanged = _cameraRequestingUsers.Remove(target.OwnerId);
				}

				if (requestStateChanged)
					TrackPublishRequestChanged?.Invoke(target, eTrackType.CAMERA, willAddPublishRequest);
			}
		}

		/// <summary>
		/// Blob 상태를 업데이트한다.
		/// </summary>
		private void UpdateBlobNodes()
		{
			var blobCount = 0;

			foreach (var otherChannelObject in _otherChannelObjects.Values)
			{
				if (GetOtherBlobState(otherChannelObject))
				{
					BlobManager.Instance.Connect(otherChannelObject.OwnerId);
					blobCount++;
				}
				else
				{
					BlobManager.InstanceOrNull?.Disconnect(otherChannelObject.OwnerId);
				}
			}

			if (blobCount > 0 && SelfChannel != null)
				BlobManager.Instance.Connect(User.Instance.CurrentUserData.ID);
			else
				BlobManager.InstanceOrNull?.Disconnect(User.InstanceOrNull?.CurrentUserData.ID ?? 0);
		}

		/// <summary>
		/// 다른 유저의 채널에 접속중이며 Voice를 요청중인 경우 OtherBlob을 연결한다.
		/// </summary>
		private bool GetOtherBlobState(ChannelObjectInfo channelObject)
		{
			var uid = channelObject.OwnerId;
			return _voiceRequestingUsers.ContainsKey(uid);
		}

		public void Dispose()
		{
			Disable();
		}

#region ChannelConnection
		private void JoinSelfChannel(SelfMediaChannelNotify notify)
		{
			var channelId      = notify.ChannelId;
			var rtcChannelInfo = notify.RtcChannelInfo;

			if (channelId == null || rtcChannelInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "ChannelId or RtcChannelInfo is null");
				return;
			}

			if (rtcChannelInfo.Direction is Direction.Incomming)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "Self chanel should be outgoing");
				return;
			}

			if (SelfChannel != null)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "SelfChannel is already exist");
				return;
			}

			SelfChannel = SmallTalkHelper.JoinChannel(channelId, rtcChannelInfo);
		}

		private void JoinOtherChannel(OtherMediaChannelNotify notify, ActiveObject targetObject)
		{
			var channelId      = notify.ChannelId;
			var rtcChannelInfo = notify.RtcChannelInfo;

			if (channelId == null || rtcChannelInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "ChannelId or RtcChannelInfo is null");
				return;
			}

			if (rtcChannelInfo.Direction is Direction.Outgoing)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "Other chanel should be incomming");
				return;
			}

			if (_otherChannelObjects.ContainsKey(notify.OwnerId))
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "OtherChannel is already exist");
				return;
			}

			var channel = SmallTalkHelper.JoinChannel(channelId, rtcChannelInfo);
			var channelObject = new ChannelObjectInfo
			{
				Channel      = channel,
				ActiveObject = targetObject,
			};

			_otherChannelObjects.Add(notify.OwnerId, channelObject);
		}

		public void LeaveSelfChannel()
		{
			if (SelfChannel == null)
				return;

			ChannelManager.InstanceOrNull?.RemoveChannel(SelfChannel.Info.ChannelId);
			Components.DeleteSmallTalkRequest request = new Components.DeleteSmallTalkRequest()
			{
				RoomId = SelfChannel.Info.ChannelId
			};
			Api.Media.PostMediaDeleteSmallTalk(request).Forget();
			
			SelfChannel = null;
			BlobManager.InstanceOrNull?.Disconnect(User.InstanceOrNull?.CurrentUserData.ID ?? 0);
		}

		private void LeaveOtherChannel(long ownerId)
		{
			if (!_otherChannelObjects.TryGetValue(ownerId, out var channelObject))
				return;

			_otherChannelObjects.Remove(ownerId);
			LeaveOtherChannelImpl(ownerId, channelObject);
		}

		private void LeaveAllOtherChannels()
		{
			foreach (var channelObject in _otherChannelObjects.Values)
			{
				LeaveOtherChannelImpl(channelObject.OwnerId, channelObject);
			}

			_otherChannelObjects.Clear();
		}

		private void LeaveOtherChannelImpl(long ownerId, ChannelObjectInfo channelObject)
		{
			ChannelManager.InstanceOrNull?.RemoveChannel(channelObject.ChannelId);
			BlobManager.InstanceOrNull?.Disconnect(ownerId);
			
			_voiceRequestingUsers.Remove(ownerId);
			_cameraRequestingUsers.Remove(ownerId);

			TrackPublishRequestChanged?.Invoke(channelObject, eTrackType.VOICE,  false);
			TrackPublishRequestChanged?.Invoke(channelObject, eTrackType.CAMERA, false);
		}
#endregion // ChannelConnection;
	}
}
