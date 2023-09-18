/*===============================================================
* Product:		Com2Verse
* File Name:	CheatMenuItemViewModel.cs
* Developer:	jehyun
* Date:			2022-07-08 17:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Cheat;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[ViewModelGroup("Cheat")]
	public sealed class CheatItemViewModel : ViewModelBase
	{

		private string _cheatName;
		private readonly CheatNode _cheatNode = null;
		private readonly UnityAction<CheatNode> _onCheatSelected;

		public CheatItemViewModel(CheatNode node, UnityAction<CheatNode> onCheatSelected)
		{
			_cheatNode = node;
			OnClickHandler = new CommandHandler(OnClicked);
			_onCheatSelected = onCheatSelected;

			var fontColor = _cheatNode.CheatInfo == null ? "yellow" : "white";
			CheatName = !_cheatNode.IsLastNode ? $"<color={fontColor}><i>{_cheatNode.NodeName}</i></color>" : $"<color={fontColor}>{_cheatNode.NodeName}</color>";
		}

		public CommandHandler OnClickHandler { get; }

		public string CheatName
		{
			get => _cheatName;
			set
			{
				_cheatName = value;
				base.InvokePropertyValueChanged(nameof(CheatName), value);
			}
		}

		private void OnClicked()
		{
			_onCheatSelected?.Invoke(_cheatNode);
		}
	}
}
