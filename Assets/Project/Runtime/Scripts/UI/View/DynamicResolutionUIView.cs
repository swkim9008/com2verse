/*===============================================================
* Product:		Com2Verse
* File Name:	DynamicResolutionUIView.cs
* Developer:	eugene9721
* Date:			2022-09-27 22:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class DynamicResolutionUIView : UIView
	{
		protected override void Activate()
		{
			base.Activate();
			OnScreenResized(Screen.width, Screen.height);
			ScreenSize.Instance.ScreenResized += OnScreenResized;
		}

		protected override void Deactivate()
		{
			ScreenSize.Instance.ScreenResized -= OnScreenResized;
			base.Deactivate();
		}

		/// <summary>
		/// 화면의 비율이 달라짐에 따라 UI 크기 조정
		/// </summary>
		/// <param name="width">현재 스크린의 가로 길이</param>
		/// <param name="height">현재 스크린의 세로 길이</param>
		private void OnScreenResized(int width, int height)
		{
			var applyWidth  = ScreenSize.DefaultSize.x;
			var applyHeight = ScreenSize.DefaultSize.y;

			if (ScreenSize.BaseAxis == ScreenSize.eBaseAxis.WIDTH)
			{
				var aspectRatioInverse = (float)height / width;
				applyHeight = (int)(applyWidth * aspectRatioInverse);
			}
			else
			{
				var aspectRatio = (float)width / height;
				applyWidth = (int)(applyHeight * aspectRatio);
			}

			CanvasScaler.referenceResolution = new Vector2(applyWidth, applyHeight);
		}
	}
}
