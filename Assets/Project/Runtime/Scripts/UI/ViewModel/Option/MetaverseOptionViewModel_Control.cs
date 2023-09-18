/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel_Control.cs
* Developer:	mikeyid77
* Date:			2023-04-14 12:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.InputSystem;
using Com2Verse.Option;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public partial class MetaverseOptionViewModel
	{
		private ControlOption _controlOption;
		public CommandHandler<int> ResizeInterfaceToggleClicked { get; private set; }
		public CommandHandler ResetControlOptionButtonClicked { get; private set; }
		public CommandHandler RebindActionMapButtonClicked { get; private set; }

		
		[UsedImplicitly]
		public int InterfaceSizeIndex
		{
			get => _controlOption.InterfaceSizeIndex;
			set
			{
				if (_controlOption == null) return;
				_controlOption.InterfaceSizeIndex = value;
				base.InvokePropertyValueChanged(nameof(InterfaceSizeIndex), InterfaceSizeIndex);
			}
		}
		
		private void InitializeControlOption()
		{
			_controlOption = OptionController.Instance.GetOption<ControlOption>();
			ResizeInterfaceToggleClicked = new CommandHandler<int>(OnResizeInterfaceToggleClicked);
			ResetControlOptionButtonClicked = new CommandHandler(OnResetControlOptionButtonClicked);
			RebindActionMapButtonClicked = new CommandHandler(OnRebindActionMapButtonClicked);
		}

		private void OnResizeInterfaceToggleClicked(int index)
		{
			if (InterfaceSizeIndex != index) InterfaceSizeIndex = index;
		}

		private void OnResetControlOptionButtonClicked()
		{
			InterfaceSizeIndex = _controlOption.Reset();
			InputSystemManager.Instance.StateMachine.ChangeState(eSTATE.RESET);
		}

		private void OnRebindActionMapButtonClicked()
		{
			UIManager.Instance.ShowRebindPopup();
		}
	}
}
