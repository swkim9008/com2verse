/*===============================================================
* Product:		Com2Verse
* File Name:	TogglePropertyExtensions.cs
* Developer:	jhkim
* Date:			2022-08-01 17:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] TogglePropertyExtensions")]
	public class TogglePropertyExtensions : MonoBehaviour
	{
		[SerializeField] private bool _isParentToggleGroup;
		protected MetaverseToggle _toggle;
		private Action<bool> _onValueWithoutNotify;
		private void Awake()
		{
			CheckToggle();

			CheckToggleGroup();
		}


		public bool ValueWithoutNotify
		{
			get
			{
				CheckToggle();
				return _toggle.isOn;
			}
			set
			{
				CheckToggle();
				_toggle.SetIsOnWithoutNotify(value);
				_onValueWithoutNotify?.Invoke(value);
			}
		}

		public bool ValueWithoutNotifyReverse
		{
			get
			{
				CheckToggle();
				return !_toggle.isOn;
			}
			set
			{
				CheckToggle();
				_toggle.SetIsOnWithoutNotify(!value);
				_onValueWithoutNotify?.Invoke(!value);
			}
		}

		public void SetOnValueWithoutNotify(Action<bool> onValueWithoutNotify) => _onValueWithoutNotify = onValueWithoutNotify;
		protected void CheckToggle()
		{
			if (_toggle.IsReferenceNull())
				_toggle = GetComponent<MetaverseToggle>();
		}

		private void CheckToggleGroup()
		{
			if (_isParentToggleGroup)
			{
				if (!transform.parent.IsReferenceNull())
				{
					var toggleGroup = transform.parent.GetComponent<ToggleGroup>();
					if (!toggleGroup.IsUnityNull())
						_toggle.group = toggleGroup;
				}
			}
		}
	}
}
