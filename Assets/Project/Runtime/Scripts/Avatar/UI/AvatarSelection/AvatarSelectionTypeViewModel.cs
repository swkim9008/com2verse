/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionTypeViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-17 16:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class AvatarSelectionTypeViewModel : AvatarSelectionViewModelBase
	{
#region Fields
		private bool _isWomanSelected;
		private bool _isManSelected;
		private bool _isSelectedGender;
#endregion Fields

#region Command Properties
		[UsedImplicitly] public CommandHandler WomanButtonClick              { get; private set; }
		[UsedImplicitly] public CommandHandler ManButtonClick                { get; private set; }
		[UsedImplicitly] public CommandHandler AvatarGenderSelectButtonClick { get; private set; }
#endregion Command Properties

#region Field Properties
		[UsedImplicitly]
		public bool IsWomanSelected
		{
			get => _isWomanSelected;
			set
			{
				if (value) IsManSelected = false;
				SetProperty(ref _isWomanSelected, value);
				InvokePropertyValueChanged(nameof(IsSelectedGender),   IsSelectedGender);
				InvokePropertyValueChanged(nameof(IsNotWomanSelected), IsNotWomanSelected);
			}
		}

		[UsedImplicitly]
		public bool IsNotWomanSelected => !IsWomanSelected;

		[UsedImplicitly]
		public bool IsManSelected
		{
			get => _isManSelected;
			set
			{
				if (value) IsWomanSelected = false;
				SetProperty(ref _isManSelected, value);
				InvokePropertyValueChanged(nameof(IsSelectedGender), IsSelectedGender);
				InvokePropertyValueChanged(nameof(IsNotManSelected), IsNotManSelected);
			}
		}

		[UsedImplicitly]
		public bool IsNotManSelected => !IsManSelected;

		[UsedImplicitly]
		public bool IsSelectedGender => IsManSelected || IsWomanSelected;
#endregion Field Properties

#region AvatarSelectionViewModelBase
		public override void Show()
		{
			if (Owner != null)
			{
				Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.TypeSubTitleTextKey;
				Owner.AvatarCloset.Controller?.SetFullBodyVirtualCamera();
			}
		}
		public override void Hide()  { }

		public override void Clear()
		{
			IsWomanSelected  = false;
			IsManSelected    = false;
		}
#endregion AvatarSelectionViewModelBase

#region Initialize
		public AvatarSelectionTypeViewModel()
		{
			WomanButtonClick              = new CommandHandler(OnWomanButtonClick);
			ManButtonClick                = new CommandHandler(OnManButtonClick);
			AvatarGenderSelectButtonClick = new CommandHandler(OnAvatarGenderSelectButtonClick);
		}
#endregion Initialize

#region Handlers
		private void OnWomanButtonClick()
		{
			IsWomanSelected = true;

			AvatarMediator.Instance.AvatarCloset.AvatarEditData.AvatarType = eAvatarType.PC01_W;
			Owner?.RefreshButtons();
		}

		private void OnManButtonClick()
		{
			IsManSelected = true;

			AvatarMediator.Instance.AvatarCloset.AvatarEditData.AvatarType = eAvatarType.PC01_M;
			Owner?.RefreshButtons();
		}

		private void OnAvatarGenderSelectButtonClick()
		{
			if (!IsSelectedGender) return;

			Owner?.SetNextType();
		}

		public void OnCharacterClicked(eAvatarType type)
		{
			switch (type)
			{
				case eAvatarType.PC01_W:
					OnWomanButtonClick();
					break;
				case eAvatarType.PC01_M:
					OnManButtonClick();
					break;
			}
		}
#endregion Handlers
	}
}
