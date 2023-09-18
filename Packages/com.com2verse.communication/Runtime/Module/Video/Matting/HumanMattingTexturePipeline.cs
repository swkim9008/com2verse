/*===============================================================
* Product:		Com2Verse
* File Name:	HumanMattingTexturePipeline.cs
* Developer:	urun4m0r1
* Date:			2022-07-11 17:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Threading;
using com.com2verse.humanmattingLight;
using Com2Verse.Communication.Unity;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication.Matting
{
	public sealed class HumanMattingTexturePipeline : IVideoTexturePipeline, IDisposable
	{
#region IVideoTexturePipeline
		private IVideoTextureProvider? _target;

		public IVideoTextureProvider? Target
		{
			get => _target;
			set
			{
				var prevValue = _target;
				if (prevValue == value)
					return;

				_target = value;

				if (prevValue != null)
					prevValue.TextureChanged -= OnTargetTextureChanged;

				if (_target != null)
					_target.TextureChanged += OnTargetTextureChanged;

				UpdateProcessState();
			}
		}

		private void OnTargetTextureChanged(Texture? texture)
		{
			UpdateProcessState();
		}
#endregion // IVideoTexturePipeline

#region IModule
		public event Action<bool>? StateChanged;

		private bool _isRunning;

		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				var prevValue = _isRunning;
				if (prevValue == value)
					return;

				_isRunning = value;
				StateChanged?.Invoke(value);

				UpdateProcessState();
			}
		}
#endregion // IModule

#region IVideoTextureProvider
		public event Action<Texture?>? TextureChanged;

		private Texture? _texture;

		public Texture? Texture
		{
			get => _texture;
			private set
			{
				var prevValue = _texture;
				if (prevValue == value)
					return;

				_texture = value;
				TextureChanged?.Invoke(value);
			}
		}
#endregion // IVideoTextureProvider

		/// <summary>
		/// 배경 제거 기능 업데이트 초당 프레임 수 (-1인 경우 원본 비디오 Fps)
		/// </summary>
		public int RefreshFps { get; set; } = -1;

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		private Texture2D? _background;

		private HumanMatting?            _predictor;
		private BackgroundProcessorData? _processorData;
		private CancellationTokenSource? _tokenSource;

		public Texture2D? Background
		{
			get => _background;
			set
			{
				var prevValue = _background;
				if (prevValue == value)
					return;

				_background = value;
				_processorData?.ChangeBackgroundTextureAsync(value, _tokenSource).Forget();
			}
		}

		private void UpdateProcessState()
		{
			var texture = Target?.Texture;
			if (IsRunning && !texture.IsUnityNull())
			{
				StopProcessLoop();

				_tokenSource = new CancellationTokenSource();
				StartProcessLoopAsync(texture!).Forget();
			}
			else
			{
				Texture = texture;

				StopProcessLoop();
			}
		}

		private static async UniTask<HumanMatting?> TryCreateModelAsync()
		{
			var resourceHandler = Resources.LoadAsync("C2VRVM_light");
			if (resourceHandler == null)
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to load model");
				return null;
			}

			var predictor    = new HumanMatting();
			var model        = await resourceHandler.ToUniTask();
			var modelHandler = predictor.CreateModelAsync(model);
			if (modelHandler == null)
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to create model");
				predictor.Dispose();
				return null;
			}

			if (!await modelHandler.AsUniTask())
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to create model");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			if (!predictor.CreatePredictor())
			{
				C2VDebug.LogErrorMethod(nameof(NoiseReductionAudioPipeline), "Failed to create predictor");
				modelHandler.Dispose();
				predictor.Dispose();
				return null;
			}

			return predictor;
		}

		private async UniTask StartProcessLoopAsync(Texture source)
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;

			var predictor = await TryCreateModelAsync();
			_predictor = predictor;

			if (predictor == null || source.IsUnityNull())
			{
				IsInitializing = false;
				StopProcessLoop();
				return;
			}

			var processorData = new BackgroundProcessorData(source.width, source.height);
			await processorData.ChangeBackgroundTextureAsync(Background, _tokenSource);
			_processorData = processorData;

			IsInitializing = false;
			IsInitialized  = true;

			do
			{
				await processorData.UpdateTargetTextureAsync(predictor, source, _tokenSource);
				Texture = processorData.Target;
			}
			while (await UniTaskHelper.Delay(GetRefreshIntervalMs(), _tokenSource));
		}

		private int GetRefreshIntervalMs()
		{
			var fps = RefreshFps <= 0 ? ModuleManager.Instance.CameraSettings.Fps : RefreshFps;
			return MathUtil.ToMilliseconds(1f / fps);
		}

		private void StopProcessLoop()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;

			_processorData?.Dispose();
			_processorData = null;

			_predictor?.Dispose();
			_predictor = null;

			IsInitializing = false;
			IsInitialized  = false;
		}

		public void Dispose()
		{
			StopProcessLoop();
		}

		~HumanMattingTexturePipeline()
		{
			Dispose();
		}
	}
}
