/*===============================================================
* Product:		Com2Verse
* File Name:	ToggleSound.cs
* Developer:	tlghks1009
* Date:			2022-05-27 09:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Com2Verse.Sound
{
	[RequireComponent(typeof(Toggle))]
	public sealed class ToggleSound : MonoBehaviour
	{
		[SerializeField] private AssetReference _audioFile;
		private Toggle _toggle;
		private UnityEngine.Events.UnityAction<bool> _clickAction;

		private void Awake()
		{
			_toggle = GetComponent<Toggle>();
			_clickAction = Click;
			_toggle.onValueChanged.AddListener( _clickAction );
		}

		private void OnDestroy()
		{
			_toggle.onValueChanged.RemoveListener(_clickAction);
		}

		private void Click( bool isOn )
		{
			if ( isOn )
				SoundManager.Instance.PlayUISound(_audioFile);
		}
	}
}
