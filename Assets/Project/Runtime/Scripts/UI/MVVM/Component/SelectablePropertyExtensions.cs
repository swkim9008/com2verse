/*===============================================================
* Product:		Com2Verse
* File Name:	SelectablePropertyExtensions.cs
* Developer:	haminjeong
* Date:			2022-07-19 13:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(Selectable))]
	[AddComponentMenu("[DB]/[DB] SelectablePropertyExtensions")]
	public sealed class SelectablePropertyExtensions : MonoBehaviour
	{
		private Selectable _selectable;
		private bool _isSelected;

		private void Awake()
		{
			_selectable = GetComponent<Selectable>();
			_isSelected = false;
		}

		[UsedImplicitly]
		public bool IsSelect
		{
			get
			{
				if (_selectable == null)
				{
					_selectable = GetComponent<Selectable>();
					_isSelected = false;
				}
				return _isSelected;
			}
			set
			{
				_isSelected = value;
				if (_isSelected)
					_selectable.OnSelect(null);
				else
					_selectable.OnDeselect(null);
			}
		}

		[UsedImplicitly]
		public bool SetDeselect
		{
			get => EventSystem.current.currentSelectedGameObject != gameObject;
			set
			{
				if (value && EventSystem.current.currentSelectedGameObject == gameObject)
					EventSystem.current.SetSelectedGameObject(null);
			}
		}

		[UsedImplicitly]
		public bool SetDeselectReverse
		{
			get => EventSystem.current.currentSelectedGameObject == gameObject;
			set
			{
				if (!value && EventSystem.current.currentSelectedGameObject == gameObject)
					EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}
}
