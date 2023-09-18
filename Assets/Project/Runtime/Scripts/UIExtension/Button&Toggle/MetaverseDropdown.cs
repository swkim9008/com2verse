/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseDropdown.cs
* Developer:	mikeyid77
* Date:			2022-10-17 11:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.UI
{
	[AddComponentMenu("[CVUI]/[CVUI] MetaverseDropdown")]
	[ExecuteInEditMode]
	public sealed class MetaverseDropdown : TMP_Dropdown
	{
		private event Action _onClicked;
		public event Action OnClicked
		{
			add
			{
				_onClicked -= value;
				_onClicked += value;
			}
			remove => _onClicked -= value;
		}
		
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			if (interactable)
			{
				_onClicked?.Invoke();
			}
		}

		private void SetPrivateGroup()
		{
			
		}

		private void SetNormalGroup()
		{
			
		}

		public void ShowDropdown()
		{
			Show();
		}
	}
}
