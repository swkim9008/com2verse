/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationItemController.cs
* Developer:	tlghks1009
* Date:			2022-10-07 13:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Com2Verse.Notification
{
	public class NotificationItemController : BaseNotificationItem
	{
		private Transform _transform;

		[SerializeField] private CanvasGroup _canvasGroup;
		[SerializeField] private LayoutElement _layoutElement;
		[SerializeField] private TextMeshProUGUI _desc;

		private const float NOTIFICATION_ITEM_BUTTONUSE_MAX_HEIGHT = 134.03f;
		private const float NOTIFICATION_ITEM_BUTTONNONUSE_MIN_HEIGHT = 80.03f;
		private const float NOTIFICATION_ITEM_TWEEN_DURATION_SECONDS = 0.2f;

		protected override void SetVisibleState(bool visible, UnityEvent onTweenFinished)
		{
			if (_transform.IsReferenceNull())
			{
				_transform = this.transform;
			}
			
			HeightSet(visible).Forget();
		}

		private async UniTask HeightSet(bool visible)
		{
			await UniTask.WaitUntil(() => _desc.textInfo.lineCount != 0);
			
			if (visible)
			{
				_layoutElement.preferredHeight = PreferredHeight + ((_desc.textInfo.lineCount - 1) * 18);
				_canvasGroup.alpha = 1;
			}
			else
			{
				 _canvasGroup.alpha = 1;
				 _canvasGroup.DOFade(0, NOTIFICATION_ITEM_TWEEN_DURATION_SECONDS);
			} 
		}

		private float PreferredHeight => _notificationInfo.NotificationData.IsFeedBack() ? NOTIFICATION_ITEM_BUTTONUSE_MAX_HEIGHT : NOTIFICATION_ITEM_BUTTONNONUSE_MIN_HEIGHT;
	}
}