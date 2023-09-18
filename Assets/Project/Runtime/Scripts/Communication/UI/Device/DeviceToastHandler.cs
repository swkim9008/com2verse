/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceToastHandler.cs
* Developer:	urun4m0r1
* Date:			2023-07-13 14:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Communication;
using Com2Verse.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Project.Communication.UI
{
	[AddComponentMenu("[Communication]/[Communication] Device ToastHandler")]
	public sealed class DeviceToastHandler : MonoBehaviour
	{
#region InspectorFields
		[SerializeField] private string? _textKeyIfEmpty;
		[SerializeField] private string? _textKeyIfUnavailable;
#endregion // InspectorFields

		private DeviceInfo? _currentDevice;

		private void OnDestroy()
		{
			Clear();
		}

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public DeviceInfo? CurrentDevice
		{
			get => _currentDevice;
			set
			{
				var prevValue = _currentDevice;
				if (prevValue == value)
					return;

				_currentDevice = value;
			}
		}

		/// <summary>
		/// 상태를 초기화합니다.
		/// </summary>
		public void Clear()
		{
			CurrentDevice = null;
		}
#endregion // ViewModelProperties

		public void ShowToastIfEmpty()
		{
			if (string.IsNullOrWhiteSpace(_textKeyIfEmpty!))
				return;

			if (CurrentDevice == null)
				return;

			if (!CurrentDevice.IsEmptyDevice)
				return;

			var message = Localization.Instance.GetString(_textKeyIfEmpty);
			UIManager.Instance.SendToastMessage(message);
		}

		public void ShowToastIfUnavailable()
		{
			if (string.IsNullOrWhiteSpace(_textKeyIfUnavailable!))
				return;

			if (CurrentDevice == null)
				return;

			if (CurrentDevice.IsEmptyDevice)
				return;

			if (CurrentDevice.IsAvailable)
				return;

			var message = Localization.Instance.GetString(_textKeyIfUnavailable);
			UIManager.Instance.SendToastMessage(message);
		}
	}
}
