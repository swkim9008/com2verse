/*===============================================================
* Product:		Com2Verse
* File Name:	SequentialButtonGroups.cs
* Developer:	eugene9721
* Date:			2022-12-12 10:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	public sealed class SequentialButtonGroups : MonoBehaviour
	{
		[SerializeField] private List<Button> _buttons = new List<Button>();
		[SerializeField] private bool         _isAlwaysEnableFirstItem;
		
		private int _index;
		
		private void Awake()
		{
			foreach (var button in _buttons)
				button.onClick.AddListener(EnableNextButton);
		}

		private void OnEnable()
		{
			if (_isAlwaysEnableFirstItem)
				EnableFirstItem();
		}

		private void EnableFirstItem()
		{
			if (_buttons.Count <= 0) return;
			foreach (var button in _buttons)
			{
				button.gameObject.SetActive(false);
			}
			_buttons[0].gameObject.SetActive(true);
			_index = 0;
		}

		private void EnableNextButton()
		{
			if (_buttons.Count <= 0) return;
			_buttons[_index].gameObject.SetActive(false);
			_index = (_index + 1) % _buttons.Count;
			_buttons[_index].gameObject.SetActive(true);
		}
	}
}
