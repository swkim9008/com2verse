/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadPageNavigator.cs
* Developer:	mikeyid77
* Date:			2023-04-03 16:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Com2Verse.Logger;

namespace Com2Verse.UI
{
	public sealed class MyPadPageNavigator : MonoBehaviour
	{
		private int _currentPage = 0;
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private HorizontalScrollSnap _horizontalScrollSnap;
		public UnityEvent PageChangedEvent;
		
		public int CurrentPage
		{
			get => _currentPage;
			set => _currentPage = value;
		}
		
		private void Start()
		{
			_scrollRect.scrollSensitivity = 0;
			MyPadManager.Instance.OnMyPadClosedEvent += ResetToFirstPage;
		}

		private void ResetToFirstPage()
		{
			// TODO : 페이지 되돌리기 관련 이슈 수정 필요
			// C2VDebug.LogCategory("MyPad", $"{nameof(ResetToFirstPage)}");
			// _horizontalScrollSnap.ChangePage(0);
			// _scrollRect.enabled = false; 
		}

		public void ChangingMyPadPage(int page)
		{
			CurrentPage = page;
			PageChangedEvent?.Invoke();
		}
	}
}
