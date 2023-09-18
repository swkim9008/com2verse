/*===============================================================
* Product:		Com2Verse
* File Name:	IAvatarClosetController.cs
* Developer:	eugene9721
* Date:			2023-05-17 11:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Ambience.Runtime.DitherOverride;
using Com2Verse.Avatar.UI;
using Com2Verse.CameraSystem;
using Com2Verse.Communication.Unity;

namespace Com2Verse.Avatar
{
	public interface IAvatarClosetController
	{
		public static class AvatarClosetSetting
		{
			private static bool  _prevDitherValue;
			private static bool  _prevHumanMattingValue;

			public static void SetSetting()
			{
				_prevDitherValue = DitherOverride.Get();
				DitherOverride.Set(false);

				_prevHumanMattingValue = ModuleManager.Instance.HumanMattingTexturePipeline.IsRunning;

				ModuleManager.Instance.HumanMattingTexturePipeline.IsRunning = false;
				CameraManager.InstanceOrNull?.SetCameraBlendTime(0);
			}

			public static void ResetSetting()
			{
				if (ModuleManager.InstanceExists)
					ModuleManager.Instance.HumanMattingTexturePipeline.IsRunning = _prevHumanMattingValue;
				DitherOverride.Set(_prevDitherValue);
				CameraManager.InstanceOrNull?.SetDefaultBlendTime();
			}
		}

#region TextKey
		public string TitleTextKey { get; }

		public string DisableToastTextKey { get; }

		public string ButtonTextKey(eViewType viewType);
#endregion TextKey

#region Properties
		public eViewType StartView { get; }

		public bool NeedAvatarCameraFrame { get; }

		public bool IsOnUndoButton { get; }

		public bool IsUseAdditionalInfoAtItem { get; }

		public AvatarJigController AvatarJigController { get; }

		public float RotateMarkerHideTime { get; }
#endregion Properties

		public void ChangeTypeTap(eViewType prevType, eViewType nextType, Action typeChangeAction);

		public void OnCreatedViewModel();

		public void OnClear();

		public void SetFaceVirtualCamera();

		public void SetFullBodyVirtualCamera();

		public void UpdateCustomize(eViewType type);

		public AvatarInfo GetAvatarInfo();

		public void OnClickBackButton(eViewType type);


		public virtual void OnCreatedAvatar(AvatarController avatarController) { }
	}
}
