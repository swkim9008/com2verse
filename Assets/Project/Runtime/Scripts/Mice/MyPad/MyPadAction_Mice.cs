/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadAction_Mice.cs
* Developer:	klizzard
* Date:			2023-07-17 11:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
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
	[UsedImplicitly] [MyPadElement("com.com2verse.convention")]
	public class MiceAction : BaseMyPadAction
	{
		protected override void DoAction()
		{
			UIManager.Instance.CreatePopup("UI_Popup_MiceApp", guiView =>
			{
				if (guiView.IsUnityNull()) return;

				// NestedViewModel 처리를 위해 미리 등록
				//var viewModel = ViewModelManager.Instance.GetOrAdd<MiceAppViewModel>();
				var viewModel = guiView.ViewModelContainer.GetOrAddViewModel<MiceAppViewModel>();
				guiView.Show();
				guiView.OnCompletedEvent += SetCustomInfo;

				viewModel.SetViewMode(MiceAppViewModel.eViewMode.DEFAULT);
			}).Forget();
		}
	}
}
