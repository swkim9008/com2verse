/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyViewModel.cs
* Developer:	jhkim
* Date:			2022-07-19 11:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.Organization;
using UnityEngine;
using EmployeeList = System.Collections.Generic.IEnumerable<int>;
using OnInviteEvent = System.Action<Com2Verse.UI.OrganizationHierarchyViewModel.ePopupType, System.Collections.Generic.IEnumerable<long>>;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public partial class OrganizationHierarchyViewModel : OrganizationBaseViewModel
	{
		public enum ePopupType
		{
			NONE,
			NEW_GROUP,
			INVITE_MEMBER,
			MEETING_RESERVATION,
		}
#region Variables
		private StackRegisterer _organizationGUIViewRegister;

		public StackRegisterer OrganizationGUIViewRegister
		{
			get => _organizationGUIViewRegister;
			set
			{
				_organizationGUIViewRegister             =  value;
				_organizationGUIViewRegister.WantsToQuit += OnClose;
			}
		}
		private static readonly string ResName = "UI_Organization_HierarchyPopup";
		private static OrganizationHierarchyViewModel _viewModel;
		private Vector3 _popupPosition = Constant.DefaultPopupPosition;
		private float _scrollY;
		private bool _enableHorizontalScroll;
		private float _scrollViewWidth;
		private float _initialScrollViewWidth;
		public CommandHandler Close { get; }

		private OnInviteEvent _onInvite;
		private ePopupType _popupType;
#endregion // Variables

#region Properties
		public Vector3 PopupPosition
		{
			get => _popupPosition;
			set
			{
				_popupPosition = value;
				InvokePropertyValueChanged(nameof(PopupPosition), value);
			}
		}

		public float ScrollY
		{
			get => _scrollY;
			set
			{
				_scrollY = value;
				InvokePropertyValueChanged(nameof(ScrollY), value);
			}
		}

		public bool EnableHorizontalScroll
		{
			get => _enableHorizontalScroll;
			set
			{
				_enableHorizontalScroll = value;
				InvokePropertyValueChanged(nameof(EnableHorizontalScroll), value);
			}
		}

		public float ScrollViewWidth
		{
			get => _scrollViewWidth;
			set
			{
				_scrollViewWidth = value;
				EnableHorizontalScroll = _scrollViewWidth - _initialScrollViewWidth > 0;
				InvokePropertyValueChanged(nameof(ScrollViewWidth), value);
			}
		}

		public float InitialScrollViewWidth
		{
			get => _initialScrollViewWidth;
			set
			{
				_initialScrollViewWidth = value;
				InvokePropertyValueChanged(nameof(InitialScrollViewWidth), value);
			}
		}
#endregion // Properties

#region View
		public static void ShowView(ePopupType type, HierarchyViewInfo info, OnInviteEvent onInvite = null)
		{
			HierarchyViewInfo.CachedInfo = info;

			ShowView(ResName, OnLoaded, OnLoadedEnd);

			void OnLoaded()
			{
				if (_viewModel == null) return;

				_viewModel._popupType = type;
				SetOnInviteEvent(onInvite);
				_viewModel.SetInfo(HierarchyViewInfo.CachedInfo);
			}

			void OnLoadedEnd()
			{
				if (_viewModel == null) return;

				_viewModel.PopupPosition = Constant.DefaultPopupPosition;

				_viewModel?.PickItem(0, 0);
				_viewModel?.RefreshHierarchyTreeData();
				_viewModel?.SelectTreeItem(0, 0);
				_viewModel?.SetScrollToTop();
			}
		}

		public static void ShowView(ePopupType type, long accountId, HierarchyViewInfo info, OnInviteEvent onInvite = null)
		{
			HierarchyViewInfo.CachedInfo = info;

			ShowView(ResName, OnLoaded, OnLoadedEnd);

			void OnLoaded()
			{
				if (_viewModel == null) return;

				_viewModel._popupType = type;
				SetOnInviteEvent(onInvite);
				_viewModel.SetInfo(HierarchyViewInfo.CachedInfo);
			}

			async void OnLoadedEnd()
			{
				if (_viewModel == null) return;

				_viewModel.PopupPosition = Constant.DefaultPopupPosition;

				var pickInfo = await _viewModel.PickItemByAccountIdAsync(accountId);
				_viewModel?.RefreshHierarchyTreeData();
				if (pickInfo.Item1 != -1 && pickInfo.Item2 != -1)
					_viewModel?.SelectTreeItem(pickInfo.Item1, pickInfo.Item2);
				_viewModel?.SetScrollToTop();
			}
		}
		public static void HideView()
		{
			if (_viewModel is {IsShow: true})
			{
				_viewModel.ClearSearchText();
				_viewModel.Dispose();
			}
			HideView(ResName);
		}
#endregion // View

#region Initialize
		public OrganizationHierarchyViewModel() : base(ResName)
		{
			_viewModel = this;

			Close = new CommandHandler(OnClose, null);

			Initialize();
		}

		private void Initialize()
		{
			InitEmployee();
			InitializeGroupInvite();
			InitTree();
			InitSearchField();
			InitTest();
		}

		private static void SetOnInviteEvent(OnInviteEvent onInvite) => _viewModel._onInvite = onInvite;
		private static void ResetViewModel()
		{
			_viewModel?.ResetEmployee();
		}

		private void SetInfo(HierarchyViewInfo info)
		{
			_checkedMembers.Clear();
			if (info?.CheckedMembers != null)
			{
				foreach (var employeeNo in info.CheckedMembers)
					_checkedMembers.TryAdd(employeeNo);
			}

			_blockedMembers.Clear();
			if (info?.BlockedMembers != null)
			{
				foreach (var employeeNo in info.BlockedMembers)
					_blockedMembers.TryAdd(employeeNo);
			}

			InitGroupInviteModel(info);
		}

		private void InitGroupInviteModel(HierarchyViewInfo info)
		{
			if (info?.CheckedMembers == null) return;

			var employeeNos = info?.CheckedMembers.ToList();
			if (info?.BlockedMembers != null)
			{
				foreach (var blocked in info.BlockedMembers)
				{
					if (employeeNos.Contains(blocked))
						employeeNos.Remove(blocked);
				}
			}

			if (!employeeNos.Any()) return;

			_groupInviteModel.Reset();

			var inviteEmployees = new CheckMemberListModel[employeeNos.Count];
			for (var i = 0; i < employeeNos.Count; i++)
			{
				var memberId = employeeNos[i];
				var model = new CheckMemberListModel
				{
					Info = DataManager.Instance.GetMember(memberId),
					IsChecked = true,
				};
				inviteEmployees[i] = model;
			}

			AddGroupInviteEmployee(inviteEmployees);
			RefreshGroupInviteUI();
		}

		private void SetScrollToTop() => ScrollY = 1f;
#endregion // Initialize

#region Binding Events
		private void OnClose()
		{
			OrganizationEmployeeDetailViewModel.HideView();
			HideView();
		}
#endregion // Binding Events

#region UI
		private void RefreshUI() => RefreshHierarchyTreeData();
#endregion // UI

#region Cheat
		public int HierarchyItemCount => HierarchyList?.CollectionCount ?? 0;
		public void Pick(int idx) => PickItem(idx);
#endregion // Cheat
		private void Dispose()
		{
			// TODO : 재사용스크롤로 변경 후 해제 처리 수정
			DisposeTree();
			DisposeEmployee();
			DisposeGroupInvite();
		}

#region Data
		public class HierarchyViewInfo
		{
			public static HierarchyViewInfo CachedInfo { get; set; }

			public MemberIdType[] CheckedMembers;
			public MemberIdType[] BlockedMembers;

			public static HierarchyViewInfo Create(IEnumerable<MemberIdType> @checked, IEnumerable<MemberIdType> blocked) =>
				new HierarchyViewInfo
				{
					CheckedMembers = @checked?.ToArray(),
					BlockedMembers = blocked?.ToArray(),
				};

			public static HierarchyViewInfo Empty => new HierarchyViewInfo
			{
				CheckedMembers = Array.Empty<MemberIdType>(),
				BlockedMembers = Array.Empty<MemberIdType>(),
			};

			public static HierarchyViewInfo MySelf
			{
				get
				{
					var employeeId = (User.Instance.CurrentUserData as OfficeUserData).EmployeeID;
					if (MemberIdType.TryParse(employeeId, out var id))
					{
						return new HierarchyViewInfo
						{
							CheckedMembers = new[] {id},
							BlockedMembers = new[] {id},
						};
					}

					return new HierarchyViewInfo
					{
						CheckedMembers = Array.Empty<MemberIdType>(),
						BlockedMembers = Array.Empty<MemberIdType>(),
					};
				}
			}
		}
#endregion // Data
	}
}
