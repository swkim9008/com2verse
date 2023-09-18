/*===============================================================
* Product:		Com2Verse
* File Name:	LightControlBehaviour.cs
* Developer:	eugene9721
* Date:			2022-12-07 10:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.Playables;

namespace Com2Verse.Director
{
	[Serializable]
	public sealed class LightControlBehaviour : PlayableBehaviour
	{
		[SerializeField] private Color _color           = Color.white;
		[SerializeField] private float _intensity       = 1f;
		[SerializeField] private float _bounceIntensity = 1f;
		[SerializeField] private float _range           = 10f;

		private Light _light;

		private bool _firstFrameHappened;

		private Color _defaultColor;
		private float _defaultIntensity;
		private float _defaultBounceIntensity;
		private float _defaultRange;

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			var light = playerData as Light;
			if (light.IsUnityNull()) return;

			CheckFirstFrame(light);

			int inputCount = playable.GetInputCount();

			Color blendedColor           = Color.clear;
			float blendedIntensity       = 0f;
			float blendedBounceIntensity = 0f;
			float blendedRange           = 0f;
			float totalWeight            = 0f;

			for (int i = 0; i < inputCount; i++)
			{
				float inputWeight   = playable.GetInputWeight(i);
				var   inputPlayable = (ScriptPlayable<LightControlBehaviour>)playable.GetInput(i);
				var   input         = inputPlayable.GetBehaviour();

				blendedColor           += input._color * inputWeight;
				blendedIntensity       += input._intensity * inputWeight;
				blendedBounceIntensity += input._bounceIntensity * inputWeight;
				blendedRange           += input._range * inputWeight;
				totalWeight            += inputWeight;
			}

			light!.color          = blendedColor + _defaultColor * (1f - totalWeight);
			light.intensity       = blendedIntensity + _defaultIntensity * (1f - totalWeight);
			light.bounceIntensity = blendedBounceIntensity + _defaultBounceIntensity * (1f - totalWeight);
			light.range           = blendedRange + _defaultRange * (1f - totalWeight);
		}

		private void CheckFirstFrame(Light light)
		{
			if (_firstFrameHappened) return;
			_firstFrameHappened = true;

			_defaultColor           = light.color;
			_defaultIntensity       = light.intensity;
			_defaultBounceIntensity = light.bounceIntensity;
			_defaultRange           = light.range;
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			InitializeData();
		}

		public override void OnPlayableDestroy(Playable playable)
		{
			InitializeData();
			_light = null;
		}

		/// <summary>
		/// 타임라인이 끝난 후 라이트 컴포넌트를 초기화시키기 위한 목적으로 사용됩니다.
		/// </summary>
		private void InitializeData()
		{
			_firstFrameHappened = false;

			if (_light.IsUnityNull())
				return;

			_light.color           = _defaultColor;
			_light.intensity       = _defaultIntensity;
			_light.bounceIntensity = _defaultBounceIntensity;
			_light.range           = _defaultRange;
		}
	}
}
