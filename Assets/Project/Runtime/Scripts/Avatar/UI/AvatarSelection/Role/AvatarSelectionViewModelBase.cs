/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionViewModelBase.cs
* Developer:	eugene9721
* Date:			2023-03-17 16:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;

namespace Com2Verse.UI
{
	public abstract class AvatarSelectionViewModelBase : ViewModelBase
	{
		protected AvatarSelectionManagerViewModel? Owner { get; private set; }

		public abstract void Show();
		public abstract void Hide();
		public abstract void Clear();

		public void Initialize(AvatarSelectionManagerViewModel owner)
		{
			Owner = owner;
		}

		public virtual void OnAvatarItemInfoChanged(AvatarInfo avatarItemInfo) { }
	}
}
