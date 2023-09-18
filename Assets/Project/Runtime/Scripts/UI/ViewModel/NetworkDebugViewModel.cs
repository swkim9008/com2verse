/*===============================================================
* Product:		Com2Verse
* File Name:	NetworkDebugViewModel.cs
* Developer:	haminjeong
* Date:			2022-11-21 15:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public sealed class NetworkDebugViewModel : ViewModelBase
	{
		private string _rttText          = string.Empty;
		private string _processDelayText = string.Empty;
		private string _objectCountText  = string.Empty;

		[UsedImplicitly]
		public string RTTText
		{
			get => _rttText;
			set
			{
				_rttText = value;
				InvokePropertyValueChanged(nameof(RTTText), value);
			}
		}

		[UsedImplicitly]
		public string ProcessDelayText
		{
			get => _processDelayText;
			set
			{
				_processDelayText = value;
				InvokePropertyValueChanged(nameof(ProcessDelayText), value);
			}
		}

		[UsedImplicitly]
		public string ObjectCountText
		{
			get => _objectCountText;
			set
			{
				_objectCountText = value;
				InvokePropertyValueChanged(nameof(ObjectCountText), value);
			}
		}
	}
}
