using System;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols;
using UnityEngine;

namespace Com2Verse.Interaction
{
	public class SeatManager : Singleton<SeatManager>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private SeatManager() { }

		public void Dispose()
		{
			UnRegisterEvent();
		}

		public void Initialize()
		{
			RegisterEvent();
		}

		public long CurrentChairSerialId { get; set; }
		private GameObject _currentChair;

		public void RegisterEvent()
		{
			MapController.Instance.OnMapObjectCreate += OnObjectCreated;
			MapController.Instance.OnMapObjectRemove += OnObjectRemoved;

			PacketReceiver.Instance.OnUseChairResponseEvent += OnResponseUseChairResponse;
			PlayerController.Instance.OnCharacterStateChanged += CharacterStateChanged;
		}

		public void UnRegisterEvent()
		{
			var mapController = MapController.InstanceOrNull;
			if (mapController != null)
			{
				mapController.OnMapObjectCreate -= OnObjectCreated;
				mapController.OnMapObjectRemove -= OnObjectRemoved;
			}

			var packetReceiver = PacketReceiver.InstanceOrNull;
			if (packetReceiver != null)
			{
				packetReceiver.OnUseChairResponseEvent -= OnResponseUseChairResponse;
			}

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
				playerController!.OnCharacterStateChanged -= CharacterStateChanged;
		}

		private void OnResponseUseChairResponse(Protocols.CommonLogic.UseChairResponse useChairResponse)
		{
			CurrentChairSerialId = useChairResponse.TargetChairId;

			var mapController = MapController.InstanceOrNull;
			if (mapController.IsReferenceNull()) return;

			_currentChair = mapController!.GetStaticObjectByID(CurrentChairSerialId)?.gameObject;
			if (_currentChair.IsUnityNull()) return;

			AttachColliderActivator(_currentChair.transform);
		}

		private void AttachColliderActivator(Transform transform)
		{
			var collider = transform.GetComponent<Collider>();
			if (!collider.IsUnityNull() && !collider.isTrigger)
			{
				collider.isTrigger = true;
				var activator = collider.gameObject.AddComponent<ColliderActivator>();
				activator.TargetCollider = collider;
			}

			foreach (Transform child in transform)
			{
				AttachColliderActivator(child);
			}
		}

		private void CharacterStateChanged(CharacterState lastState, CharacterState newState)
		{
			if (CurrentChairSerialId > 0 && lastState == CharacterState.Sit)
			{
				StandUp();
			}
		}

		public void StandUp()
		{
			Commander.Instance.ChairDisuseCommand(CurrentChairSerialId, User.Instance.CurrentUserData.ObjectID);
			CurrentChairSerialId = -1;
		}

		public async UniTask InitializeTableData()
		{
			// (TODO) Load my seat data
		}

		public void OnObjectCreated(ObjectState state, BaseMapObject mapObject)
		{
		}

		public void OnObjectRemoved(BaseMapObject mapObject)
		{
		}
	}
}
