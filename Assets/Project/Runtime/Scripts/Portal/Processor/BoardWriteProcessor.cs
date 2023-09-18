using Com2Verse.Data;
using Com2Verse.Office;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.BOARD__WRITE)]
	public class BoardWriteProcessor : BaseLogicTypeProcessor
	{
		private GUIView _lastview;
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;

			base.OnTriggerEnter(triggerInParameter);
		}

		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			UIManager.Instance.CreatePopup(BoardManager.Instance.PostPrefabName, (guiView) =>
			{
				guiView.NeedDimmedPopup = true;

				guiView.ViewModelContainer.ClearAll();
				guiView.ViewModelContainer.AddViewModel(BoardManager.Instance.BoardViewModel);

				var viewModel = BoardManager.Instance.BoardViewModel;
				viewModel.ShowMessageBoard = true;

				guiView.Show();
				viewModel.InputText = string.Empty;
				viewModel.ObjectId = triggerInParameter.ParentMapObject.ObjectID.ToString();

				_lastview = guiView;
			}).Forget();
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			base.OnTriggerExit(triggerOutParameter);
			_lastview?.Hide();
		}
	}
}
