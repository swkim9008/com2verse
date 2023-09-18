/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObjectStatePublisher.cs
* Developer:	haminjeong
* Date:			2023-02-03 14:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Com2Verse.Extension;

namespace Com2Verse.Network
{
	public sealed class ActiveObjectStatePublisher : BaseObjectStatePublisher
	{
		protected override Protocols.GameMechanic.ObjectStateRequest GetStateFromObject()
		{
			var state = base.GetStateFromObject();
			var activeObject = MyObject as ActiveObject;
			if (activeObject.IsReferenceNull()) return state;
			state.CharacterState = activeObject.AnimatorController.CurrentCharacterState;
			// state.AnimatorId = activeObject.AnimatorID;
			// state.EmotionState = new Protocols.EmotionState()
			// {
			// 	EmotionId = activeObject.EmotionState,
			// };
			return state;
		}

		protected override Protocols.Changed IsEqualToCurrentState(ref Protocols.GameMechanic.ObjectStateRequest targetState)
		{
			var result = base.IsEqualToCurrentState(ref targetState);

			var activeObject = MyObject as ActiveObject;
			if (activeObject.IsReferenceNull()) return result;

			if (!CurrentState.CharacterState.Equals(targetState.CharacterState))
			{
				result |= Protocols.Changed.CharacterState;
			}
			
			return result;
		}
	}
}
