/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationEmployeeInfoViewModel.cs
* Developer:	jhkim
* Date:			2022-10-16 14:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Organization;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationEmployeeInfoViewModel : ViewModelBase
	{
#region Variables
		private string _name;
		private string _deptName;
		private bool _isShowDepartment;
		private string _jobInfo;
		private float _nameWidth;
		private float _preferredWidth;
		private bool _isSetWidth;
		private bool _isMine = false;

		private MemberIdType _memberId;
#endregion // Variables

#region Properties
		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				InvokePropertyValueChanged(nameof(Name), value);
			}
		}

		public string DeptName
		{
			get => _deptName;
			set
			{
				_deptName = value;
				InvokePropertyValueChanged(nameof(DeptName), value);
			}
		}

		public bool IsShowDepartment
		{
			get => _isShowDepartment;
			set
			{
				_isShowDepartment = value;
				InvokePropertyValueChanged(nameof(IsShowDepartment), value);
			}
		}
		public string JobInfo
		{
			get => _jobInfo;
			set
			{
				_jobInfo = value;
				InvokePropertyValueChanged(nameof(JobInfo), value);
			}
		}

		public bool IsMine
		{
			get => _isMine;
			set
			{
				_isMine = value;
				InvokePropertyValueChanged(nameof(IsMine), value);
			}
		}

		public float NameWidth
		{
			get
			{
				if (_nameWidth == 0) return -1;

				return _nameWidth;
			}
			set
			{
				/* (Min Width 세팅용) 조건
				 * 1. 유효한 값일 때 (width > 0)
				 * 2. 이전 값보다 클때만 세팅
				 */
				if (value == 0 || value <= _nameWidth)
					return;

				_nameWidth = value;
				InvokePropertyValueChanged(nameof(NameWidth), value);
			}
		}

		public float PreferredWidth
		{
			get => _preferredWidth == 0 ? -1 : _preferredWidth;
			set
			{
				_preferredWidth = value == 0 ? -1 : value;
				InvokePropertyValueChanged(nameof(PreferredWidth), PreferredWidth);
			}
		}
#endregion // Properties

#region Initialize
		public OrganizationEmployeeInfoViewModel() { }
		public OrganizationEmployeeInfoViewModel(MemberIdType memberId, bool showDepartment = true) => SetInfo(memberId, showDepartment);
#endregion // Initialize
		public void SetInfo(MemberIdType memberId, bool showDepartment)
		{
			_memberId = memberId;
			var memberModel = DataManager.Instance.GetMember(memberId);
			IsMine = memberModel.IsMine();

			SetVisibleDepartment(showDepartment);
			RefreshUI();
		}

		public void SetVisibleDepartment(bool visible) => IsShowDepartment = visible;
		void RefreshUI()
		{
			var memberModel = DataManager.Instance.GetMember(_memberId);
			if (memberModel == null) return;

			Name = memberModel.Member.MemberName;
			DeptName = memberModel.GetTeamStr();
			JobInfo = memberModel.GetPositionLevelTeamStr();
		}
	}
}
