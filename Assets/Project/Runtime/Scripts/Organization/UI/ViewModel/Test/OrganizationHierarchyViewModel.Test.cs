/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyViewModel.Test.cs
* Developer:	jhkim
* Date:			2022-09-08 18:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Organization;

namespace Com2Verse.UI
{
	// Test
	public partial class OrganizationHierarchyViewModel
	{
#region Variables
		private string _cheatGroupIdx;
		private string _cheatIdx;
		public CommandHandler RefreshList { get; private set; }
		public CommandHandler CheatPickItem { get; private set; }
#endregion // Variables

#region Properties
		public string CheatGroupIdx
		{
			get => _cheatGroupIdx;
			set
			{
				_cheatGroupIdx = value;
				InvokePropertyValueChanged(nameof(CheatGroupIdx), value);
			}
		}

		public string CheatIdx
		{
			get => _cheatIdx;
			set
			{
				_cheatIdx = value;
				InvokePropertyValueChanged(nameof(CheatIdx), value);
			}
		}
#endregion // Properties

#region Initialization
		private void InitTest()
		{
			RefreshList = new CommandHandler(OnRefreshList);
			CheatPickItem = new CommandHandler(OnCheatPickItem);
		}
#endregion // Initialization

#region Binding Events
		private void OnRefreshList()
		{
			if (DataManager.Instance.IsReady)
			{
				Organization.DataManager.SendOrganizationChartRequest(DataManager.Instance.GroupID, success =>
				{
					if (success)
					{
						RefreshUI();
					}
				});
			}
		}

		private void OnCheatPickItem()
		{
			if (!int.TryParse(CheatGroupIdx, out var groupIdx)) return;
			if (!int.TryParse(CheatIdx, out var idx)) return;

			PickItem(groupIdx, idx);
		}
#endregion // Binding Events
	}
}
