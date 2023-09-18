/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadItemViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-23 10:37
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Data;

namespace Com2Verse.UI
{
	[ViewModelGroup("MyPad")]
	public sealed partial class MyPadItemViewModel : ViewModelBase
	{
		private readonly Action<string> _onItemClickedAction;
		private readonly Action<string> _onItemRemovedAction;
		private readonly Color _enableColor = new Color(1, 1, 1, 1);
		private readonly Color _disableColor = new Color(1, 1, 1, 160 / 255f);

		private Sprite _icon;
		private Color _iconColor;
		private string _name;
		private bool _notifyView = false;
		private int _notifyCount = 0;
		private bool _enableApp = true;

		private bool _removeToggle = false;
		private Transform _removeToggleRect = null;
		private UIBlocker _removeToggleBlocker = null;
		public CommandHandler RemoveButtonClicked { get; }

		public MyPadItemViewModel(Action<string> onClick, Action<string> onRemove)
		{
			_onItemClickedAction = onClick;
			_onItemRemovedAction = onRemove;
			RemoveButtonClicked = new CommandHandler(RemoveMyPadItemEvent);
		}

		partial void RegisterRedDot();
		partial void UnregisterRedDot();
		
		public override void OnInitialize()
		{
			base.OnInitialize();

			RegisterRedDot();
		}
        
		public override void OnRelease()
		{
			UnregisterRedDot();
			
			base.OnRelease();
		}

#region Property
		public string Id { get; set; }

		public int Index { get; set; }

		public eServiceType Type { get; set; }

		public bool IsDelete { get; set; }

		public Sprite Icon
		{
			get => _icon;
			set => SetProperty(ref _icon, value);
		}

		public string IconName
		{
			set => Icon = SpriteAtlasManager.Instance.GetSprite("Atlas_MyPad", value);
		}

		public Color IconColor
		{
			get => _iconColor;
			set => SetProperty(ref _iconColor, value);
		}

		public string Name
		{
			get => Localization.Instance.GetString(_name);
			set => SetProperty(ref _name, value);
		}

		public bool NotifyView
		{
			get => _notifyView;
			set => SetProperty(ref _notifyView, value);
		}

		public string NotifyCount
		{
			get => _notifyCount.ToString();
			set
			{
				_notifyCount = Convert.ToInt32(value);
				InvokePropertyValueChanged(nameof(NotifyCount), NotifyCount);
				NotifyView = (_notifyCount > 0);
			}
		}

		public bool EnableApp
		{
			get => _enableApp;
			set
			{
				IconColor = (value) ? _enableColor : _disableColor;
				SetProperty(ref _enableApp, value);
			}
		}

		public bool IsHold
		{
			get => false;
			set
			{
				if (!EnableApp) return;
				if (value) RefreshRemoveToggle();
				else ClickMyPadItemEvent();
			}
		}

		public bool RemoveToggle
		{
			get => _removeToggle;
			set => SetProperty(ref _removeToggle, value);
		}

		public Transform RemoveToggleRect
		{
			get => _removeToggleRect;
			set => _removeToggleRect = value;
		}
#endregion // Property

#region Command
		private void ClickMyPadItemEvent()
		{
			_onItemClickedAction?.Invoke(Id);
		}

		private void RemoveMyPadItemEvent()
		{
			_onItemRemovedAction?.Invoke(Id);
		}

		public void RefreshMyPadItem()
		{
			EnableApp = MyPadManager.Instance.CheckEnableApp(Type);
		}
#endregion // Command

#region Utils
		private void RefreshRemoveToggle()
		{
			if (IsDelete)
			{
				RemoveToggle = !RemoveToggle;
				if (RemoveToggle) _removeToggleBlocker = UIBlocker.CreateBlocker(RemoveToggleRect, RefreshRemoveToggle);
				else _removeToggleBlocker.DestroyBlocker();
			}
		}
#endregion // Utils
	}
}
