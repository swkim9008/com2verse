/*===============================================================
* Product:		Com2Verse
* File Name:	UserCharacterStateViewModel.cs
* Developer:	eugene9721
* Date:			2022-10-14 18:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Network;
using JetBrains.Annotations;
using Protocols;

namespace Com2Verse.UI
{
	public sealed class UserCharacterStateViewModel : ViewModelBase
	{
		private CharacterState _characterState;

#region ViewModelProperties
		[UsedImplicitly] public bool UserCharacterStateNone        => GetState(CharacterState.None);
		[UsedImplicitly] public bool UserCharacterStateIdleWalkRun => GetState(CharacterState.IdleWalkRun);
		[UsedImplicitly] public bool UserCharacterStateJumpStart   => GetState(CharacterState.JumpStart);
		[UsedImplicitly] public bool UserCharacterStateInAir       => GetState(CharacterState.InAir);
		[UsedImplicitly] public bool UserCharacterStateJumpLand    => GetState(CharacterState.JumpLand);
		[UsedImplicitly] public bool UserCharacterStateSit         => GetState(CharacterState.Sit);
#endregion // ViewModelProperties

		private bool GetState(CharacterState characterState) => _characterState == characterState;

		public void SetCharacterState(CharacterState characterState)
		{
			_characterState = characterState;
			RefreshViewModel();
		}

		private void RefreshViewModel()
		{
			InvokePropertyValueChanged(nameof(UserCharacterStateNone),        UserCharacterStateNone);
			InvokePropertyValueChanged(nameof(UserCharacterStateIdleWalkRun), UserCharacterStateIdleWalkRun);
			InvokePropertyValueChanged(nameof(UserCharacterStateJumpStart),   UserCharacterStateJumpStart);
			InvokePropertyValueChanged(nameof(UserCharacterStateInAir),       UserCharacterStateInAir);
			InvokePropertyValueChanged(nameof(UserCharacterStateJumpLand),    UserCharacterStateJumpLand);
			InvokePropertyValueChanged(nameof(UserCharacterStateSit),         UserCharacterStateSit);
		}
	}
}
