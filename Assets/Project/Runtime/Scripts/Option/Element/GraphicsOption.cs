/*===============================================================
* Product:		Com2Verse
* File Name:	GraphicsOption.cs
* Developer:	jehyun
* Date:			2022-10-16 3:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Com2Verse.Option
{
	[Serializable] [MetaverseOption("GraphicsOption")]
	public sealed class GraphicsOption : BaseMetaverseOption
	{
		public enum eQualityLevel
		{
			HIGH_FIDELITY,
			BALANCED,
			PERFORMANT,
			/// <summary>
			/// No rendering processing.
			/// </summary>
			EMPTY,
			MAX
		}

		// URP-HighFidelity, URP-Balanced, URP-Performant
		private readonly float[] RENDER_SCALE_QHD = new[] { 1.0f, 0.667f, 0.5f };
		private readonly float[] RENDER_SCALE_FHD = new[] { 1.0f, 0.667f, 0.5f };
		private readonly int QUALITY_LEVEL_INITIAL_VALUE = -1;

		private int _qualityLevelCount = 0;
		private bool _isValidateQualityLevel = true;

		[SerializeField]
		private int _qualityLevel = -1;
		private bool _emptyQualityLevelEnabled = false;

		/// <summary>
		/// The last index is an Empty Level, so set it to -2.
		/// </summary>
		public int MaxQualityLevelIndex => _qualityLevelCount - 2;

		public bool IsValidateQualityLevel => _isValidateQualityLevel;

		public int QualityLevel
		{
			get => _qualityLevel;
			set
			{
				if ((int)eQualityLevel.EMPTY != value)
				{
					if (_qualityLevel != value)
					{
						_qualityLevel = value;
						RefreshQualityAsset(_qualityLevel);
					}
				}
				else
					C2VDebug.LogWarning("eQualityLevel.EMPTY must be executed with EmptyQualityLevelEnabled.");
			}
		}

		public bool EmptyQualityLevelEnabled
		{
			get => _emptyQualityLevelEnabled;
			set
			{
				if (_emptyQualityLevelEnabled != value)
				{
					_emptyQualityLevelEnabled = value;
					RefreshQualityAsset(_emptyQualityLevelEnabled ? (int)eQualityLevel.EMPTY : QualityLevel);
				}
			}
		}

		private Action<float> _onChangedRenderScale = null;
		public event Action<float> OnChangedRenderScale
		{
			add
			{
				_onChangedRenderScale -= value;
				_onChangedRenderScale += value;
			}
			remove => _onChangedRenderScale -= value;
		}

		private Action<eQualityLevel> _onChangedQualityLevel = null;

		public event Action<eQualityLevel> OnChangedQualityLevel
		{
			add
			{
				_onChangedQualityLevel -= value;
				_onChangedQualityLevel += value;
			}
			remove => _onChangedQualityLevel -= value;
		}

		public GraphicsOption()
		{
			_qualityLevel = QUALITY_LEVEL_INITIAL_VALUE;
			_qualityLevelCount = QualitySettings.names.Length;
			_isValidateQualityLevel = (int)eQualityLevel.MAX == _qualityLevelCount;
			if(!_isValidateQualityLevel)
				C2VDebug.LogError($"The number of quality levels and the enum maximum do not match. - Enum Max : {(int)eQualityLevel.MAX}, Quality Level Count : {_qualityLevelCount}");
		}

		public override void OnInitialize()
		{
			Apply();
		}

		public override void Apply()
		{
			base.Apply();

			// Set to HighFidelity if _qualityLevel equals QUALITY LEVEL INITIAL_VALUE
			if (_qualityLevel == QUALITY_LEVEL_INITIAL_VALUE)
				_qualityLevel = (int)eQualityLevel.HIGH_FIDELITY;
			
			RefreshQualityAsset(QualityLevel);
		}

		private void RefreshQualityAsset(int level)
		{
			if (!_isValidateQualityLevel)
			{
				C2VDebug.LogError($"The number of quality levels and the enum maximum do not match. - Enum Max : {(int)eQualityLevel.MAX}, Quality Level Count : {_qualityLevelCount}");
				return;
			}

			if (EmptyQualityLevelEnabled)
			{
				QualitySettings.SetQualityLevel(level);
				_onChangedQualityLevel?.Invoke((eQualityLevel)level);
				base.SaveData();
			}
			else
			{
				level = Mathf.Clamp(level, 0, Mathf.Min(RENDER_SCALE_QHD.Length - 1, MaxQualityLevelIndex));
				QualitySettings.SetQualityLevel(level);
				_onChangedQualityLevel?.Invoke((eQualityLevel)level);
				base.SaveData();
				SetRenderScale(level);
			}
		}

		private void SetRenderScale(int qualityLevel)
		{
			var renderPipelineAsset = (UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(qualityLevel);
			// TODO 모니터를 세로로 길게 사용하는 경우 처리를 어떻게 할 것인가?
			int height = Screen.currentResolution.height;
#if UNITY_EDITOR
			height = Convert.ToInt32(ScreenSize.Instance.GetMainGameViewSize().y);
#endif
			if (!renderPipelineAsset.IsUnityNull())
			{
				var currentRenderScale = renderPipelineAsset.renderScale;
				var nextRenderScale = currentRenderScale;

				// FIXME
				// render scale 임시 처리
				// 1440P 이상인 경우 Render Scale일 강제로 낮춤
				if (height >= 1440)
					nextRenderScale = RENDER_SCALE_QHD[qualityLevel];
				else
					nextRenderScale = RENDER_SCALE_FHD[qualityLevel];

				if (Math.Abs(currentRenderScale - nextRenderScale) > 0.01f)
				{
					renderPipelineAsset.renderScale = nextRenderScale;
					_onChangedRenderScale?.Invoke(nextRenderScale);
				}
			}
		}

		public override void SetTableOption()
		{
			if (_qualityLevel < 0)
			{
				C2VDebug.LogCategory("OptionController", $"GraphicsOption - new QualityLevel");
				_qualityLevel = Convert.ToInt32(TargetTableData[eSetting.GRAPHICS_QUALITY].Default) - 1;
			}
		}
	}
}
