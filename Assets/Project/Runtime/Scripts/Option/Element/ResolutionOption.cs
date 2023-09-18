/*===============================================================
* Product:		Com2Verse
* File Name:	ResolutionOption.cs
* Developer:	mikeyid77
* Date:			2023-06-26 12:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.PlatformControl;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.Option
{
	[Serializable] [MetaverseOption("ResolutionOption")]
	public sealed class ResolutionOption : BaseMetaverseOption
	{
		[NonSerialized] private bool _resetTrigger = false;
		[NonSerialized]  private int _beforeResolution = -1;
		[SerializeField] private int _screenModeIndex;
		[SerializeField] private int _resolutionIndex = -1;
		
		private Action<int> _cancelAction = null;
		public Action<int> CancelEvent
		{
			get => _cancelAction;
			set => _cancelAction = value;
		}

		public bool IsBuild
		{
			get
			{
#if UNITY_EDITOR
				return false;
#else
                return true;
#endif
			}
			set => C2VDebug.LogWarningCategory("GraphicOption", "Can't set BuildType");
		}
		
		public int ScreenModeIndex
		{
			get => _screenModeIndex;
			set
			{
				if (_screenModeIndex == value) return;
				
				_screenModeIndex = value;
				if (IsBuild)
				{
					PlatformController.Instance.SetScreenMode(value == 0);
					base.SaveData();
				}
			}
		}

		public int ResolutionIndex
		{
			get => _resolutionIndex;
			set
			{
				if (_resolutionIndex == value) return;
				if (_resetTrigger)
				{
					_resetTrigger = false;
					_resolutionIndex = value;
					if (IsBuild && _screenModeIndex != 0)
					{
						PlatformController.Instance.SetScreenResolution(value);
						base.SaveData();
					}
				}
				else
				{
					_beforeResolution = _resolutionIndex;
					_resolutionIndex = value;
					if (IsBuild && _screenModeIndex != 0)
					{
						PlatformController.Instance.SetScreenResolution(value);
						base.SaveData();
						UIManager.Instance.ShowPopupScreenResolution(() =>
						{
							_resetTrigger = true;
							_cancelAction?.Invoke(_beforeResolution);
						});
					}
					else if (_screenModeIndex == 0)
					{
						_resolutionIndex = 0;
						PlatformController.Instance.SetScreenResolution(-1);
						base.SaveData();
					}
				}
			}
		}

		public override void Apply()
		{
			base.Apply();
			PlatformController.Instance.SetScreenResolution(_resolutionIndex);
		}
		
		public override void SetTableOption()
		{
			if (_resolutionIndex < 0)
			{
				C2VDebug.LogCategory("OptionController", $"ResolutionOption - new ResolutionIndex");
				_resolutionIndex = Convert.ToInt32(TargetTableData[eSetting.GRAPHICS_RESOLUTIONMOD].Default) - 1;
			}
		}

		public void OnScreenModeToggleEvent(bool isFullScreen) => _screenModeIndex = (isFullScreen) ? 0 : 1;
	}
}
