/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyViewModel.GroupInvite.cs
* Developer:	jhkim
* Date:			2022-10-06 16:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	// Group Invite
	public partial class OrganizationHierarchyViewModel
	{
#region Variables
		private GroupInviteModel _groupInviteModel;
		private const float GroupInviteWidthOffset = -110f; // 그룹초대 Width에서 스크롤 영역 + 여백을 제외하기 위한 상수

		private Collection<OrganizationGroupInviteSelectedRowViewModel> _groupInviteRows = new();
		private float _groupInviteViewWidth;
		private float _groupInviteViewHeight;
		private bool _isShowGroupInvite;
		private string _inviteButtonText;
		public CommandHandler CancelGroupInvite { get; private set; }
		public CommandHandler RequestGroupInvite { get; private set; }

		private ObjectPool<OrganizationGroupInviteSelectedRowViewModel> _rowPool;
#endregion // Variables

#region Properties
		public Collection<OrganizationGroupInviteSelectedRowViewModel> GroupInviteRows
		{
			get => _groupInviteRows;
			set
			{
				_groupInviteRows = value;
				InvokePropertyValueChanged(nameof(GroupInviteRows), value);
			}
		}

		public float GroupInviteViewWidth
		{
			get => _groupInviteViewWidth;
			set
			{
				_groupInviteViewWidth = value;
				UpdateGroupInviteRowsMaxWidth(_groupInviteViewWidth + GroupInviteWidthOffset);
				InvokePropertyValueChanged(nameof(GroupInviteViewWidth), value);
			}
		}

		public float GroupInviteViewHeight
		{
			get => _groupInviteViewHeight;
			set
			{
				_groupInviteViewHeight = value;
				InvokePropertyValueChanged(nameof(GroupInviteViewHeight), value);
			}
		}
		public bool IsShowGroupInvite
		{
			get => _isShowGroupInvite;
			set
			{
				_isShowGroupInvite = value;
				if (value == false)
					ClearGroupInvite();
				else
					RefreshGroupInviteText();
				InvokePropertyValueChanged(nameof(IsShowGroupInvite), IsShowGroupInvite);
			}
		}

		public string InviteButtonText
		{
			get => _inviteButtonText;
			set
			{
				_inviteButtonText = value;
				InvokePropertyValueChanged(nameof(InviteButtonText), value);
			}
		}
		private float MaxRowWidth => GroupInviteViewWidth + GroupInviteWidthOffset;
#endregion // Properties

#region View
#endregion // View

#region Initialize
		private void InitializeGroupInvite()
		{
			_groupInviteModel = GroupInviteModel.CreateNew();
			_rowPool = new ObjectPool<OrganizationGroupInviteSelectedRowViewModel>(CreateRowItem, GetRowItem, ReleaseRowItem, DestroyRowItem);
			RequestGroupInvite = new CommandHandler(OnRequestGroupInvite);
			CancelGroupInvite = new CommandHandler(OnCancelGroupInvite);

			RefreshGroupInviteUI();
		}
#endregion // Initialize

#region Binding Events
		private async void OnRequestGroupInvite()
		{
			if (_groupInviteModel.SelectedInfo.IsEmpty)
			{
				// TODO : 초대 할 사람이 없는 경우 처리
			}
			else
			{
				var memberIds = await CreateMemberIdsAsync();
				_onInvite?.Invoke(_popupType, memberIds);
				DisposeGroupInvite();
			}

			async UniTask<MemberIdType[]> CreateMemberIdsAsync()
			{
				var mySelf = await DataManager.Instance.GetMyselfAsync();
				var isValidMyself = mySelf != null;
				var count = isValidMyself ? _groupInviteModel.SelectedInfo.SelectedMap.Count + 1 : _groupInviteModel.SelectedInfo.SelectedMap.Count;
				var memberNos = new MemberIdType[count];
				var idx = 0;

				foreach (var (key, value) in _groupInviteModel.SelectedInfo.SelectedMap)
				{
					memberNos[idx] = value.Info.Member.AccountId;
					idx++;
				}

				if (isValidMyself)
					memberNos[idx] = mySelf.Member.AccountId;
				return memberNos;
			}
		}

		private void OnCancelGroupInvite() => DisposeGroupInvite();
#endregion // Binding Events

#region Refresh UI
		private void RefreshGroupInviteAfterAdd()
		{
			if (!IsShowGroupInvite)
				IsShowGroupInvite = true;
			RefreshGroupInviteUI();
		}

		private void RefreshGroupInviteAfterRemove()
		{
			RefreshGroupInviteUI();
			if (IsShowGroupInvite && _groupInviteModel.SelectedInfo.IsEmpty)
				IsShowGroupInvite = false;
		}

		private void RefreshGroupInviteUI()
		{
			ClearGroupInviteRows();
			FillGroupInviteRows();
		}

		private void RefreshGroupInviteText()
		{
			switch (_popupType)
			{
				case ePopupType.MEETING_RESERVATION:
				case ePopupType.INVITE_MEMBER:
					InviteButtonText = Localization.Instance.GetString(Constant.TextKey.MeetingInviteButtonText);
					break;
				case ePopupType.NEW_GROUP:
					InviteButtonText = Localization.Instance.GetString(Constant.TextKey.MakeGroup);
					break;
				default:
					InviteButtonText = Localization.Instance.GetString(Constant.TextKey.GroupInviteButtonText);
					break;
			}
		}
#endregion // Refresh UI

#region Add / Remove Row
		private void ClearGroupInviteRows()
		{
			var rows = GroupInviteRows.Value.ToArray();
			foreach (var row in rows)
			{
				row.CleanUp();
				_rowPool.Release(row);
			}

			GroupInviteRows.Reset();
		}

		private void FillGroupInviteRows()
		{
			OrganizationGroupInviteSelectedRowViewModel row = null;
			foreach (var info in _groupInviteModel.SelectedInfo.SelectedMap.Values)
			{
				var width = CharacterWidthCalculator.GetWidth(info.Info.Member.MemberName);
				if (row == null || !row.IsAvailableAdd(width))
					row = GetOrCreateRow(width);

				row.AddItem(info, width);
			}

			// SetSpaceToLastSibling = true;
			RefreshScrollViewHeight();
		}

		private OrganizationGroupInviteSelectedRowViewModel GetOrCreateRow(float itemWidth)
		{
			OrganizationGroupInviteSelectedRowViewModel result = null;
			for (int i = 0; i < GroupInviteRows.CollectionCount; ++i)
			{
				var row = GroupInviteRows.Value[i];
				if (row.IsAvailableAdd(itemWidth))
				{
					result = row;
					break;
				}
			}

			if (result == null)
				result = _rowPool.Get();

			return result;
		}

		private void UpdateGroupInviteRowsMaxWidth(float maxWidth)
		{
			foreach (var row in GroupInviteRows.Value)
				row.MaxWidth = maxWidth;
		}
#endregion // Add / Remove Row

#region Add / Remove Item
		private void ClearGroupInvite()
		{
			foreach (var employee in _groupInviteModel.SelectedInfo.SelectedMap.Values)
				SetCheckEmployee(employee, false);

			_groupInviteModel.Reset();
			RefreshScrollViewHeight();
		}

		private void OnRemoveGroupInviteItem(CheckMemberListModel info)
		{
			RemoveGroupInvite(info);
			RemoveCheckedItem(info);

			if (_prevSelected != null)
				RefreshEmployeeCheckUI(_prevSelected.Value.ID);

			RefreshSearchEmployeeCheckUI(info.Info.Member.AccountId, false);
		}

		private void AddGroupInviteEmployee(IEnumerable<CheckMemberListModel> employees)
		{
			var isDirty = false;
			foreach (var employee in employees)
			{
				if (!IsInviteAvailable(employee)) continue;
				if (_groupInviteModel.SelectedInfo.Contains(employee)) continue;

				_groupInviteModel.SelectedInfo.AddItem(employee);
				isDirty = true;
			}

			if (isDirty)
				RefreshGroupInviteAfterAdd();
		}

		private void RemoveGroupInviteEmployee(IEnumerable<CheckMemberListModel> employees)
		{
			var isDirty = false;
			foreach (var employee in employees)
			{
				if (_groupInviteModel.SelectedInfo.Contains(employee))
				{
					if (!IsInviteAvailable(employee)) continue;

					_groupInviteModel.SelectedInfo.RemoveItem(employee);
					isDirty = true;
				}
			}

			if (isDirty)
				RefreshGroupInviteAfterRemove();
		}
#endregion // Add / Remove Item

#region Calculate CharacterWidth
		class CharacterWidthCalculator
		{
			private enum eCharType
			{
				UNDEFINED,
				NUMBER,
				KOREAN,
				ALPHABET,
				ALPHABET_CAPITAL,
			}
			// UI 변경시 값 수정 필요
			private const float BaseWidth = 45f;
			private const float DefaultCharacterWidth = 13f;

			private struct AlphabetThickness
			{
				public static readonly float VeryThin = 4.5f;
				public static readonly float Thin = 6f;
				public static readonly float Medium = 7.6f;
				public static readonly float Thick = 9.8f;
				public static readonly float VeryThick = 11.8f;
			}

			private struct AlphabetCapitalThickness
			{
				public static readonly float Thin = 4f;
				public static readonly float Medium = 7.2f;
				public static readonly float Thick = 8.5f;
				public static readonly float VeryThick = 9.5f;
			}

			private static Dictionary<eCharType, CharacterWidthInfo[]> _charWidthMap = new Dictionary<eCharType, CharacterWidthInfo[]>
			{
				{
					eCharType.NUMBER,
					new CharacterWidthInfo[]
					{
						new() {RangeBegin = '0', RangeEnd = '9', Width = 6.6f}, // 13.68f
					}
				},
				{
					eCharType.KOREAN,
					new CharacterWidthInfo[]
					{
						new() {RangeBegin = '가', RangeEnd = '힣', Width = 11f}, // 16.5f
					}
				},
				{
					eCharType.ALPHABET,
					new CharacterWidthInfo[]
					{
						new() {RangeBegin = 'a', RangeEnd = 'e', Width = AlphabetThickness.Medium},
						new() {RangeBegin = 'f', RangeEnd = 'f', Width = AlphabetThickness.VeryThin},
						new() {RangeBegin = 'g', RangeEnd = 'h', Width = AlphabetThickness.Medium},
						new() {RangeBegin = 'i', RangeEnd = 'j', Width = AlphabetThickness.VeryThin},
						new() {RangeBegin = 'k', RangeEnd = 'k', Width = AlphabetThickness.Medium},
						new() {RangeBegin = 'l', RangeEnd = 'l', Width = AlphabetThickness.VeryThin},
						new() {RangeBegin = 'm', RangeEnd = 'm', Width = AlphabetThickness.VeryThick},
						new() {RangeBegin = 'n', RangeEnd = 'q', Width = AlphabetThickness.Medium},
						new() {RangeBegin = 'r', RangeEnd = 't', Width = AlphabetThickness.VeryThin},
						new() {RangeBegin = 'u', RangeEnd = 'v', Width = AlphabetThickness.Medium},
						new() {RangeBegin = 'w', RangeEnd = 'w', Width = AlphabetThickness.Thick},
						new() {RangeBegin = 'x', RangeEnd = 'z', Width = AlphabetThickness.Thin},
					}
				},
				{
					eCharType.ALPHABET_CAPITAL,
					new CharacterWidthInfo[]
					{
						new() {RangeBegin = 'A', RangeEnd = 'C', Width = AlphabetCapitalThickness.Thick},
						new() {RangeBegin = 'D', RangeEnd = 'D', Width = AlphabetCapitalThickness.VeryThick},
						new() {RangeBegin = 'E', RangeEnd = 'E', Width = AlphabetCapitalThickness.Thick},
						new() {RangeBegin = 'F', RangeEnd = 'F', Width = AlphabetCapitalThickness.Medium},
						new() {RangeBegin = 'G', RangeEnd = 'H', Width = AlphabetCapitalThickness.VeryThick},
						new() {RangeBegin = 'I', RangeEnd = 'I', Width = AlphabetCapitalThickness.Thin},
						new() {RangeBegin = 'J', RangeEnd = 'L', Width = AlphabetCapitalThickness.Medium},
						new() {RangeBegin = 'M', RangeEnd = 'O', Width = AlphabetCapitalThickness.VeryThick},
						new() {RangeBegin = 'P', RangeEnd = 'P', Width = AlphabetCapitalThickness.Thick},
						new() {RangeBegin = 'Q', RangeEnd = 'Q', Width = AlphabetCapitalThickness.VeryThick},
						new() {RangeBegin = 'R', RangeEnd = 'R', Width = AlphabetCapitalThickness.Thick},
						new() {RangeBegin = 'S', RangeEnd = 'U', Width = AlphabetCapitalThickness.Thick},
						new() {RangeBegin = 'V', RangeEnd = 'V', Width = AlphabetCapitalThickness.Medium},
						new() {RangeBegin = 'W', RangeEnd = 'W', Width = AlphabetCapitalThickness.VeryThick},
						new() {RangeBegin = 'X', RangeEnd = 'Z', Width = AlphabetCapitalThickness.Medium},
					}
				}
			};
			private struct CharacterWidthInfo
			{
				public char RangeBegin;
				public char RangeEnd;
				public float Width;
			}

			public static float GetWidth(string text)
			{
				var width = BaseWidth;
				foreach (var c in text)
					width += GetCharacterWidth(c);
				return width;
			}

			private static float GetCharacterWidth(char c)
			{
				var type = GetCharType(c);
				if (type == eCharType.UNDEFINED) return DefaultCharacterWidth;

				if (!_charWidthMap.TryGetValue(type, out var charWidthMap))
					return DefaultCharacterWidth;

				foreach (var info in charWidthMap)
				{
					if (c >= info.RangeBegin && c <= info.RangeEnd)
						return info.Width;
				}
				return DefaultCharacterWidth;
			}

			private static eCharType GetCharType(char c) => c switch
			{
				>= '0' and <= '9' => eCharType.NUMBER,
				>= '가' and <= '힣' => eCharType.KOREAN,
				>= 'a' and <= 'z' => eCharType.ALPHABET,
				>= 'A' and <= 'Z' => eCharType.ALPHABET_CAPITAL,
				_ => eCharType.UNDEFINED,
			};
		}
#endregion // Calculate CharacterWidth

#region Pooling
		OrganizationGroupInviteSelectedRowViewModel CreateRowItem() => new(MaxRowWidth, OnRemoveGroupInviteItem);
		void GetRowItem(OrganizationGroupInviteSelectedRowViewModel item) => GroupInviteRows.AddItem(item);
		void ReleaseRowItem(OrganizationGroupInviteSelectedRowViewModel item)
		{
			if (GroupInviteRows.Value.Contains(item))
				GroupInviteRows.RemoveItem(item);
		}

		void DestroyRowItem(OrganizationGroupInviteSelectedRowViewModel item)
		{
			if(GroupInviteRows.Value.Contains(item))
				GroupInviteRows.RemoveItem(item);
		}
#endregion // Pooling

		private void DisposeGroupInvite()
		{
			IsShowGroupInvite = false;
			var rows = GroupInviteRows.Value.ToArray();
			foreach (var row in rows)
			{
				foreach (var item in row.Selected.Value)
					RemoveCheckedItem(item.Id);
				row.Dispose();
				_rowPool.Release(row);
			}
			_rowPool.Dispose();

			if (_prevSelected != null)
				RefreshEmployeeCheckUI(_prevSelected.Value.ID);

			SetSearchEmployeeAllCheck(false);
		}
	}
}
