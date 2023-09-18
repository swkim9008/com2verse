/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationGroupInviteSelectedItemViewModel.cs
* Developer:	jhkim
* Date:			2022-07-29 11:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationGroupInviteSelectedItemViewModel : ViewModelBase
	{
#region Variables
		private string _name;
		private float _width;
		public CommandHandler Remove { get; }

		private CheckMemberListModel _model;
		private Action<CheckMemberListModel> _onRemove;
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

		public float Width
		{
			get => _width;
			set
			{
				_width = value;
				InvokePropertyValueChanged(nameof(Width), value);
			}
		}

		public MemberIdType Id
		{
			get => _model.Info.Member.AccountId;
			set => InvokePropertyValueChanged(nameof(Id), value);
		}
#endregion // Properties

#region Initialize
		public OrganizationGroupInviteSelectedItemViewModel() { }

		public OrganizationGroupInviteSelectedItemViewModel(Action<CheckMemberListModel> onRemove)
		{
			_onRemove = onRemove;
			Remove = new CommandHandler(OnRemove);
		}
#endregion // Initialize

#region Binding Events
		private void OnRemove() => _onRemove?.Invoke(_model);
#endregion // Binding Events

		public void SetModel(CheckMemberListModel model)
		{
			_model = model;
			Name = _model.Info.Member.MemberName;
		}
	}
}
