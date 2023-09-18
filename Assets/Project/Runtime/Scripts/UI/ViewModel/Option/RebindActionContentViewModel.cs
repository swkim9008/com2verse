/*===============================================================
* Product:		Com2Verse
* File Name:	RebindActionContentViewModel.cs
* Developer:	mikeyid77
* Date:			2023-04-12 10:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.InputSystem;

namespace Com2Verse.UI
{
	public sealed class RebindActionContentViewModel : ViewModelBase
	{
		public enum eBindingState
		{
			CANT_CHANGE,
			COMPLETE,
			CHANGING,
			NEED_REBIND,
		}
		
		private eBindingState _currentState = eBindingState.COMPLETE;
		private bool _canRebind;
		private int _index;
		private string _actionName;
		private string _bindName;
		private string _bindPath;
		private Action<int, string> _startRebindAction;
		public CommandHandler RebindButtonClicked { get; }
		
		public RebindActionContentViewModel() { }

		public RebindActionContentViewModel(bool canRebind, Action<int, string> startRebindAction)
		{
			_canRebind = canRebind;
			RebindButtonClicked = new CommandHandler(OnRebindButtonClicked);
			_startRebindAction = startRebindAction;
		}

#region Property
		public eBindingState CurrentState
		{
			get => _currentState;
			set
			{
				if (_currentState == value) return;

				_currentState = value;
				base.InvokePropertyValueChanged(nameof(CurrentState), CurrentState);
			}
		}

		public int Index
		{
			get => _index;
			set
			{
				_index = value;
				base.InvokePropertyValueChanged(nameof(Index), Index);
			}
		}

		public string ActionName
		{
			get => _actionName;
			set
			{
				_actionName = value;
				base.InvokePropertyValueChanged(nameof(ActionName), ActionName);
			}
		}
		
		public string BindName
		{
			get => _bindName;
			set
			{
				_bindName = value;
				base.InvokePropertyValueChanged(nameof(BindName), BindName);
			}
		}
		
		public string BindPath
		{
			get => _bindPath;
			set
			{
				_bindPath = value;
				base.InvokePropertyValueChanged(nameof(BindPath), BindPath);

				if (!_canRebind)
					CurrentState = eBindingState.CANT_CHANGE;
				else if (string.IsNullOrEmpty(value))
					CurrentState = eBindingState.NEED_REBIND;
				else
					CurrentState = eBindingState.COMPLETE;
			}
		}
#endregion

#region Command
		private void OnRebindButtonClicked()
		{
			if (!_canRebind)
			{
				// TODO : UI 변경 후 수정 필요
				UIManager.Instance.ShowPopupCommon(RebindString.PopupCantBind);
				return;
			}
			if (CurrentState == eBindingState.CHANGING) return;
					
			CurrentState = eBindingState.CHANGING;
			_startRebindAction?.Invoke(Index, ActionName);
		}
#endregion // Command

#region Method
		public bool NeedRebind()
		{
			return CurrentState == eBindingState.NEED_REBIND;
		}
#endregion // Method
	}
}
