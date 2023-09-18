/*===============================================================
* Product:		Com2Verse
* File Name:	LoopbackVolumeViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-29 21:34
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication.Unity;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class LoopbackVolumeViewModel : ViewModelBase, IDisposable
	{
		public LoopbackVolumeViewModel()
		{
			ModuleManager.Instance.Voice.VolumeDetector.VuLevelChanged += OnVuLevelChanged;
		}

		public void Dispose()
		{
			var detector = ModuleManager.InstanceOrNull?.Voice.VolumeDetector;
			if (detector != null)
			{
				detector.VuLevelChanged -= OnVuLevelChanged;
			}
		}

		private void OnVuLevelChanged(float _)
		{
			InvokePropertyValueChanged(nameof(VuLevel),                     VuLevel);
			InvokePropertyValueChanged(nameof(InversedVuLevel),             InversedVuLevel);
			InvokePropertyValueChanged(nameof(InterpolatedVuLevel),         InterpolatedVuLevel);
			InvokePropertyValueChanged(nameof(InterpolatedInversedVuLevel), InterpolatedInversedVuLevel);
		}

#region ViewModelProperties
		/// <summary>
		/// 실제 녹음되는 소리의 음량
		/// </summary>
		public float VuLevel => ModuleManager.InstanceOrNull?.Voice.VolumeDetector.VuLevel ?? 0f;

		/// <summary>
		/// 1 - VuLevel (UI 표시용)
		/// </summary>
		public float InversedVuLevel => 1f - VuLevel;

		/// <summary>
		/// VuLevel을 증폭한 값 (UI 표시용)
		/// </summary>
		public float InterpolatedVuLevel => VuLevel * Utils.Define.AudioInputLevelUiMultiplier;

		/// <summary>
		/// 1 - InterpolatedVuLevel (UI 표시용)
		/// </summary>
		public float InterpolatedInversedVuLevel => 1f - InterpolatedVuLevel;
#endregion // ViewModelProperties
	}
}
