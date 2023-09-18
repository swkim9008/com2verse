/*===============================================================
* Product:		Com2Verse
* File Name:	RebindActionViewModel.cs
* Developer:	mikeyid77
* Date:			2023-04-12 10:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class RebindActionViewModel : ViewModelBase
	{
		private Collection<RebindActionContentViewModel> _rebindActionContentCollection = new();
		private Vector2 _contentPos = Vector2.zero;

		private bool _isChanged = false;
		private bool _startRebindTrigger = false;
		private string _targetRebind = string.Empty;
		
		public GUIView CurrentView { get; set; }
		public CommandHandler<bool> CloseButtonClicked { get; }
		public CommandHandler CancelButtonClicked { get; }
		public CommandHandler ResetButtonClicked { get; }
		
		public RebindActionViewModel()
		{
			InputSystemManager.Instance.CheckDuplicateBindEvent += CheckDuplicate;
			InputSystemManager.Instance.RefreshBindingUiEvent += RefreshBindingUi;
			CloseButtonClicked = new CommandHandler<bool>(OnCloseButtonClicked);
			CancelButtonClicked = new CommandHandler(OnCancelButtonClicked);
			ResetButtonClicked = new CommandHandler(OnResetButtonClicked);

			SetContent(true, InputSystemManager.Instance.GetTargets());
		}

		#region Property
		public Collection<RebindActionContentViewModel> RebindActionContentCollection
		{
			get => _rebindActionContentCollection;
			set
			{
				_rebindActionContentCollection = value;
				base.InvokePropertyValueChanged(nameof(RebindActionContentCollection), RebindActionContentCollection);
			}
		}

		public Vector2 ContentPos
		{
			get => _contentPos;
			set
			{
				_contentPos = value;
				base.InvokePropertyValueChanged(nameof(ContentPos), ContentPos);
			}
		}

		public bool StartRebindTrigger
		{
			get => _startRebindTrigger;
			set
			{
				_startRebindTrigger = value;
				base.InvokePropertyValueChanged(nameof(StartRebindTrigger), StartRebindTrigger);
			}
		}

		public string TargetRebind
		{
			get => _targetRebind;
			set
			{
				_targetRebind = value;
				base.InvokePropertyValueChanged(nameof(TargetRebind), TargetRebind);
			}
		}
#endregion // Property

#region Command
		private void OnCloseButtonClicked(bool isApply)
		{
			if (isApply)
			{
				if (CheckNeedRebind())
				{
					UIManager.Instance.ShowPopupYesNo(RebindString.PopupCommonTitle, RebindString.PopupNeedRebindContext, 
						(guiView) =>
						{
							C2VDebug.LogCategory("ControlOption", $"Apply Rebind");
							CurrentView.Hide();
						},
						null,
						null,
						RebindString.PopupCommonYes,
						RebindString.PopupCommonNo);
				}
				else
				{
					C2VDebug.LogCategory("ControlOption", $"Apply Rebind");
					CurrentView.Hide();
				}
			}
			else
			{
				if (_isChanged)
				{
					UIManager.Instance.ShowPopupYesNo(RebindString.PopupCommonTitle, RebindString.PopupSaveContext, 
						(guiView) =>
						{
							OnCloseButtonClicked(true);
						},
						(guiView) =>
						{
							C2VDebug.LogCategory("ControlOption", $"Cancel Rebind");
							InputSystemManager.Instance.StateMachine.ChangeState(eSTATE.RESET);
							CurrentView.Hide();
						},
						null,
						RebindString.PopupCommonYes,
						RebindString.PopupCommonNo);
				}
				else
				{
					CurrentView.Hide();
				}
			}
		}

		private void OnCancelButtonClicked()
		{
			InputSystemManager.Instance.CancelBinding();
		}

		private void OnResetButtonClicked()
		{
			C2VDebug.LogCategory("ControlOption", $"Reset Action Asset");
			InputSystemManager.Instance.StateMachine.ChangeState(eSTATE.RESET);
		}
#endregion // Command

#region Method
		private void SetContent(bool isInit, List<InputSystemManager.ViewTarget> targetList)
		{
			var targetIndex = 0;
			foreach (var target in targetList)
			{
				if (isInit)
				{
					_rebindActionContentCollection.AddItem(
						new RebindActionContentViewModel(target.CanRebind, StartRebind)
						{
							Index = target.Index,
							ActionName = target.ActionName,
							BindName = target.BindName,
							BindPath = target.BindPath
						});
				}
				else
				{
					var targetViewModel = _rebindActionContentCollection.Value[targetIndex];
					targetViewModel.Index = target.Index;
					targetViewModel.BindPath = target.BindPath;

					// TODO : 변경되었는지 확인하는 플로우 수정 필요
					_isChanged = true;
				}
				targetIndex++;
			}
		}
		
		private void StartRebind(int index, string target)
		{
			C2VDebug.LogCategory("ControlOption", $"Try Rebind Action : {target}(index - {index})");
			
			TargetRebind = target;
			StartRebindTrigger = true;
			
			InputSystemManager.Instance.Index = index;
			InputSystemManager.Instance.StateMachine.ChangeState(eSTATE.REBIND);
		}

		private void CheckDuplicate(string path, string actionName, int index)
		{
			var name 
				= (RebindActionHelper.RebindActionDict.ContainsKey(actionName))
					? RebindActionHelper.RebindActionDict[actionName].Name
					: actionName;
			
			UIManager.Instance.ShowPopupYesNo(RebindString.PopupCommonTitle, RebindString.PopupBindingContext(name), 
				(guiView) =>
				{
					InputSystemManager.Instance.ApplyDuplicateBinding(index);
				},
				(guiView) =>
				{
					InputSystemManager.Instance.CancelBinding();
				},
				null,
				RebindString.PopupCommonYes,
				RebindString.PopupCommonNo);
		}

		private bool CheckNeedRebind()
		{
			var result = false;
			foreach (var viewModel in _rebindActionContentCollection.Value)
			{
				if (viewModel.NeedRebind())
				{
					result = true;
					break;
				}
			}
			return result;
		}
		
		private void RefreshBindingUi()
		{
			if (CurrentView == null) return;
			if (CurrentView.VisibleState == GUIView.eVisibleState.OPENED)
			{
				C2VDebug.LogCategory("ControlOption", $"Refresh Action Asset");
			
				StartRebindTrigger = false;
				TargetRebind = string.Empty;
			
				SetContent(false, InputSystemManager.Instance.GetTargets());
			}
		}
#endregion // Method
	}
}
