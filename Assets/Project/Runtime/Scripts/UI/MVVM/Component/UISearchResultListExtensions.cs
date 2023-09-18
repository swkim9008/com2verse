/*===============================================================
* Product:		Com2Verse
* File Name:	UISearchResultListExtensions.cs
* Developer:	jhkim
* Date:			2022-09-07 18:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] UISearchResultListExtensions")]
	public sealed class UISearchResultListExtensions : MonoBehaviour
	{
		private UISearchResultList _searchResultList;
		private bool _intialized;

		// initialize from UISearchResultList
		public void Initialize()
		{
			if (!_intialized && TryGetComponent(out _searchResultList))
			{
				_searchResultList = GetComponent<UISearchResultList>();
				_searchResultList.OnValueChanged += OnValueChanged;
				_intialized = true;
			}
		}
		void OnValueChanged()
		{
			if (TryGetComponent(out _searchResultList))
			{
				var inputText = _searchResultList.TextWithIME.Trim();
				var needShow = _searchResultList.ChildCount > 0 || inputText.Length > 0;
				if (needShow)
				{
					if(!_searchResultList.IsShow)
						_searchResultList.ShowScrollView();
				}
				else
				{
					if(_searchResultList.IsShow)
						_searchResultList.HideScrollView();
				}
			}
		}

		private bool TryGetComponent(out UISearchResultList result)
		{
			result = GetComponent<UISearchResultList>();
			return !result.IsReferenceNull();
		}
	}
}
