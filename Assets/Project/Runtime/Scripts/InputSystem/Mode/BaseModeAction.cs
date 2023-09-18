/*===============================================================
* Product:		Com2Verse
* File Name:	BaseModeAction.cs
* Developer:	haminjeong
* Date:			2023-05-25 18:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;

namespace Com2Verse.Project.InputSystem
{
	[AttributeUsage(AttributeTargets.Class)]
	public class InputModeAttribute : Attribute
	{
		public eModeType InputModeType { get; }

		public InputModeAttribute(eModeType modeType)
		{
			InputModeType = modeType;
		}
	}
	
	public abstract class BaseModeAction
	{
		private static readonly int DefinitionMultiply = 10000;

		protected eInputSystemState InputState;
		protected eCameraState      CameraState;
		protected int               AnimatorId;

		public abstract eModeType CurrentMode { get; }
		
		public abstract void Initialize();

		public virtual void RestoreSetting() { }

		public virtual void ApplyMode(Action onCameraChangeComplete = null)
		{
			CameraManager.Instance.ChangeState(CameraState, onComplete: onCameraChangeComplete);
			InputSystemManagerHelper.ChangeState(InputState);
			Commander.Instance.SetAnimatorID(GetMappedAnimatorID(User.Instance.CharacterObject));
		}

		protected int GetMappedAnimatorID(MapObject targetObject)
		{
			if (targetObject.IsUnityNull()) return -1;
			eAvatarType avatarType = (eAvatarType)targetObject.AvatarType;
			return (int)avatarType * DefinitionMultiply + AnimatorId;
		}
	}
}
