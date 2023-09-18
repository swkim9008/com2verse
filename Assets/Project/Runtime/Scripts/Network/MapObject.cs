/*===============================================================
* Product:		Com2Verse
* File Name:	MapObject.cs
* Developer:	haminjeong
* Date:			2022-12-29 12:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Globalization;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.PlayerControl;
using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.Network
{
	public partial class MapObject : BaseMapObject
	{
		public const string NickNameTag = "NickName";

#if !METAVERSE_RELEASE && UNITY_EDITOR
		[field: SerializeField, ReadOnly] private Protocols.CharacterState _characterState;
#endif
		private float _distance = 0f; // 나와의 거리
		public float DistanceFromMe => _distance;

		public float ObjectHeight { get; protected set; } = FallbackHeight;

		protected virtual void Awake()
		{
			OnObjectAwakeEx();
		}

		protected virtual void OnObjectAwakeEx()
		{
			OnHudStateChanged += OnHudCullingGroupStateChanged;
#if !METAVERSE_RELEASE && UNITY_EDITOR
			AwakeOnDebugEditor();
#endif
		}

		protected virtual void OnDestroy()
		{
			ReleaseObject();
			OnHudStateChanged -= OnHudCullingGroupStateChanged;
		}

		/// <inheritdoc />
		public override void Init(long serial, long ownerID, bool needUpdate)
		{
			base.Init(serial, ownerID, needUpdate);

			SetLayer();
		}

		protected virtual void SetLayer()
		{
			gameObject.layer = (int)Define.eLayer.OBJECT;
			var childColliders = transform.GetComponentsInChildren<Collider>();
			if (childColliders != null)
			{
				foreach (Collider child in childColliders)
				{
					if (!child.isTrigger)
						child.gameObject.layer = (int)Define.eLayer.OBJECT;
				}
			}
		}

		protected override void OnObjectUpdated()
		{
			base.OnObjectUpdated();

			if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(ControlPoints[1].NickName))
			{
				if (string.IsNullOrEmpty(GetStringFromTags(NickNameTag)))
				{
					Name = ControlPoints[1].NickName;
					if (IsMine)
						User.Instance.CurrentUserData.UserName = Name;
				}
			}

			if (User.Instance.CharacterObject.IsUnityNull() || IsMine || transform.IsUnityNull())
				_distance = 0;
			else
				_distance = Vector3.Distance(User.Instance.CharacterObject.transform.position, transform.position);

#if !METAVERSE_RELEASE && UNITY_EDITOR
			_characterState = (Protocols.CharacterState)CharacterState;
#endif
#if ENABLE_CHEATING && !METAVERSE_RELEASE
			if (User.Instance.NetworkDebugViewModel != null && IsMine && ProcessDelay > 0)
				User.Instance.NetworkDebugViewModel.ProcessDelayText = $"Process {ProcessDelay.ToString(CultureInfo.InvariantCulture)}ms";
#endif // ENABLE_CHEATING
		}

		protected override void OnObjectReleased()
		{
#if UNITY_EDITOR && ENABLE_CHEATING && !METAVERSE_RELEASE
			if (IsDebug && MapController.InstanceExists) MapController.Instance.RemoveObjectDebug(ObjectID);
#endif // ENABLE_CHEATING

			if (IsMine)
			{
				if (User.InstanceExists)
					User.Instance.ReleaseMapObject(this);

				var playerController = PlayerController.InstanceOrNull;
				if (!playerController.IsUnityNull())
					PlayerController.Instance.RemoveEvents();

				var animationManager = AnimationManager.InstanceOrNull;
				if (!animationManager.IsUnityNull())
					AnimationManager.Instance.DisableEvents();
			}

			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager != null && cameraManager.MainCameraTarget == transform)
				cameraManager.MainCameraTarget = null;

			if (!HudCullingGroup.IsUnityNull())
				HudCullingGroup!.Remove(this);
			HudCullingGroup = null;

			base.OnObjectReleased();
		}

		public virtual void SetName(string nameString)
		{
			if (string.IsNullOrEmpty(nameString)) return;
			Name = nameString;
			if (IsMine)
				User.Instance.CurrentUserData.UserName = Name;
		}
	}
}
