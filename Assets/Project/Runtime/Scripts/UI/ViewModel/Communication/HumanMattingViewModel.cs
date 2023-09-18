/*===============================================================
 * Product:		Com2Verse
 * File Name:	HumanMattingViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-28 10:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using Com2Verse.Extension;
using Com2Verse.Option;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class HumanMattingViewModel : ViewModelBase, IDisposable
	{
		// TODO: 좀 더 우아한 방법으로 수정 필요
		private readonly List<HumanMattingBackgroundData?> _backgroundData = new()
		{
			null,
			new("UI_BackgroundEffect_01_SD.png", "UI_BackgroundEffect_01_HD.png"),
			new("UI_BackgroundEffect_02_SD.png", "UI_BackgroundEffect_02_HD.png"),
			new("UI_BackgroundEffect_03_SD.png", "UI_BackgroundEffect_03_HD.png"),
			new("UI_BackgroundEffect_04_SD.png", "UI_BackgroundEffect_04_HD.png"),
			new("UI_BackgroundEffect_05_SD.png", "UI_BackgroundEffect_05_HD.png"),
			new("UI_BackgroundEffect_06_SD.png", "UI_BackgroundEffect_06_HD.png"),
		};

		[UsedImplicitly] public CommandHandler<int> SelectBackgroundCommand { get; }

		private int _selectedBackgroundIndex;

		public HumanMattingViewModel()
		{
			var option = OptionController.Instance.GetOption<DeviceOption>();
			if (option != null)
			{
				SelectedBackgroundIndex = option.HumanMattingBackgroundIndex;
			}

			SelectBackgroundCommand = new CommandHandler<int>(index => SelectedBackgroundIndex = index);

			ModuleManager.Instance.CameraSettings.SettingsChanged += OnCameraSettingsChanged;
		}

		private void OnCameraSettingsChanged(IReadOnlyVideoSettings _)
		{
			UpdateBackground();
		}

		public int SelectedBackgroundIndex
		{
			get => _selectedBackgroundIndex;
			set
			{
				var previousIndex = _selectedBackgroundIndex;
				if (previousIndex == value)
					return;

				if (value < 0 || value >= _backgroundData.Count)
					return;

				_selectedBackgroundIndex = value;
				UpdateBackground();

				_backgroundData[previousIndex]?.UnloadBackgroundTexture();

				var option = OptionController.Instance.GetOption<DeviceOption>();
				if (option != null)
				{
					option.HumanMattingBackgroundIndex = value;
					option.SaveData();
				}

				InvokePropertyValueChanged(nameof(IsBackgroundIndex0), IsBackgroundIndex0);
				InvokePropertyValueChanged(nameof(IsBackgroundIndex1), IsBackgroundIndex1);
				InvokePropertyValueChanged(nameof(IsBackgroundIndex2), IsBackgroundIndex2);
				InvokePropertyValueChanged(nameof(IsBackgroundIndex3), IsBackgroundIndex3);
				InvokePropertyValueChanged(nameof(IsBackgroundIndex4), IsBackgroundIndex4);
				InvokePropertyValueChanged(nameof(IsBackgroundIndex5), IsBackgroundIndex5);
				InvokePropertyValueChanged(nameof(IsBackgroundIndex6), IsBackgroundIndex6);
			}
		}

		private void UpdateBackground()
		{
			ChangeBackgroundAsync(_backgroundData[SelectedBackgroundIndex]).Forget();
		}

		private async UniTask ChangeBackgroundAsync(HumanMattingBackgroundData? backgroundData)
		{
			if (backgroundData == null)
			{
				ModuleManager.Instance.HumanMattingTexturePipeline.Background = null;
				ModuleManager.Instance.HumanMattingTexturePipeline.IsRunning  = false;
				return;
			}

			var texture = ModuleManager.Instance.CameraSettings.Height >= Utils.Define.Matting.HdTextureHeightThreshold
				? await backgroundData.LoadHdBackgroundTextureAsync()
				: await backgroundData.LoadSdBackgroundTextureAsync();

			ModuleManager.Instance.HumanMattingTexturePipeline.Background = texture;
			ModuleManager.Instance.HumanMattingTexturePipeline.IsRunning  = !texture.IsUnityNull();
		}

		public void Dispose()
		{
			foreach (var backgroundData in _backgroundData)
			{
				backgroundData?.Dispose();
			}
		}

#region ViewModelProperty
		[UsedImplicitly] public bool IsBackgroundIndex0 => SelectedBackgroundIndex == 0;
		[UsedImplicitly] public bool IsBackgroundIndex1 => SelectedBackgroundIndex == 1;
		[UsedImplicitly] public bool IsBackgroundIndex2 => SelectedBackgroundIndex == 2;
		[UsedImplicitly] public bool IsBackgroundIndex3 => SelectedBackgroundIndex == 3;
		[UsedImplicitly] public bool IsBackgroundIndex4 => SelectedBackgroundIndex == 4;
		[UsedImplicitly] public bool IsBackgroundIndex5 => SelectedBackgroundIndex == 5;
		[UsedImplicitly] public bool IsBackgroundIndex6 => SelectedBackgroundIndex == 6;
#endregion // ViewModelProperty
	}
}
