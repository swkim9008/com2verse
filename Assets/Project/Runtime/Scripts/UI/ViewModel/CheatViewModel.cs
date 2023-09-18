/*===============================================================
* Product:		Com2Verse
* File Name:	CheatViewModel.cs
* Developer:	jehyun
* Date:			2022-07-08 17:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using Com2Verse.Cheat;

namespace Com2Verse.UI
{
	[ViewModelGroup("Cheat")]
	public sealed class CheatViewModel : ViewModelBase
	{

		private string _currentCheatName;
		private bool _isCheatExecutable;
		private Collection<CheatItemViewModel> _cheatItemCollection = new();
		private Collection<CheatParameterItemViewModel> _cheatParamItemCollection = new();
		private CheatNode _currentCheatNode = null;

		public CheatViewModel()
		{
			OnRunClickHandler = new CommandHandler(OnRunClicked);
		}

		public string CurrentCheatName
		{
			get => _currentCheatName;
			set
			{
				_currentCheatName = value;
				base.InvokePropertyValueChanged(nameof(CurrentCheatName), value);
			}
		}

		public bool IsCheatExecutable
		{
			get => _isCheatExecutable;
			set
			{
				_isCheatExecutable = value;
				base.InvokePropertyValueChanged(nameof(IsCheatExecutable), value);
			}
		}

		public Collection<CheatItemViewModel> CheatItemCollection
		{
			get => _cheatItemCollection;
			set
			{
				_cheatItemCollection = value;
				base.InvokePropertyValueChanged(nameof(CheatItemCollection), value);
			}
		}

		public Collection<CheatParameterItemViewModel> CheatParamItemCollection
		{
			get => _cheatParamItemCollection;
			set
			{
				_cheatParamItemCollection = value;
				base.InvokePropertyValueChanged(nameof(CheatParamItemCollection), value);
			}
		}

		public CommandHandler OnRunClickHandler { get; }

		public override void OnInitialize()
		{
			SetCheatNodeCollection(CheatManager.Instance.RootNode);
		}

		private void OnRunClicked()
		{
#if ENABLE_CHEATING
			if (_currentCheatNode?.CheatInfo != null)
			{
				var parameters = CheatParamItemCollection.Value.Select((viewModel) => viewModel.ParameterValue.Trim()).ToArray();
				_currentCheatNode?.CheatInfo.MethodInfo.Invoke(null, parameters);
				DeselectCheat();
			}
#endif
		}

		private void SetCheatNodeCollection(CheatNode parentNode)
		{
			DeselectCheat();
			CheatItemCollection.Reset();

			// 최상위 노드가 아닐 경우 parent node로 이동할 때 필요한 더미 노드를 추가.
			if (!parentNode.IsRootNode)
				CheatItemCollection.AddItem(new CheatItemViewModel(new CheatNode() { Parent = parentNode.Parent, NodeName = "../" }, OnCheatSelectedEvent));

			foreach (var child in parentNode.Children)
			{
				var item = new CheatItemViewModel(child, OnCheatSelectedEvent);
				CheatItemCollection.AddItem(item);
			}
		}

		private void SetCheatParameter(CheatNode cheatNode)
		{
			DeselectCheat();

			var methodInfo = cheatNode.CheatInfo?.MethodInfo;

			if (methodInfo == null)
				return;

			_currentCheatNode = cheatNode;
			IsCheatExecutable = true;
			CurrentCheatName = _currentCheatNode.NodeName;
			var cheatInfo = _currentCheatNode.CheatInfo;

			var parameterInfos = methodInfo.GetParameters();

			for (int i = 0; i < parameterInfos.Length; ++i)
			{
				var parameterInfo = parameterInfos[i];
				var helpText = (cheatInfo.HelpTexts?.Length > i) ? cheatInfo.HelpTexts[i] : string.Empty;
				var item = new CheatParameterItemViewModel()
				{
					ParameterName = parameterInfo.Name + ":",
					ParameterDescription = (string.IsNullOrEmpty(helpText)) ? "cheat value" : helpText,
					ParameterValue = parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue.ToString() : string.Empty,
				};

				CheatParamItemCollection.AddItem(item);
			}
		}

		private void DeselectCheat()
		{
			IsCheatExecutable = false;
			_currentCheatNode = null;
			CurrentCheatName  = "[ Com2Verse Cheat Console ]";
			CheatParamItemCollection.Reset();
		}

		private void CheatSelected(CheatNode selectedNode)
		{
			if (selectedNode.CheatInfo == null)
				SetCheatNodeCollection(selectedNode.Children.Count != 0 ? selectedNode : selectedNode.Parent);
			else
				SetCheatParameter(selectedNode);
		}

		private void OnCheatSelectedEvent(CheatNode selectedNode)
		{
			CheatSelected(selectedNode);
		}
	}
}
