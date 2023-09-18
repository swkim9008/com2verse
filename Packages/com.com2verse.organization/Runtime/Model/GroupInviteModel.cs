/*===============================================================
* Product:		Com2Verse
* File Name:	GroupInviteModel.cs
* Developer:	jhkim
* Date:			2022-07-27 14:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using MemberIdType = System.Int64;

namespace Com2Verse.Organization
{
	public struct GroupInviteModel
	{
		// private List<GroupInfo> GroupInfos;
		public GroupInviteSelectedInfo SelectedInfo;

		public static GroupInviteModel CreateNew() =>
			new GroupInviteModel
			{
				// GroupInfos = new List<GroupInfo>(),
				SelectedInfo = GroupInviteSelectedInfo.CreateNew(),
			};

		// private bool Validate() => GroupInfos != null && SelectedInfo.Validate();

		public void Reset()
		{
			// GroupInfos.Clear();
			SelectedInfo.Reset();
		}
	}

	public struct GroupInviteSelectedInfo
	{
		private Dictionary<MemberIdType, CheckMemberListModel> _selectedMapItems;
		public IReadOnlyDictionary<MemberIdType, CheckMemberListModel> SelectedMap => _selectedMapItems;
		public static GroupInviteSelectedInfo CreateNew() =>
			new GroupInviteSelectedInfo
			{
				_selectedMapItems = new(),
			};

		private MemberIdType GetKey(CheckMemberListModel item) => item.Info.Member.AccountId;
		public bool Contains(CheckMemberListModel item) => _selectedMapItems.ContainsKey(GetKey(item));
		public void AddItem(CheckMemberListModel item)
		{
			var key = GetKey(item);
			if (_selectedMapItems.ContainsKey(key)) return;
			_selectedMapItems.Add(key, item);
		}

		public void RemoveItem(CheckMemberListModel item)
		{
			var key = GetKey(item);
			if (_selectedMapItems.ContainsKey(key))
				_selectedMapItems.Remove(key);
		}

		public bool IsEmpty => _selectedMapItems?.Count == 0;
		public bool Validate() => _selectedMapItems != null;

		public void Reset()
		{
			_selectedMapItems.Clear();
		}
	}
}
