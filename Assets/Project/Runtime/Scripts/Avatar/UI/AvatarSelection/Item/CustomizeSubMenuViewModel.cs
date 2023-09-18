/*===============================================================
* Product:		Com2Verse
* File Name:	CustomizeSubMenuViewModel.cs
* Developer:	eugene9721
* Date:			2023-05-02 17:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class CustomizeSubMenuViewModel : ViewModelBase
	{
		private const string FaceDetailsKey    = "UI_AvatarCreate_Face_SubMenu_FaceDetails";
		private const string EyeDetailsKey     = "UI_AvatarCreate_Face_SubMenu_Eye";
		private const string EyeBrowDetailsKey = "UI_AvatarCreate_Face_SubMenu_EyeBrow";
		private const string NoseDetailsKey    = "UI_AvatarCreate_Face_SubMenu_Nose";
		private const string MouthDetailsKey   = "UI_AvatarCreate_Face_SubMenu_Mouth";
		private const string EyeDecoDetailsKey = "UI_AvatarCreate_MakeUp_SubMenu_EyeDeco";
		private const string CheekDetailsKey   = "UI_AvatarCreate_MakeUp_SubMenu_Cheek";
		private const string LipDetailsKey     = "UI_AvatarCreate_MakeUp_SubMenu_Lip";
		private const string TattooDetailsKey  = "UI_AvatarCreate_MakeUp_SubMenu_Tattoo";

		private const string HatDetailsKey     = "UI_AvatarCreate_Accessaries_SubMenu_Hat";
		private const string GlassesDetailsKey = "UI_AvatarCreate_Accessaries_SubMenu_Glasses";
		private const string BagDetailsKey     = "UI_AvatarCreate_Accessaries_SubMenu_Bag";

#region Fields
		private bool _isFaceSubMenu;

		private eFaceSubMenu    _faceSubMenu;
		private eFashionSubMenu _fashionSubMenu;

		private Action<CustomizeSubMenuViewModel> _onSelectedEvent;

		private bool _isSelected;
#endregion Fields

#region Field Properties
		[UsedImplicitly]
		public eFaceSubMenu FaceSubMenu
		{
			get => _faceSubMenu;
			set
			{
				_isFaceSubMenu = true;
				SetProperty(ref _faceSubMenu, value);
				InvokePropertyValueChanged(nameof(SubMenuName), SubMenuName);
			}
		}

		[UsedImplicitly]
		public eFashionSubMenu FashionSubMenu
		{
			get => _fashionSubMenu;
			set
			{
				_isFaceSubMenu = false;
				SetProperty(ref _fashionSubMenu, value);
				InvokePropertyValueChanged(nameof(SubMenuName), SubMenuName);
			}
		}

		public string SubMenuName => _isFaceSubMenu ? GetFaceString() : GetFashionString();

		public event Action<CustomizeSubMenuViewModel> OnSelectedEvent
		{
			add
			{
				_onSelectedEvent -= value;
				_onSelectedEvent += value;
			}
			remove => _onSelectedEvent -= value;
		}

		[UsedImplicitly]
		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				SetProperty(ref _isSelected, value);
				InvokePropertyValueChanged(nameof(IsDeselected), IsDeselected);
			}
		}

		[UsedImplicitly] public bool IsDeselected => !IsSelected;
#endregion Field Properties

#region Command Properties
		[UsedImplicitly] public CommandHandler SubMenuClicked { get; }
#endregion Command Properties

		public CustomizeSubMenuViewModel()
		{
			SubMenuClicked = new CommandHandler(OnSubMenuClicked);
		}

		private void OnSubMenuClicked()
		{
			_onSelectedEvent?.Invoke(this);
		}

		private string GetFaceString()
		{
			switch (_faceSubMenu)
			{
				case eFaceSubMenu.FACE_DETAILS:
					return Localization.Instance.GetString(FaceDetailsKey);
				case eFaceSubMenu.EYE:
					return Localization.Instance.GetString(EyeDetailsKey);
				case eFaceSubMenu.EYE_BROW:
					return Localization.Instance.GetString(EyeBrowDetailsKey);
				case eFaceSubMenu.NOSE:
					return Localization.Instance.GetString(NoseDetailsKey);
				case eFaceSubMenu.MOUTH:
					return Localization.Instance.GetString(MouthDetailsKey);
				case eFaceSubMenu.EYE_DECO:
					return Localization.Instance.GetString(EyeDecoDetailsKey);
				case eFaceSubMenu.CHEEK:
					return Localization.Instance.GetString(CheekDetailsKey);
				case eFaceSubMenu.LIP:
					return Localization.Instance.GetString(LipDetailsKey);
				case eFaceSubMenu.TATTOO:
					return Localization.Instance.GetString(TattooDetailsKey);
			}

			return string.Empty;
		}

		private string GetFashionString()
		{
			switch (_fashionSubMenu)
			{
				case eFashionSubMenu.HAT:
					return Localization.Instance.GetString(HatDetailsKey);
				case eFashionSubMenu.GLASSES:
					return Localization.Instance.GetString(GlassesDetailsKey);
				case eFashionSubMenu.BAG:
					return Localization.Instance.GetString(BagDetailsKey);
			}

			return string.Empty;
		}
	}
}
