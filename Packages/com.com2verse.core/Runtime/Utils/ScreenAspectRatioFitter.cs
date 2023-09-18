/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenAspectRatioFitter.cs
* Developer:	urun4m0r1
* Date:			2022-07-20 16:48
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine.UI;

namespace Com2Verse.Utils
{
	public sealed class ScreenAspectRatioFitter : AspectRatioFitter
	{
		private static ScreenSize? ScreenSize => ScreenSize.InstanceOrNull;

		protected override void Awake()
		{
			base.Awake();
			OnScreenResized(ScreenSize.Instance.Width, ScreenSize.Instance.Height);
			if (ScreenSize != null) ScreenSize.ScreenResized += OnScreenResized;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (ScreenSize != null) ScreenSize.ScreenResized -= OnScreenResized;
		}

		private void OnScreenResized(int width, int height)
		{
			aspectRatio = (float)width / height;
		}
	}
}
