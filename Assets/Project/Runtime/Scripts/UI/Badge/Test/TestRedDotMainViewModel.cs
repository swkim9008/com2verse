/*===============================================================
* Product:		Com2Verse
* File Name:	TestRedDotMainViewModel.cs
* Developer:	NGSG
* Date:			2023-04-26 10:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("TestRedDot")]
	public sealed class TestRedDotMainViewModel : ViewModelBase
	{
		private bool _viewSubMain = false;
		public bool ViewSubMain
		{
			get => _viewSubMain;
			set
			{
				_viewSubMain = value;
				base.InvokePropertyValueChanged(nameof(ViewSubMain), value);
				if (value)
				{
					Debug.Log($"ViewSubMain = {value}");
				}
			}
		} 

		public CommandHandler MainMenuClick    { get; private set; }
		public CommandHandler SubMenuClick     { get; private set; }
		public CommandHandler ShirtCreateClick { get; private set; }
		public CommandHandler PantsCreateClick { get; private set; }
		
		private int _shirtStartIndex = 0;
		private int _pantsStartIndex = 0;

		private List<string> _dataTable = new List<string>()
		{
			//10, 30, 50, 90, 100
			"a", "b", "c", "d", "e", "f", "g", "h", "i", "j"
		};

		public static List<RedDotCollectionTestData> ShirtInfoList = new List<RedDotCollectionTestData>();
		public static List<RedDotCollectionTestData> PantsInfoList = new List<RedDotCollectionTestData>();
		
		public TestRedDotMainViewModel()
		{
			MainMenuClick = new CommandHandler(() => ViewSubMain ^= true);
			SubMenuClick  = new CommandHandler(() =>
			{
				UIManager.Instance.CreatePopup("UI_RedDotTest", guiView =>
				{
					if(guiView.name.Contains("(Clone)"))
						guiView.name = guiView.name.Substring(0, guiView.name.LastIndexOf("(Clone)"));
					guiView.Show();

					var viewModel = guiView.ViewModelContainer.GetViewModel<TestRedDotSubViewModel>();
					viewModel.CurrentView = guiView;

					//RedDotManager.Instance.Notify("MAIN_MENU_SUB1_TAB1");
					RedDotManager.Instance.Notify("MAIN_MENU_SUB1_TAB1_ITEM");
					RedDotManager.Instance.Notify("MAIN_MENU_SUB1_TAB2_ITEM");
					//RedDotManager.Instance.NotifyAll();
				});
			});

			ShirtCreateClick = new CommandHandler(()=>
			{
				RedDotManager.Instance.AddBadge(new RedDotData("MAIN_MENU_SUB1_TAB1_ITEM", _shirtStartIndex++));
			});
			
			PantsCreateClick = new CommandHandler(() =>
			{
				RedDotManager.Instance.AddBadge(new RedDotData("MAIN_MENU_SUB1_TAB2_ITEM", _pantsStartIndex++));
			});
		}

	}
}
