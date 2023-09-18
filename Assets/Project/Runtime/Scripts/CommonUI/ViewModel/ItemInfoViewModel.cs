/*===============================================================
* Product:		Com2Verse
* File Name:	ItemInfoViewModel.cs
* Developer:	eugene9721
* Date:			2023-04-13 10:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public abstract class ItemInfoViewModel : ViewModelBase
	{
		private Transform _tweenControllerTransform;
		private Vector2   _pivot;
		private Transform _itemInfoTransform;

		protected RectTransform TweenControllerRect { get; set; }
		protected RectTransform ItemInfoRect        { get; set; }

		[UsedImplicitly]
		public Transform TweenControllerTransform
		{
			get => _tweenControllerTransform;
			set
			{
				SetProperty(ref _tweenControllerTransform, value);
				TweenControllerRect = _tweenControllerTransform as RectTransform;
				SetFashionItemInfoPosition();
			}
		}

		[UsedImplicitly]
		public Transform ItemInfoTransform
		{
			get => _itemInfoTransform;
			set
			{
				SetProperty(ref _itemInfoTransform, value);
				ItemInfoRect = _itemInfoTransform as RectTransform;
			}
		}

		protected virtual void SetFashionItemInfoPosition()
		{
			if (ItemInfoRect.IsUnityNull() || TweenControllerRect.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(nameof(ItemInfoTransform), $"{nameof(ItemInfoRect)} or {nameof(TweenControllerRect)} is null");
				return;
			}

			var infoCorners    = ItemInfoRect!.GetCorners();
			var infoCornerMaxX = infoCorners[2].x;
			var infoCornerMinX = infoCorners[0].x;

			var tweenControllerCorners = TweenControllerRect!.GetCorners();
			var tweenControllerMaxX    = tweenControllerCorners[2].x;
			var tweenControllerMinX    = tweenControllerCorners[0].x;
			var tweenControllerMaxY    = tweenControllerCorners[1].y;

			if (tweenControllerMinX - (infoCornerMaxX - infoCornerMinX) < 0)
			{
				ItemInfoRect.pivot    = RectTransformExtension.TopLeftPivotPosition;
				ItemInfoRect.position = new Vector3(tweenControllerMaxX, tweenControllerMaxY, 0);
			}
			else
			{
				ItemInfoRect.pivot    = RectTransformExtension.TopRightPivotPosition;
				ItemInfoRect.position = new Vector3(tweenControllerMinX, tweenControllerMaxY, 0);
			}
		}
	}
}
