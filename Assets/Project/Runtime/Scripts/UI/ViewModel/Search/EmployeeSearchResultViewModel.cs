/*===============================================================
* Product:		Com2Verse
* File Name:	EmployeeSearchResultViewModel.cs
* Developer:	jhkim
* Date:			2022-10-14 15:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Organization;
using UnityEngine;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	public class EmployeeSearchResultViewModel : BaseSearchResultViewModel, INestedViewModel
	{
#region Variables
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();
		private MemberIdType _memberId;
		private RectTransform _employeeDetailRectTransform;
#endregion // Variables

#region Properties
		public MemberIdType MemberId => _memberId;
		public bool ActiveOnClickProfile { get; set; } = false;
#endregion // Properties

#region Initialization
		public EmployeeSearchResultViewModel() { }

		public EmployeeSearchResultViewModel(MemberIdType employeeNo) : this()
		{
			SetEmployeeNo(employeeNo);
		}
		public EmployeeSearchResultViewModel(MemberIdType employeeNo, RectTransform employeeDetailRectTransform) : this()
		{
			_employeeDetailRectTransform = employeeDetailRectTransform;
			SetEmployeeNo(employeeNo);
		}
#endregion // Initialization

		public void SetEmployeeNo(MemberIdType employeeNo)
		{
			_memberId = employeeNo;

			NestedViewModels.Clear();
			NestedViewModels.Add(new OrganizationProfileIconViewModel(_memberId, OnClickProfile));
			NestedViewModels.Add(new OrganizationEmployeeInfoViewModel(_memberId));
		}
		private void OnClickProfile()
		{
			if (!ActiveOnClickProfile) return;
			OrganizationEmployeeDetailViewModel.SetModel(DataManager.Instance.GetMember(_memberId));
			OrganizationEmployeeDetailViewModel.ShowView(() =>
			{
				OrganizationEmployeeDetailViewModel.SetPosition(_employeeDetailRectTransform);
			});
		}
	}
}
