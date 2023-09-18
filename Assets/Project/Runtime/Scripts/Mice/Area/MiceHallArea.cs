/*===============================================================
* Product:		Com2Verse
* File Name:	MiceHallArea.cs
* Developer:	ikyoung
* Date:			2023-03-31 17:54
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Threading;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.Logger;
using Com2Verse.Rendering.Instancing;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols;
using UnityEngine;

namespace Com2Verse.Mice
{
	public sealed class MiceHallArea : MiceArea
	{
		public MiceHallArea()
		{
			MiceAreaType = eMiceAreaType.HALL;
		}

		private MiceSeatController _seatController                  = default;
		private float              _lastGhostAvatarStartDistance    = 0.0f;
		private float              _lastGhostAvatarCompleteDistance = 0.0f;
		private bool               _gpuInstancingSettingsChanged    = false;
		private int                _listenerCount                   = 0;

		public override void OnStart(eMiceAreaType prevArea)
		{
			base.OnStart(prevArea);

			MapController.Instance.OnMapObjectWillCreate += OnMapObjectWillCreate;
			MapController.Instance.OnMapObjectCreate     += OnMapObjectCreate;
			MapController.Instance.OnMapObjectRemove     += OnMapObjectRemove;

			if (_seatController == null)
				_seatController = GameObject.FindObjectOfType<MiceSeatController>();
			_listenerCount                = 0;
			_gpuInstancingSettingsChanged = false;

			MiceService.Instance.ChangeCamera(0);
			MiceEffectManager.Instance.SetActive(false);
			SetGPUInstancingSettings();
		}

		public override void OnStop()
		{
			base.OnStop();

			MapController.Instance.OnMapObjectWillCreate -= OnMapObjectWillCreate;
			MapController.Instance.OnMapObjectCreate     -= OnMapObjectCreate;
			MapController.Instance.OnMapObjectRemove     -= OnMapObjectRemove;

			ResetGPUInstancingSettings();
		}

		public override void OnTeleportCompletion()
		{
			base.OnTeleportCompletion();

			var useSeat      = false;
			var playCutScene = false;
			switch (MiceService.Instance.CurrentUserAuthority)
			{
				case MiceWebClient.eMiceAuthorityCode.NORMAL:
				case MiceWebClient.eMiceAuthorityCode.STAFF:
					useSeat      = true;
					playCutScene = true;
					break;
				case MiceWebClient.eMiceAuthorityCode.OPERATOR:
					useSeat      = true;
					playCutScene = false;
					break;
			}

			if (useSeat)
			{
				UseSeat(User.Instance.CharacterObject);
			}

			if (playCutScene)
			{
				MiceEffectManager.Instance.SetActive(false);
				MiceService.Instance.ChangeCurrentState(eMiceServiceState.PLAYING_CUTSCENE);
			}
			else
			{
				MiceEffectManager.Instance.SetActive(true);
				MiceService.Instance.ChangeCurrentState(eMiceServiceState.SESSION_PLAYING);
			}
		}

		public void OnMapObjectWillCreate(Protocols.ObjectState objectState)
		{
			if (objectState.GetConferenceObjectType() == eConferenceObjectType.LISTENER)
			{
				_listenerCount++;
			}
		}

		public void OnMapObjectCreate(Protocols.ObjectState objectState, BaseMapObject mapObject)
		{
			if (mapObject is not ActiveObject activeObject) return;
			if (objectState.GetConferenceObjectType() != eConferenceObjectType.LISTENER) return;

			// 태그 갱신을 위해 미리 업데이트
			activeObject.UpdateObject();
			if (activeObject.IsMine) return;

			UseSeat(activeObject);
		}

		public void OnMapObjectRemove(BaseMapObject mapObject)
		{
			if (mapObject is not ActiveObject activeObject) return;
			if (activeObject.ConferenceObjectType == eConferenceObjectType.LISTENER)
			{
				_listenerCount--;
			}

			if (activeObject.IsMine) return;
			ClearSeat(activeObject);
		}

		public bool UseSeat(ActiveObject activeObject)
		{
			if (_seatController.IsUnityNull()) return false;

			_seatController.UseSeat(activeObject);
			return true;
		}

		public void ClearSeat(ActiveObject activeObject)
		{
			if (_seatController.IsUnityNull()) return;

			_seatController.ClearSeat(activeObject);
		}

		public override async UniTask RequestEnterMiceLounge(MiceEventID eventID, CancellationTokenSource cts)
		{
			C2VDebug.LogMethod(GetType().Name);
			Commander.Instance.RequestEnterMiceLounge(MiceService.Instance.EventID);
			await UniTask.CompletedTask;
		}

		public override async UniTask RequestEnterMiceFreeLounge(MiceEventID eventID, CancellationTokenSource cts)
		{
			C2VDebug.LogMethod(GetType().Name);
			Commander.Instance.RequestEnterMiceFreeLounge(MiceService.Instance.EventID);
			await UniTask.CompletedTask;
		}

		public override async UniTask RequestEnterMiceHall(MiceSessionID sessionID, CancellationTokenSource cts)
		{
			C2VDebug.LogMethod(GetType().Name);
			await UniTask.CompletedTask;
		}

		public override async UniTask RequestEnterMiceLobby(CancellationTokenSource cts)
		{
			await base.RequestEnterMiceLobby(cts);
			C2VDebug.LogMethod(GetType().Name);
			Commander.Instance.RequestEnterMiceLobby();
		}

		public override void OnMiceRoomNotify(Protocols.Mice.MiceRoomNotify response)
		{
			base.OnMiceRoomNotify(response);
			_informationMessage = response.EventName;
		}

		public override string CurrentAreaDisplayInfo()
		{
			// TODO 임시
			return $"현재 위치\n{MiceAreaType.ToName()}\n{MiceService.Instance.SessionID}번 세션\n{MiceService.Instance.CurrentStateType}";
		}

		public override long GetRoomID()
		{
			return MiceService.Instance.SessionID;
		}

		public override MiceWebClient.MiceType GetMiceType()
		{
			return MiceWebClient.MiceType.ConferenceSession;
		}

		public override bool CheckReservedObject(ObjectState objState)
		{
			if (objState.GetConferenceObjectType() != eConferenceObjectType.LISTENER) return false;
			if (objState.Serial                    == MapController.Instance.UserSerialID) return false;

			return _listenerCount >= MiceSeatController.Instance.TotalSeatCount;
		}

		private void SetGPUInstancingSettings()
		{
			if (!_gpuInstancingSettingsChanged)
			{
				_lastGhostAvatarStartDistance    = GPUInstancingManager.GhostAvatarStartDistance;
				_lastGhostAvatarCompleteDistance = GPUInstancingManager.GhostAvatarCompleteDistance;
			}

			var mainCamera = CameraManager.Instance.MainCamera;
			var ghostBase  = default(Transform);
			if (!mainCamera.IsUnityNull())
				ghostBase = mainCamera!.transform;

			GPUInstancingManager.Instance.SetGhostBase(GPUInstancingManager.eGhostDistanceCheckType.TARGET, ghostBase);
			//TODO: 거리 조절 필요
			GPUInstancingManager.GhostAvatarStartDistance    = 8.0f;
			GPUInstancingManager.GhostAvatarCompleteDistance = 30.0f;

			_gpuInstancingSettingsChanged = true;
		}

		private void ResetGPUInstancingSettings()
		{
			GPUInstancingManager.GhostAvatarStartDistance    = _lastGhostAvatarStartDistance;
			GPUInstancingManager.GhostAvatarCompleteDistance = _lastGhostAvatarCompleteDistance;
			GPUInstancingManager.Instance.SetGhostBase();
			_gpuInstancingSettingsChanged = false;
		}
	}
}
