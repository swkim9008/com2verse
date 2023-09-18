/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarItemSelectAction.cs
* Developer:	eugene9721
* Date:			2023-05-11 16:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Cysharp.Threading.Tasks;

namespace Com2Verse
{
	public sealed class AvatarItemSelectAction : ReversibleAction
	{
		private AvatarInfo PrevInfo    { get; }
		private AvatarInfo CurrentInfo { get; }

		public AvatarItemSelectAction(AvatarInfo prevInfo, AvatarInfo newInfo)
		{
			PrevInfo    = prevInfo.Clone();
			CurrentInfo = newInfo.Clone();
		}

		public override void Do(bool isInitial)
		{
			base.Do(isInitial);
			AvatarMediator.Instance.AvatarCloset.UpdateAvatarModel(CurrentInfo).Forget();
		}

		public override void Undo()
		{
			base.Undo();
			AvatarMediator.Instance.AvatarCloset.UpdateAvatarModel(PrevInfo).Forget();
		}
	}
}
