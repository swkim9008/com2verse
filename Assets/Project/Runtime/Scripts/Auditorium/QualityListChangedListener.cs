/*===============================================================
* Product:		Com2Verse
* File Name:	QualityListChangedListener.cs
* Developer:	haminjeong
* Date:			2023-08-18 11:40
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Option;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.Communication.UI
{
	[AddComponentMenu("[Communication]/[Communication] Quality List Changed Listener")]
	public sealed class QualityListChangedListener : MonoBehaviour
	{
#region InspectorFields
		[Header("Events")]
		[SerializeField] private UnityEvent<List<string>, int>? _qualityListChanged;
#endregion // InspectorFields

		private readonly List<string> _qualityNames = new();

		private void OnDestroy()
		{
			Clear();
		}

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public List<string> QualityNames
		{
			get => _qualityNames;
			set
			{
				_qualityNames.Clear();
				_qualityNames.AddRange(value);
				int index  = -1;
				var option = OptionController.Instance.GetOption<DeviceOption>();
				if (option != null)
					index = option.UseVoiceRecordingQuality-1;
				InvokeListChangedEvent(_qualityNames, index);
			}
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			_qualityNames.Clear();
		}
#endregion // ViewModelProperties
		private void InvokeListChangedEvent(List<string> deviceNames, int index)
		{
			_qualityListChanged?.Invoke(deviceNames, index);
		}
	}
}
