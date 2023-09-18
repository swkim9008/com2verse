using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.Office;
using Com2Verse.Option;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.EventTrigger
{
	class PostMessage
	{
		public string Command;
	}
	
	[LogicType(eLogicType.BOARD__READ)]
	public class BoardReadProcessor : BaseLogicTypeProcessor
	{
		private readonly string _uriFormat = "{0}/?lang={1}&objectId={2}&accesstoken={3}&mapname={4} ";
		private readonly string _tokenFormat = "Bearer {0}";
		private readonly string _authorizationHeaderKey = "Authorization";
		private readonly Vector2 _boardWebviewSize = new Vector2(1300, 717 + Define.DEFAULT_WEBVIEW_HEADER_SIZE);
		private readonly Dictionary<string, string> _additionalHeaders = new Dictionary<string, string>();
		
		private GUIView _lastView;

		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;

			base.OnTriggerEnter(triggerInParameter);
		}

		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			
			var objectId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex, 0);
			var mapname = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex, 1);
			
			BaseInteraction(objectId, mapname);
			BoardManager.Instance.BoardViewModel?.OnRead();
		}
		
		public void BaseInteraction(string objectId, string mapname)
		{
			if (string.IsNullOrEmpty(objectId) || string.IsNullOrEmpty(mapname))
				return;
			
			_additionalHeaders.Clear();
			_additionalHeaders.Add(_authorizationHeaderKey, string.Format(_tokenFormat, BoardManager.Instance.PrivateAccessToken));

			var languageOption = OptionController.Instance.GetOption<LanguageOption>().GetLanguage();
			string lang = languageOption switch
			{
				Localization.eLanguage.ENG => "en",
				Localization.eLanguage.KOR => "ko",
				_ => "en"
			};
			
			var uri = string.Format(_uriFormat, BoardManager.Instance.BoardURI, lang, objectId, User.Instance.CurrentUserData.AccessToken, mapname);
			UIManager.Instance.ShowPopupWebViewWithHeaders(false, _boardWebviewSize, uri, _additionalHeaders, (webviewGui =>
			{
				webviewGui.UseFocusEvent = false;
				webviewGui.AllowDuplicate = false;
				webviewGui.OnClosingEvent += OnWebviewClosed;
			}), EmittedAction);

			void EmittedAction(string message)
			{
				var json = JsonUtility.FromJson<PostMessage>(message);
				if (json.Command == null)
					return;
				
				if (json.Command.Equals("post_message"))
				{
					if (BoardManager.Instance.PostPoUpAnimationPlaying("Board_Post_Close"))
						return;
					
					UIManager.Instance.CreatePopup(BoardManager.Instance.PostPrefabName, (guiView) =>
					{
						guiView.NeedDimmedPopup = true;

						guiView.ViewModelContainer.ClearAll();
						guiView.ViewModelContainer.AddViewModel(BoardManager.Instance.BoardViewModel);
				
						var viewModel = BoardManager.Instance.BoardViewModel;
						
						guiView.Show();
						viewModel.ShowMessageBoard = true;
						viewModel.InputText = string.Empty;
						viewModel.ObjectId = objectId;
						_lastView = guiView;
					}).Forget();
				}
			}
		}

		private void OnWebviewClosed(GUIView guiView)
		{
			_lastView?.Hide();
			guiView.UseFocusEvent = true;
		}
	}
}
