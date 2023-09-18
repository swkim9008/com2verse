// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	SoundUtil.cs
//  * Developer:	yangsehoon
//  * Date:		2023-09-08 오전 10:34
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;

namespace Com2Verse.Sound
{
	public static class SoundUtil
	{
		public static float CalculateFadeRatio(eFadingMode mode, float remTime, float duration)
		{
			float ratio = mode switch
			{
				eFadingMode.LINEAR => remTime / duration,
				eFadingMode.QUADRATIC => remTime * remTime / duration / duration,
				eFadingMode.SQUAREROOT => (float)Math.Sqrt(remTime / duration),
				eFadingMode.SMOOTHSTEP => (float)(0.5f - Math.Cos(Math.PI * remTime / duration) / 2),
				_ => 0
			};

			return Math.Min(1, Math.Max(0, ratio));
		}

		public static float NormalizeVolume(float volume, float maxDecibel, float minDecibel)
		{
			return Cubic((volume - minDecibel) / (maxDecibel - minDecibel));
		}

		public static float DeNormalizeVolume(float normalized, float maxDecibel, float minDecibel)
		{
			return (float)Math.Cbrt(normalized) * (maxDecibel - minDecibel) + minDecibel;
		}

		public static float Cubic(float x) => (float)Math.Pow(x, 3);
	}
}
