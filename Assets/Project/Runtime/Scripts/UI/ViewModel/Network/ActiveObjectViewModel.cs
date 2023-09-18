/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObjectViewModel.cs
* Developer:	eugene9721
* Date:			2022-08-18 11:53
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.Network;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Network")]
	public sealed class ActiveObjectViewModel : MapObjectViewModel, INestedViewModel
	{
#region INestedViewModel
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();

		public ChatUserViewModel ChatViewModel { get; }
#endregion INestedViewModel

		public new static ActiveObjectViewModel Empty { get; } = new(null);

		[UsedImplicitly] public CommandHandler OpenAnotherUserMenu { get; }

#region Variables
		// Avatar Hud
		private bool    _isActive;
		private Vector2 _rectPosition = Utils.Define.INACTIVE_UI_POSITION;
		private int     _sortOrder;

		// User Function
		private bool _isOnAnotherUserMenuUI;
#endregion Variables

#region Properties
		/// <summary>
		/// MapObject 하위 2D UI의 활성화 여부 (=캔버스 범위 내에 있고 Occluded가 아닌지)
		/// </summary>
		[UsedImplicitly]
		public bool IsActive
		{
			get => _isActive;
			private set
			{
				var prevValue = _isActive;
				if (prevValue == value)
					return;

				SetProperty(ref _isActive, value);
			}
		}

		/// <summary>
		/// MapObject 하위 2D UI의 위치
		/// </summary>
		[UsedImplicitly]
		public Vector2 RectPosition
		{
			get => _rectPosition;
			private set
			{
				var prevValue = _rectPosition;
				if (prevValue == value)
					return;

				SetProperty(ref _rectPosition, value);
			}
		}

		/// <summary>
		/// MapObject 하위 2D UI의 정렬 순서 (=SiblingIndex)
		/// </summary>
		[UsedImplicitly]
		public int SortOrder
		{
			get => _sortOrder;
			set
			{
				var prevValue = _sortOrder;
				if (prevValue == value)
					return;

				SetProperty(ref _sortOrder, value);
			}
		}

		[UsedImplicitly]
		public bool IsOnAnotherUserMenuUI
		{
			get => _isOnAnotherUserMenuUI;
			set => SetProperty(ref _isOnAnotherUserMenuUI, value && !IsMine);
		}
#endregion Properties

		public ActiveObjectViewModel(ActiveObject? activeObject) : base(activeObject)
		{
			OpenAnotherUserMenu = new CommandHandler(OnOpenAnotherUserMenu);
			ChatViewModel       = new ChatUserViewModel(activeObject);

			NestedViewModels.Add(ChatViewModel);
		}

		protected override void Dispose(bool disposing)
		{
			ChatViewModel.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnPrevValueUnassigned(MapObject value)
		{
			base.OnPrevValueUnassigned(value);

			value.Updated -= OnObjectUpdated;
		}

		protected override void OnCurrentValueAssigned(MapObject value)
		{
			base.OnCurrentValueAssigned(value);

			value.Updated += OnObjectUpdated;
		}

		public override void RefreshViewModel()
		{
			base.RefreshViewModel();

			InvokePropertyValueChanged(nameof(IsActive),     IsActive);
			InvokePropertyValueChanged(nameof(RectPosition), RectPosition);
			InvokePropertyValueChanged(nameof(SortOrder),    SortOrder);

			InvokePropertyValueChanged(nameof(IsOnAnotherUserMenuUI), IsOnAnotherUserMenuUI);

			OnObjectUpdated(Value);
		}

		private void OnObjectUpdated(BaseMapObject? baseMapObject)
		{
			if (baseMapObject is not MapObject mapObject)
				return;

			// 캐릭터 레이어가 감추어져 있을때 HUD도 감춰준다.
            var mainCamera = CameraManager.InstanceOrNull?.MainCamera;
            if (mainCamera != null)
			{
				if (0 == (mainCamera.cullingMask & (1 << LayerMask.NameToLayer("Character"))))
				{
					SetHudDisabled();
                    return;
				}
            }

            if (!mapObject.IsOccluded)
				SetHudPosition(mapObject);
			else
				SetHudDisabled();
		}

		private void SetHudPosition(MapObject mapObject)
		{
			var guiView = User.Instance.UserFunctionUI?.GuiView;
			if (guiView.IsReferenceNull()) return;

			var canvasScaler = guiView!.CanvasScaler;
			if (canvasScaler.IsUnityNull()) return;

			var mainCamera = CameraManager.InstanceOrNull?.MainCamera;
			if (mainCamera.IsUnityNull()) return;

            var uiRoot = mapObject.GetUIRoot();
			if (uiRoot.IsUnityNull()) return;

			// Canvas = Screen Space
			// Canvas Scaler = Scale With Screen Size
			var rectPosition = mainCamera!.WorldToScreenPoint(uiRoot!.position);

			float referenceWidth  = canvasScaler!.referenceResolution.x;
			float referenceHeight = canvasScaler.referenceResolution.y;

			rectPosition.x = (rectPosition.x / Screen.width)  * referenceWidth;
			rectPosition.y = (rectPosition.y / Screen.height) * referenceHeight;

			SetHudEnabled(rectPosition);
		}

		private void SetHudEnabled(Vector2 position)
		{
			IsActive     = true;
			RectPosition = position;
		}

		private void SetHudDisabled()
		{
			IsActive     = false;
			RectPosition = Utils.Define.INACTIVE_UI_POSITION;
		}

		private void OnOpenAnotherUserMenu()
		{
			if (OwnerId == 0) return;
			if (User.InstanceExists)
			{
				var userFunctionUI = User.Instance.UserFunctionUI;
				if (userFunctionUI == null) return;
				if (OwnerId == User.Instance.CurrentUserData.ID) userFunctionUI.ToggleUserMenuUI();
				else userFunctionUI.ToggleAnotherUserMenuUI(OwnerId);
			}
		}
	}
}
