/*===============================================================
 * Product:		Com2Verse
 * File Name:	VoiceUI.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-12 14:35
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Communication
{
	[AddComponentMenu("[Communication]/[Communication] Voice UI")]
	public class VoiceUI : MonoBehaviour
	{
		protected virtual void OnEnable()  => RegisterVoiceUI();
		protected virtual void OnDisable() => UnregisterVoiceUI();

		private void RegisterVoiceUI()
		{
			DeviceModuleManager.Instance.TryAddVoiceUI(this);
		}

		private void UnregisterVoiceUI()
		{
			DeviceModuleManager.InstanceOrNull?.RemoveVoiceUI(this);
		}
	}
}
