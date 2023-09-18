/*===============================================================
* Product:		Com2Verse
* File Name:	TMPHyperLink.cs
* Developer:	haminjeong
* Date:			2022-07-15 17:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Option;
using Com2Verse.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(TMP_Text))]
	public class TMPHyperLink : MonoBehaviour
	{
		private struct TextKey
		{
			public const string UICommonNoticePopupTitle = "UI_Common_Notice_Popup_Title";
			public const string UIChatLinkWarningPopup   = "UI_Chat_LinkWarning_Popup_Desc";
		}

		private TMP_Text _textMeshPro;

		void Start()
		{
			_textMeshPro = Util.GetOrAddComponent<TMP_Text>(gameObject);
			_textMeshPro.ForceMeshUpdate();
		}

		private void Update()
		{
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				if (!EventSystem.current.IsPointerOverGameObject()) return;
				if (!CurrentScene.SceneName.Equals("SceneLogin") && !CurrentScene.SceneName.Equals("SceneAvatarSelection"))
				{
					if (UIStackManager.Instance.Count >= 1 &&
						!UIStackManager.Instance.TopMostViewName!.Equals("NotificationPopUpViewModel") &&
                        !UIStackManager.Instance.TopMostViewName!.Equals("UI_Conference_Chat") )
						return;
				}
				CheckHyperLink();
			}
		}

		private void CheckHyperLink()
		{
			Vector2 position  = Mouse.current.position.ReadValue();
			int     linkIndex = TMP_TextUtilities.FindIntersectingLink(_textMeshPro, new Vector3(position.x, position.y, 0), null);
			if (linkIndex != -1)
			{
				TMP_LinkInfo linkInfo = _textMeshPro.textInfo.linkInfo[linkIndex];
				string       url      = linkInfo.GetLinkID();
				if (!url.StartsWith("https://") && !url.StartsWith("http://"))
					url = url.Insert(0, "https://");
				ShowHyperLink(url);
			}
		}

		private void ShowHyperLink(string url)
		{
			var uiManager  = UIManager.Instance;
			var chatOption = OptionController.Instance.GetOption<ChatOption>();

			void ShowWebView() => uiManager.ShowPopupWebView(true, new Vector2(1300, 800), url);
			if (chatOption?.IsOnLinkWarning ?? false)
			{
				var title   = Localization.Instance.GetString(TextKey.UICommonNoticePopupTitle);
				var context = Localization.Instance.GetString(TextKey.UIChatLinkWarningPopup);
				uiManager.ShowPopupYesNo(title, context,_ => ShowWebView());
				return;
			}
			ShowWebView();
		}
	}
}
