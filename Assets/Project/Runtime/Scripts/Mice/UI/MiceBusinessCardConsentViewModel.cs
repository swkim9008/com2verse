/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardConsentViewModel.cs
* Developer:	wlemon
* Date:			2023-08-21 16:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Threading;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public class MiceBusinessCardConsentViewModel : ViewModelBase
	{
		public enum ePopupResult
		{
			NONE        = 0,
			AGREE       = 1,
			CANCEL      = 2,
			SHOW_DETAIL = 3,
		}

		public static readonly string BaseResName   = "UI_Popup_BusinessCard_Consent";
		public static readonly string DetailResName = "UI_Popup_BusinessCard_ConsentDetail";

		private bool _isAgree;

		public CommandHandler Agree  { get; private set; }
		public CommandHandler Cancel { get; private set; }
		public CommandHandler ShowDetail { get; private set; }

		public bool IsAgree
		{
			get => _isAgree;
			set => SetProperty(ref _isAgree, value);
		}

		public ePopupResult PopupResult { get; set; }

		public MiceBusinessCardConsentViewModel()
		{
			Agree      = new CommandHandler(OnAgree);
			Cancel     = new CommandHandler(OnCancel);
			ShowDetail = new CommandHandler(OnShowDetail);

			PopupResult = ePopupResult.NONE;
		}

		private void OnAgree()
		{
			if (IsAgree) PopupResult = ePopupResult.AGREE;
		}

		private void OnCancel()
		{
			PopupResult = ePopupResult.CANCEL;
		}

		private void OnShowDetail()
		{
			PopupResult = ePopupResult.SHOW_DETAIL;
		}

		public static async UniTask<ePopupResult> ShowViewUntilResult(bool isDetail, CancellationToken cancellationToken = default)
		{
			var viewModel = ViewModelManager.Instance.GetOrAdd<MiceBusinessCardConsentViewModel>();
			viewModel.PopupResult = ePopupResult.NONE;

			var guiView = default(GUIView);
			await UIManager.Instance.CreatePopup(isDetail ? DetailResName : BaseResName, (loadedGUIView) => { guiView = loadedGUIView; });
			cancellationToken.ThrowIfCancellationRequested();
			guiView.Show();
			await UniTask.WaitUntil(() => viewModel.PopupResult != ePopupResult.NONE, cancellationToken: cancellationToken);
			guiView.Hide();

			return viewModel.PopupResult;
		}
	}
}
