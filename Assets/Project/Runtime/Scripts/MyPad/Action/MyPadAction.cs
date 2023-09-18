/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadAction.cs
* Developer:	tlghks1009
* Date:			2022-08-24 13:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.GuestAccess;
using Com2Verse.Logger;
using Com2Verse.Organization;
using Com2Verse.Tutorial;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

/*
 * MyPad ID 테이블 -
 * https://docs.google.com/spreadsheets/d/13yOmgejXRR_WMS3wUP1LGFVs-2Hiq3ml3eQWMFJQDmI/edit#gid=53286131
 * 사용법 -
 * https://jira.com2us.com/wiki/pages/viewpage.action?pageId=300549812
 *
 *
 * 필수!!!!
 * MyPad는 기본적으로 ( ActionMapUIControl ( 을 사용중!!!!!
 * 꼭 ChangePreviousActionMap 호출 또는 InputAction을 다른 곳으로 보내줘야합니다!
*/
namespace Com2Verse.UI
{
	[UsedImplicitly] [MyPadElement("com.com2verse.setting")]
	public class SettingAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			UIManager.Instance.CreatePopup("UI_Popup_Option", (guiView) =>
			{
				if (guiView.IsUnityNull()) return;

				IsActivation = false;
				guiView.Show();
				guiView.OnCompletedEvent += SetCustomInfo;

				var viewModel = guiView.ViewModelContainer.GetViewModel<MetaverseOptionViewModel>();
				guiView.OnOpenedEvent += (guiView) => viewModel.ScrollRectEnable = true;
				guiView.OnClosedEvent += (guiView) => viewModel.ScrollRectEnable = false;
				viewModel.IsAccountOn =  true; // TODO: default값이 필요없다면 삭제
			}).Forget();
		}
	}
	
	[UsedImplicitly] [MyPadElement("com.com2verse.chatbot")]
	public class ChatBotAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			C2VDebug.LogCategory("MyPad", $"{nameof(ChatBotAction)}");
		}
	}
	
	[UsedImplicitly] [MyPadElement("com.com2verse.avatarcustomize")]
	public class AvatarAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			C2VDebug.LogCategory("MyPad", $"{nameof(AvatarAction)}");
			AvatarCustomizeManager.Instance.ShowCustomizePopup();
		}
	}
	
	[UsedImplicitly] [MyPadElement("com.com2verse.escape")]
	public class EscapeAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			UIManager.Instance.CreatePopup("UI_Popup_MyPad_Escape", (guiView) =>
			{
				if (guiView.IsUnityNull()) return;
				
				guiView.Show();
				guiView.OnCompletedEvent += SetCustomInfo;

				var viewModel = guiView.ViewModelContainer.GetViewModel<MyPadEscapeViewModel>();
				viewModel!.TopContext = Localization.Instance.GetString("UI_Escape_Desc_01");
				viewModel.Context     = MyPadString.EscapePopupCanEscape;
				viewModel.GuiView     = guiView;
				viewModel.Title       = MyPadString.EscapePopupTitle;
				viewModel.Yes         = MyPadString.EscapePopupText;
				viewModel.No          = MyPadString.CommonPopupCancel;
				viewModel.OnYesEvent  = (guiVIew) => { UIStackManager.Instance.RemoveAll(); };
				viewModel.OnNoEvent   = null;
			}).Forget();
		}
	}

	[UsedImplicitly] [MyPadElement("com.com2verse.office.connecting")]
	public class ConnectingAction : BaseMyPadAction
	{
		protected override async void DoAction()
		{
			// 조직도 없으면 return
			if (!DataManager.Instance.IsReady)
			{
				UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_ConnectingApp_UnauthenticatedUser_Popup_Text"));
				return;
			}

			// 조직도는 있는데 조직도에 내가 없으면 return
			var myself = await DataManager.Instance.GetMyselfAsync();
			if (myself != null && !myself.IsMine())
			{
				UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_ConnectingApp_UnauthenticatedUser_Popup_Text"));
				return;
			}

			if (myself == null)
			{
				UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_ConnectingApp_UnauthenticatedUser_Popup_Text"));
				return;
			}

			UIManager.Instance.CreatePopup("UI_ConnectingApp", (guiView) =>
			{
				if (guiView.IsUnityNull()) return;

				guiView.Show();
				guiView.OnCompletedEvent += SetCustomInfo;

				var viewModel = guiView.ViewModelContainer.GetViewModel<MeetingInfoListViewModel>();
				viewModel.GuiView = guiView;
			}).Forget();
		}
	}

	[UsedImplicitly] [MyPadElement("com.com2verse.office.teleport")]
	public class TeleportAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			WarpSpaceViewModel.ShowAsync(SetCustomInfo).Forget();
		}
	}
	
	[UsedImplicitly] [MyPadElement("com.com2verse.office.guest")]
	public class GuestAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			if (CurrentScene.SpaceCode is eSpaceCode.MEETING)
			{
				MyPadManager.Instance.ShowErrorPopup(MyPadManager.eErrorPopup.INVALID_SERVICE);
				return;
			}

			UIManager.Instance.CreatePopup("UI_Popup_Guest_Access", guiView =>
			{
				if (guiView.IsUnityNull()) return;

				guiView.Show();
				guiView.OnCompletedEvent += SetCustomInfo;
				
				var viewModel = guiView.ViewModelContainer.GetViewModel<GuestAccessViewModel>();
				viewModel.SetView(guiView);
			}).Forget();
		}
	}
	
	[UsedImplicitly] [MyPadElement("com.com2verse.tutorial")]
	public class TutorialAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			TutorialManager.Instance.TutorialRemindPopUpOpen();
		}
	}
}
