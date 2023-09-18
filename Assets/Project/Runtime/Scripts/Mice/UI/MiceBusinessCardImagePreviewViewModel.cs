/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardImageEditViewModel.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public class MiceBusinessCardImagePreviewViewModel : ViewModelBase
	{
		public static readonly string ResName   = "UI_Popup_BusinessCard_ImagePreview";

		public enum eLayoutType
		{
			MATCH,
			FILL,
			EXPAND,
		}

#region Variables
		private Texture2D                    _image;
		private eLayoutType                  _layoutType;
		private AspectRatioFitter.AspectMode _currentAspectMode;
		private float                        _currentAspectRatio;
		private AspectRatioFitter.AspectMode _matchAspectMode;
		private float                        _matchAspectRatio;
		private AspectRatioFitter.AspectMode _fillAspectMode;
		private float                        _fillAspectRatio;
		private AspectRatioFitter.AspectMode _expandAspectMode;
		private float                        _expandAspectRatio;
		private bool                         _isImageApplying;
		public  CommandHandler               SelectMatch  { get; }
		public  CommandHandler               SelectFill   { get; }
		public  CommandHandler               SelectExpand { get; }
		public  CommandHandler               Apply        { get; }

		public Action<Texture2D> OnImageSelected { get; set; }
#endregion

#region Properties
		public Texture2D Image
		{
			get => _image;
			set => SetProperty(ref _image, value);
		}

		public eLayoutType LayoutType
		{
			get => _layoutType;
			set => SetProperty(ref _layoutType, value);
		}

		public AspectRatioFitter.AspectMode CurrentAspectMode
		{
			get => _currentAspectMode;
			set => SetProperty(ref _currentAspectMode, value);
		}

		public float CurrentAspectRatio
		{
			get => _currentAspectRatio;
			set => SetProperty(ref _currentAspectRatio, value);
		}

		public AspectRatioFitter.AspectMode MatchAspectMode
		{
			get => _matchAspectMode;
			set => SetProperty(ref _matchAspectMode, value);
		}

		public float MatchAspectRatio
		{
			get => _matchAspectRatio;
			set => SetProperty(ref _matchAspectRatio, value);
		}

		public AspectRatioFitter.AspectMode FillAspectMode
		{
			get => _fillAspectMode;
			set => SetProperty(ref _fillAspectMode, value);
		}

		public float FillAspectRatio
		{
			get => _fillAspectRatio;
			set => SetProperty(ref _fillAspectRatio, value);
		}

		public AspectRatioFitter.AspectMode ExpandAspectMode
		{
			get => _expandAspectMode;
			set => SetProperty(ref _expandAspectMode, value);
		}

		public float ExpandAspectRatio
		{
			get => _expandAspectRatio;
			set => SetProperty(ref _expandAspectRatio, value);
		}
#endregion

#region Initialize
		public MiceBusinessCardImagePreviewViewModel()
		{
			SelectMatch      = new CommandHandler(OnSelectMatch);
			SelectFill       = new CommandHandler(OnSelectFill);
			SelectExpand     = new CommandHandler(OnSelectExpand);
			Apply            = new CommandHandler(OnApply);
			LayoutType       = eLayoutType.MATCH;
			_isImageApplying = false;
		}
#endregion

#region Binding Events
		private void OnSelectMatch()
		{
			LayoutType = eLayoutType.MATCH;
			RefreshImageLayout();
		}

		private void OnSelectFill()
		{
			LayoutType = eLayoutType.FILL;
			RefreshImageLayout();
		}

		private void OnSelectExpand()
		{
			LayoutType = eLayoutType.EXPAND;
			RefreshImageLayout();
		}

		private void OnApply()
		{
			if (_isImageApplying) return;
			ApplyImage().Forget();
		}

		private async UniTask ApplyImage()
		{
			_isImageApplying = true;
			var captureLayoutType = MiceUICapture.eLayoutType.DEFAULT;
			switch (LayoutType)
			{
				case eLayoutType.MATCH:
					captureLayoutType = MiceUICapture.eLayoutType.DEFAULT;
					break;
				case eLayoutType.FILL:
					captureLayoutType = MiceUICapture.eLayoutType.FILL;
					break;
				case eLayoutType.EXPAND:
					captureLayoutType = MiceUICapture.eLayoutType.EXPAND;
					break;
			}

			var generatedImage = await MiceUICapture.Capture(Image, MiceUserInfo.ImageSize, MiceUserInfo.ImageSize, captureLayoutType);
			OnImageSelected?.Invoke(generatedImage);
			_isImageApplying = false;
		}
#endregion

		private void RefreshImageLayout()
		{
			if (Image == null) return;
			var width  = (float)Image.width;
			var height = (float)Image.height;
			FillAspectMode  = AspectRatioFitter.AspectMode.FitInParent;
			FillAspectRatio = 1.0f;
			
			MatchAspectMode   = AspectRatioFitter.AspectMode.FitInParent;
            MatchAspectRatio  = width / height;

            ExpandAspectMode  = AspectRatioFitter.AspectMode.FitInParent;
            ExpandAspectRatio = 1.0f;
            if (width > height)
			{
				ExpandAspectMode  = AspectRatioFitter.AspectMode.HeightControlsWidth;
				ExpandAspectRatio = width / height;
			}
			else
			{
				ExpandAspectMode  = AspectRatioFitter.AspectMode.WidthControlsHeight;
				ExpandAspectRatio = width / height;
			}

			CurrentAspectMode  = FillAspectMode;
			CurrentAspectRatio = FillAspectRatio;
			switch (LayoutType)
			{
				case eLayoutType.MATCH:
					CurrentAspectMode = MatchAspectMode;
					CurrentAspectRatio = MatchAspectRatio;
					break;
				case eLayoutType.FILL:
					break;
				case eLayoutType.EXPAND:
					CurrentAspectMode  = ExpandAspectMode;
					CurrentAspectRatio = ExpandAspectRatio;
					break;
			}
		}

		public void SetImage(string path)
		{
			var bytes = System.IO.File.ReadAllBytes(path);
			// LoadImage will replace with with incoming image size.
			var texture = new Texture2D(2, 2);
			texture.LoadImage(bytes);
			Image = texture;
			RefreshImageLayout();
		}

		public static void ShowView(string path, Action<Texture2D> onImageSelected)
		{
			UIManager.Instance.CreatePopup(ResName, (guiView) =>
			{
				guiView.Show();

				var viewModel = guiView.ViewModelContainer.GetViewModel<MiceBusinessCardImagePreviewViewModel>();
				viewModel.SetImage(path);
				viewModel.OnImageSelected = onImageSelected;
			}).Forget();
		}
	}
}
