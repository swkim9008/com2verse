/*===============================================================
* Product:		Com2Verse
* File Name:	Ren.cs
* Developer:	ljk
* Date:			2022-07-22 15:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = System.Object;

namespace Com2Verse.Rendering
{
	public class GraphicAttributeInfo
	{
		public string _propertyName;
		public string _target;
		public string _attributeDisplayName;

		public Vector2 _recommendedValueRange;

		public bool _integerMustSquare = false;
		//public object _originalValue; // 테이블값?
	}

	public enum ePostProcessEffect
	{
		ALL,
		BLOOM,
		VIGNETTE,
		TONEMAPPING,
		MAX
	}
	
	public sealed class GraphicsSettingsManager : MonoSingleton<GraphicsSettingsManager>
	{
		private readonly string C2V_QUALITY_LEVEL = "C2V_Quality_Level";
		
		private GraphicsSettingsViewModel _uiViewModel;

		private UniversalRenderPipelineAsset _renderPipelineAsset;
		private ScriptableRendererData[] _scriptableRenderers;
		private List<ScriptableRendererFeature> _renderingFeatures;

		private int _emptyTargetLevel = 0;
		
		private int _currentQualityLevel;
		private bool _isInit = false;
		private Dictionary<int, PostProcessData> _cachePostProcessPerQuality;
		// TODO : 기획에서 언젠가 제어 범위를 조절하고 싶다면 테이블로부터 구성하는 형태로
		private List<GraphicAttributeInfo> _editableGraphicAttributeInfos =
			new List<GraphicAttributeInfo>()
			{
				// QualitySettings
				new GraphicAttributeInfo(){_propertyName = "realtimeReflectionProbes", _target = "QualitySettings" , _attributeDisplayName = " 실시간 리플렉션 프로브 " , _recommendedValueRange = new Vector2(0,1)},
				new GraphicAttributeInfo(){_propertyName = "resolutionScalingFixedDPIFactor", _target = "QualitySettings" , _attributeDisplayName = " 해상도 고정 DPI " , _recommendedValueRange = new Vector2(0,5)},
				new GraphicAttributeInfo(){_propertyName = "vSyncCount", _target = "QualitySettings" , _attributeDisplayName = " vSync Count " , _recommendedValueRange = new Vector2(0,2)},
				new GraphicAttributeInfo(){_propertyName = "masterTextureLimit", _target = "QualitySettings" , _attributeDisplayName = " 최대 텍스쳐 크기(밉맵) " , _recommendedValueRange = new Vector2(0,3)},
				new GraphicAttributeInfo(){_propertyName = "anisotropicFiltering", _target = "QualitySettings" , _attributeDisplayName = " 기울어진 텍스쳐 렌더링 " , _recommendedValueRange = Vector2.zero},
				
				new GraphicAttributeInfo(){_propertyName = "streamingMipmapsActive", _target = "QualitySettings" , _attributeDisplayName = " 밉맵 스트리밍 " , _recommendedValueRange = new Vector2(0,1)},
				new GraphicAttributeInfo(){_propertyName = "antiAliasing", _target = "QualitySettings" , _attributeDisplayName = " 안티 앨리어싱 " , _recommendedValueRange = new Vector2(0,8) , _integerMustSquare = true},
				new GraphicAttributeInfo(){_propertyName = "particleRaycastBudget", _target = "QualitySettings" , _attributeDisplayName = " 파티클 Raycast 수 " , _recommendedValueRange = new Vector2(0,4096)},
				new GraphicAttributeInfo(){_propertyName = "billboardsFaceCameraPosition", _target = "QualitySettings" , _attributeDisplayName = " 카메라를 향한 빌보드 " , _recommendedValueRange = new Vector2(0,1)},
				new GraphicAttributeInfo(){_propertyName = "softParticles", _target = "QualitySettings" , _attributeDisplayName = " 부드러운 파티클 경계 " , _recommendedValueRange = new Vector2(0,1)},
				
				new GraphicAttributeInfo(){_propertyName = "shadowmaskMode", _target = "QualitySettings" , _attributeDisplayName = " ShadowMask 모드 " , _recommendedValueRange = Vector2.zero},
				// URPAsset
				new GraphicAttributeInfo(){_propertyName = "supportsHDR", _target = "URPAsset" , _attributeDisplayName = " HDR 지원 " , _recommendedValueRange = Vector2.zero},
				new GraphicAttributeInfo(){_propertyName = "msaaSampleCount", _target = "URPAsset" , _attributeDisplayName = " MSAA 샘플카운트 " , _recommendedValueRange = new Vector2(0,8),_integerMustSquare = true},
				new GraphicAttributeInfo(){_propertyName = "renderScale", _target = "URPAsset" , _attributeDisplayName = " 렌더링 스케일 " , _recommendedValueRange = new Vector2(0.1f,2)},
				new GraphicAttributeInfo(){_propertyName = "upscalingFilter", _target = "URPAsset" , _attributeDisplayName = " Upscaling Filter " , _recommendedValueRange = Vector2.zero},
				new GraphicAttributeInfo(){_propertyName = "supportsCameraDepthTexture", _target = "URPAsset" , _attributeDisplayName = " 카메라 뎁스텍스쳐 지원 " , _recommendedValueRange = Vector2.zero},
				
				new GraphicAttributeInfo(){_propertyName = "mainLightRenderingMode", _target = "URPAsset" , _attributeDisplayName = " 메인 라이트 렌더링모드 " , _recommendedValueRange = Vector2.zero},
				new GraphicAttributeInfo(){_propertyName = "supportsMainLightShadows", _target = "URPAsset" , _attributeDisplayName = " 메인 라이트 그림자 " , _recommendedValueRange = new Vector2(0,1)},
				new GraphicAttributeInfo(){_propertyName = "mainLightShadowmapResolution", _target = "URPAsset" , _attributeDisplayName = " 메인 라이트 그림자 해상도 " , _recommendedValueRange = new Vector2(1,4096),_integerMustSquare = true},
				new GraphicAttributeInfo(){_propertyName = "additionalLightsRenderingMode", _target = "URPAsset" , _attributeDisplayName = " 추가 라이트 렌더링 모드 " , _recommendedValueRange = Vector2.zero},
				new GraphicAttributeInfo(){_propertyName = "supportsAdditionalLightShadows", _target = "URPAsset" , _attributeDisplayName = " 추가 라이트 그림자 " , _recommendedValueRange = new Vector2(0,1)},
				
				new GraphicAttributeInfo(){_propertyName = "reflectionProbeBlending", _target = "URPAsset" , _attributeDisplayName = " 반사 프로브 블렌딩 " , _recommendedValueRange = new Vector2(0,1)},
				new GraphicAttributeInfo(){_propertyName = "reflectionProbeBoxProjection", _target = "URPAsset" , _attributeDisplayName = " 박스 반사 프로브 " , _recommendedValueRange = new Vector2(0,1)},
				new GraphicAttributeInfo(){_propertyName = "shadowDistance", _target = "URPAsset" , _attributeDisplayName = " 그림자 거리 " , _recommendedValueRange = new Vector2(1,999)},
				new GraphicAttributeInfo(){_propertyName = "shadowCascadeCount", _target = "URPAsset" , _attributeDisplayName = " 그림자 단계 수 " , _recommendedValueRange = new Vector2(0,8)},
				new GraphicAttributeInfo(){_propertyName = "supportsSoftShadows", _target = "URPAsset" , _attributeDisplayName = " 소프트 쉐도우 지원 " , _recommendedValueRange = new Vector2(0,1)},
				// Application
				new GraphicAttributeInfo(){_propertyName = "targetFrameRate", _target = "Application" , _attributeDisplayName = " Fps " , _recommendedValueRange = new Vector2(-1,500)},
				
				// Post Processing
				new GraphicAttributeInfo(){_propertyName = "postprocessing", _target = "Rendering_Additional" , _attributeDisplayName = " 포스트프로세싱 " , _recommendedValueRange = new Vector2(0,1)},
				
				// quality setting functionality
				new GraphicAttributeInfo(){_propertyName = "qualityPreset", _target = "Rendering_Additional" , _attributeDisplayName = " 퀄리티세팅레벨 " , _recommendedValueRange = new Vector2(0,3)},
			};

		private Dictionary<string, GraphicAttributeInfo> _editableGraphicAttributeInfosDic;
		private Dictionary<string, Object> _cacheLastModification;

		public Action _onQualitySettingChanged;
		
		public List<GraphicAttributeInfo> EditableGraphicAttributeInfos
		{
			get
			{
				if(!_isInit)
					Initialize();
				return _editableGraphicAttributeInfos;
			}
		}
		
		public int QualityLevelCount => QualitySettings.names.Length;
		
		/// FIXME
		/// jehyun
		/// 임시 저장! 그래픽스 세팅 설정이 추가되면 변경 필요
		public int QualityLevel
		{
			get
			{
				if (LocalSave.Temp.IsExist(C2V_QUALITY_LEVEL))
					LocalSave.Temp.SaveInt(C2V_QUALITY_LEVEL, QualityLevelCount - 1);
				return LocalSave.Temp.LoadInt(C2V_QUALITY_LEVEL);
			}
			set => LocalSave.Temp.SaveInt(C2V_QUALITY_LEVEL, Mathf.Clamp(value, 0, QualityLevelCount - 1));
		}

#region Mono

		private void OnEnable()
		{
			Initialize();
		}

#endregion
		
		// 월드 렌더러를 끕니다. ( 월드 렌더러 <-> 화면 클리어용 렌더러
		public void EnableWorldRenderer(bool enable)
		{
			if(!enable)
				QualitySettings.SetQualityLevel(QualityLevelCount-1);
			else
				QualitySettings.SetQualityLevel(_currentQualityLevel);
		}
		public void Initialize()
		{
			if(_isInit)
				return;
			
			RefreshQualityAsset();

			if (_editableGraphicAttributeInfosDic == null)
			{
				_editableGraphicAttributeInfosDic = new Dictionary<string, GraphicAttributeInfo>();
				_editableGraphicAttributeInfos.ForEach( x => _editableGraphicAttributeInfosDic.Add(x._propertyName,x));
			}
			
			KeepBeforeModify();

			_isInit = true;
		}

		public void RefreshQualityAsset()
		{
			_currentQualityLevel = QualityLevel;
			
			_renderPipelineAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
			
			_scriptableRenderers = (ScriptableRendererData[])(typeof(UniversalRenderPipelineAsset))
				.GetField("m_RendererDataList",BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(_renderPipelineAsset);
			_renderingFeatures = new List<ScriptableRendererFeature>();
			for (int i = 0; i < _scriptableRenderers.Length; i++)
				_renderingFeatures.AddRange(_scriptableRenderers[i].rendererFeatures);
		}

		/// <summary>
		/// 내용 수정 전 호출 ( ex. 옵션창 열 때 )
		/// </summary>
		public void Prepare()
		{
			RefreshQualityAsset();
			KeepBeforeModify();
		}

		// - 마지막 세팅값 추적 ( 유저가 변경내용을 Discard 할 경우 )
		// 변경내용이 실시간으로 게임에 반영되어 보일 필요가 없다면 기능 폐기
		private void KeepBeforeModify()
		{ 
			_cacheLastModification = new Dictionary<string, object>();
			
			_editableGraphicAttributeInfos.ForEach(x =>
			{
				object value = GetRenderingProperty(x._propertyName);
				_cacheLastModification.Add(x._propertyName,value);
			});
		}

		/// <summary>
		/// 수정내용 폐기후 마지막 설정값으로 복원
		/// </summary>
		public void DiscardModifiedAndRestore()
		{
			_editableGraphicAttributeInfos.ForEach(x =>
			{
				SetRenderingProperty(x._propertyName,_cacheLastModification[x._propertyName]);
			});
		}

		/// <summary>
		/// 마지막 설정값 저장
		/// </summary>
		public void SaveModified()
		{
			KeepBeforeModify();
		}

		public void OpenWindow()
		{
			UIManager.Instance.LoadUICanvasRootAsync("UI_Cheat_GraphicsSettings",
				(cheatWindow) =>
				{
					
				}).Forget();
		}

		/// <summary>
		/// 렌더링 설정값 셋
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		public void SetRenderingProperty( string propertyName, object value )
		{
			if(!_editableGraphicAttributeInfosDic.ContainsKey(propertyName))
				return;
			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("QualitySettings"))
				typeof(QualitySettings).GetProperty(propertyName).SetValue(null,value);
			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("URPAsset"))
				typeof(UniversalRenderPipelineAsset).GetProperty(propertyName).SetValue(_renderPipelineAsset,value);
			if(_editableGraphicAttributeInfosDic[propertyName]._target.Equals("Application"))
				typeof(Application).GetProperty(propertyName).SetValue(null,value);
			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("Rendering_Additional"))
			{
				if (propertyName.Equals("postprocessing"))
					TryEnablePostProcessing(ePostProcessEffect.ALL,(bool)value);
				else if (propertyName.Equals("qualityPreset"))
				{
					if (qualityChangeLocker == null)
					{
						QualitySettings.SetQualityLevel(Mathf.Clamp((int)value,0,QualitySettings.names.Length),true);
						qualityChangeLocker = StartCoroutine(LockUpdateQualityLevel());
					}
				}
			}
		}

		private Coroutine qualityChangeLocker;
		IEnumerator LockUpdateQualityLevel()
		{
			yield return new WaitForSeconds(0.1f);
			if(_onQualitySettingChanged != null)
				_onQualitySettingChanged();
			qualityChangeLocker = null;
		}
		
		/// <summary>
		/// 렌더링 설정값 불러오기
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public object GetRenderingProperty(string propertyName)
		{
			if(!_editableGraphicAttributeInfosDic.ContainsKey(propertyName))
				return null;

			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("QualitySettings"))
				return typeof(QualitySettings).GetProperty(propertyName).GetValue(null);
			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("URPAsset"))
				return typeof(UniversalRenderPipelineAsset).GetProperty(propertyName).GetValue(_renderPipelineAsset);
			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("Application"))
				return typeof(Application).GetProperty(propertyName).GetValue(null);
			if (_editableGraphicAttributeInfosDic[propertyName]._target.Equals("Rendering_Additional"))
			{
				if (propertyName.Equals("postprocessing"))
					return IsPostProcessingEnabled();
				if (propertyName.Equals("qualityPreset"))
					return QualitySettings.GetQualityLevel();
			}
			return null;
		}

		public bool IsPostProcessingEnabled()
		{
			try
			{
				return (_scriptableRenderers[0] as UniversalRendererData).postProcessData != null;
			}
			catch
			{
				return false;
			}
		}
		
		/// <summary>
		/// 포스트 프로세싱 제어
		/// </summary>
		/// <param name="postProcessEffect">제어 할 이펙트</param>
		/// <param name="enable">On/Off</param>
		public void TryEnablePostProcessing(ePostProcessEffect postProcessEffect, bool enable)
		{
			if (postProcessEffect == ePostProcessEffect.ALL)
			{
				if (_cachePostProcessPerQuality == null)
					_cachePostProcessPerQuality = new Dictionary<int, PostProcessData>();

				// 처음부터 포스트프로세싱 이펙트를 쓰지 않는 퀄리티 레벨이라면 의미없는 지점
				if (!_cachePostProcessPerQuality.ContainsKey(_currentQualityLevel))
				{
					_cachePostProcessPerQuality.Add(_currentQualityLevel,
						(_scriptableRenderers[0] as UniversalRendererData).postProcessData);
				}
				else
				{
					(_scriptableRenderers[0] as UniversalRendererData).postProcessData =
						enable ? _cachePostProcessPerQuality[_currentQualityLevel] : null;
				}
			}
			else
			{
				Type findType = postProcessEffect == ePostProcessEffect.BLOOM ? typeof(Bloom) 
								: postProcessEffect == ePostProcessEffect.VIGNETTE ? typeof(Vignette) 
								: postProcessEffect == ePostProcessEffect.TONEMAPPING ? typeof(Tonemapping) : null;
				
				// 때마다 모든 볼륨을 가져와야 할까?
				Volume[] allVolumesInScene = GameObject.FindObjectsOfType<Volume>(true);
				for (int i = 0; i < allVolumesInScene.Length; i++)
				{
					List<VolumeComponent> volumeComponents = allVolumesInScene[i].profile.components;
					volumeComponents.ForEach(x =>
					{
						if (x.GetType().Equals(findType))
							x.active = enable;
					});
				}
			}
		}
	}
}
