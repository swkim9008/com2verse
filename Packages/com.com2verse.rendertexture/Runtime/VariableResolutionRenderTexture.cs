/*===============================================================
* Product:    Com2Verse
* File Name:  VariableResolutionRenderTexture.cs
* Developer:  eugene9721
* Date:       2022-05-02 11:17
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UIExtension
{
	/// <summary>
	/// Change the size of RenderTexture when the window size changes
	/// just support Target Type: RawImage, Source Type: Camera
	/// just support 1:1 matching between Target And Source
	/// </summary>
	public sealed class VariableResolutionRenderTexture : MonoBehaviour
	{
		[SerializeField] private RawImage _targetRawImage;
		[SerializeField] private Camera   _sourceCamera;

		[SerializeField] private int _windowSizeChangeCriteria         = 50;
		[SerializeField] private int _windowSizeChangeEndFrameCriteria = 5;

		private RenderTexture _renderTexture;
		private bool          _isChanged;
		private int           _prevWindowWidth;
		private int           _prevWindowHeight;
		private int           _frameChecker;

		private float _widthRatio;
		private float _heightRatio;
		
		private Action<RenderTexture> _renderTextureResized;

		public event Action<RenderTexture> RenderTextureResized
		{
			add
			{
				_renderTextureResized -= value;
				_renderTextureResized += value;
			}
			remove => _renderTextureResized -= value;
		}

		private void Update()
		{
			// If the window size has been maintained for n frames since it was changed
			if (_isChanged && _prevWindowWidth == Screen.width && _prevWindowHeight == Screen.height)
			{
				_frameChecker++;
			}
			else if (_isChanged)
			{
				_frameChecker     = 0;
				_prevWindowWidth  = Screen.width;
				_prevWindowHeight = Screen.height;
			}

			if (_frameChecker >= _windowSizeChangeEndFrameCriteria)
				RenderTextureResolutionChange();
		}

		private void OnEnable()
		{
			if (_renderTexture.IsReferenceNull()) return;
			if (!_sourceCamera.IsReferenceNull())
				_sourceCamera.targetTexture = _renderTexture;
			
			if (!Mathf.Approximately(_widthRatio * Screen.width, _renderTexture.width) ||
			    !Mathf.Approximately(_widthRatio * Screen.height, _renderTexture.height))
				RenderTextureResolutionChange();
		}

		private void OnDisable()
		{
			_sourceCamera.targetTexture = null;
		}

		public void Initialize(RenderTexture renderTexture, Camera sourceCamera, RawImage targetRawImage)
		{
			_sourceCamera   = sourceCamera;
			_targetRawImage = targetRawImage;
			_renderTexture  = renderTexture;

			if (_renderTexture is { width: > 0, height: > 0 })
			{
				_widthRatio  = (float)_renderTexture.width  / Screen.width;
				_heightRatio = (float)_renderTexture.height / Screen.height;
			}

			_sourceCamera.targetTexture = _renderTexture;
			_targetRawImage.texture     = _renderTexture;

			ScreenSize.Instance.ScreenResized -= ScreenSizeChangeEvent;
			ScreenSize.Instance.ScreenResized += ScreenSizeChangeEvent;
		}

		private void RenderTextureResolutionChange()
		{
			// Discard existing RenderTexture
			_renderTexture.DiscardContents();
			_renderTexture.Release();
			Destroy(_renderTexture);

			// Create New RenderTexture for change resolution
			_renderTexture =
				RenderTextureHelper.CreateRenderTexture(
					_renderTexture.format,
					Convert.ToInt32(_widthRatio  * Screen.width),
					Convert.ToInt32(_heightRatio * Screen.height)
				);

			_sourceCamera.targetTexture = _renderTexture;
			_targetRawImage.texture     = _renderTexture;
			_renderTexture.Create();
			
			_renderTextureResized?.Invoke(_renderTexture);

			_isChanged    = false;
			_frameChecker = 0;
		}

		private void ScreenSizeChangeEvent(int width, int height)
		{
			int widthDifference  = Mathf.Abs(_prevWindowWidth  - width);
			int heightDifference = Mathf.Abs(_prevWindowHeight - height);

			// if the window size has changed more than a criteria size
			if (widthDifference > _windowSizeChangeCriteria || heightDifference > _windowSizeChangeCriteria)
			{
				_isChanged        = true;
				_prevWindowWidth  = Screen.width;
				_prevWindowHeight = Screen.height;
			}
		}

		private bool HasRequiredComponents(out string requiredComponentName)
		{
			bool hasTargetRawImage = _targetRawImage != null;
			bool hasSourceCamera   = _sourceCamera   != null;

			if (hasTargetRawImage && hasSourceCamera)
			{
				requiredComponentName = "TargetRawImage and SourceCamera";
				return false;
			}

			if (hasTargetRawImage)
			{
				requiredComponentName = "TargetRawImage";
				return false;
			}

			if (hasSourceCamera)
			{
				requiredComponentName = "SourceCamera";
				return false;
			}

			requiredComponentName = string.Empty;
			return true;
		}
	}
}
