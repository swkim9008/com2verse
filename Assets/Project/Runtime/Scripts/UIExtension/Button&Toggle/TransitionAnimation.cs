/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseButtonAnimation.cs
* Developer:	tlghks1009
* Date:			2022-05-24 10:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.UI
{
	public sealed class TransitionAnimation : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
	{
		[SerializeField] private Animator _targetAnimator;
		[SerializeField] private string _pressedTrigger = "Pressed";
		
		private Vector3 _originalScale;
		
		private void Awake()
		{
			FindTargetAnimator();
			_originalScale = this.transform.localScale;
		}


		public void OnPointerDown(PointerEventData eventData)
		{
			if (_targetAnimator.gameObject.activeInHierarchy)
				_targetAnimator.Play(_pressedTrigger, -1, 0f);
		}

		public void OnPointerClick(PointerEventData eventData)
		{	
			_targetAnimator.transform.localScale = _originalScale;
		}

		private void FindTargetAnimator()
		{
			if (_targetAnimator == null)
			{
				var transformOfChild = transform.GetChild(0);
				if (ReferenceEquals(transformOfChild, null))
				{
					C2VDebug.LogError($"[Button] child not found. Name : {this.name}");
					return;
				}

				_targetAnimator = transformOfChild.GetComponent<Animator>();
				if (_targetAnimator == null)
				{
					C2VDebug.LogError($"[Button] Animator not found. Name : { this.name }");
					return;
				}
			}
		}
	}
}
