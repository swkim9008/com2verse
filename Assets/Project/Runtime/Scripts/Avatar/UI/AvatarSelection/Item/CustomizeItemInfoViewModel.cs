/*===============================================================
* Product:		Com2Verse
* File Name:	CustomizeItemInfoViewModel.cs
* Developer:	eugene9721
* Date:			2023-05-12 10:39
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
	public sealed class CustomizeItemInfoViewModel : ItemInfoViewModel
	{
		private string _customizeItemName; // 6월 빌드에서는 아이템 이름만 표시

		[UsedImplicitly]
		public string CustomizeItemName
		{
			get => _customizeItemName;
			set
			{
				SetProperty(ref _customizeItemName, value);
				InvokePropertyValueChanged(nameof(CustomizeItemActiveScale), CustomizeItemActiveScale);
			}
		}

		[UsedImplicitly]
		public Vector3 CustomizeItemActiveScale => !string.IsNullOrEmpty(CustomizeItemName) ? Vector3.one : Vector3.zero;

		/// <summary>
		/// 6월 빌드에서만 임시 사용
		/// </summary>
		protected override void SetFashionItemInfoPosition()
		{
			if (ItemInfoRect.IsUnityNull() || TweenControllerRect.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(nameof(ItemInfoTransform), $"{nameof(ItemInfoRect)} or {nameof(TweenControllerRect)} is null");
				return;
			}

			var prevScale = ItemInfoRect!.localScale;
			var infoCorners = ItemInfoRect!.GetCorners();
			var infoCornerMaxY = infoCorners[2].y;
			var infoCornerMinY = infoCorners[0].y;
			var yDiff = infoCornerMaxY - infoCornerMinY;
			ItemInfoRect.localScale = prevScale;

			var tweenControllerCorners = TweenControllerRect!.GetCorners();
			var tweenControllerMaxX    = tweenControllerCorners[2].x;
			var tweenControllerMinX    = tweenControllerCorners[0].x;
			var tweenControllerMinY    = tweenControllerCorners[0].y;
			var xDiff                  = tweenControllerMaxX - tweenControllerMinX;

			ItemInfoRect!.pivot   = RectTransformExtension.TopPivotPosition;
			ItemInfoRect.position = new Vector3(tweenControllerMaxX - xDiff * 0.5f, tweenControllerMinY + yDiff * 0.5f, 0);
		}
	}
}
