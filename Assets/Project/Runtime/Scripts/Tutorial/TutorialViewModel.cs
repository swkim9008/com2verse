/*===============================================================
* Product:		Com2Verse
* File Name:	ChatBot.cs
* Developer:	ydh
* Date:			2022-12-07 10:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.LruObjectPool;
using Com2Verse.Tutorial;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Office")]
	public sealed class TutorialViewModel : ViewModelBase
	{
		[UsedImplicitly] public CommandHandler CloseBtn { get; }
		[UsedImplicitly] public CommandHandler NextDescBtn { get; }
		[UsedImplicitly] public CommandHandler PrevDescBtn { get; }

		private string _chatBotDesc;

		public string ChatBotDesc
		{
			get => _chatBotDesc;
			set => SetProperty(ref _chatBotDesc, value);
		}

		private bool _isPreviousButtonEnabled;

		public bool IsPreviousButtonEnabled
		{
			get => _isPreviousButtonEnabled;
			set => SetProperty(ref _isPreviousButtonEnabled, value);
		}
		
		private bool _isNextButtonEnabled;

		public bool IsNextButtonEnabled
		{
			get => _isNextButtonEnabled;
			set => SetProperty(ref _isNextButtonEnabled, value);
		}

		private string _tutorialTitle;

		public string TutorialTitle
		{
			get => _tutorialTitle;
			set => SetProperty(ref _tutorialTitle, value);
		}

		private string _pageCount;
		public string PageCount
		{
			get => _pageCount;
			set => SetProperty(ref _pageCount, value);
		}

		private float _tutorialCanvasAlpha;

		public float TutorialCanvasAlpha
		{
			get => _tutorialCanvasAlpha;
			set => SetProperty(ref _tutorialCanvasAlpha, value);
		}
		
		public TutorialViewModel()
		{
			CloseBtn = new CommandHandler(OnClose);
			NextDescBtn = new CommandHandler(OnClickNextBtn);
			PrevDescBtn = new CommandHandler(OnClickPrevBtn);
		}

		private string _image;
		public void SetTextureType(string image)
		{
			if (image == null)
			{
				return;
			}
			
			_image = image;
			LoadTexture().Forget();
		}

		private Texture _tutorialTexture;

		public Texture TutorialTexture
		{
			get => _tutorialTexture;
			set => SetProperty(ref _tutorialTexture, value);
		}

		private async UniTask LoadTexture()
		{
			//var result = await C2VAddressables.LoadAssetAsync<Texture>(_image).ToUniTask();
			//씬 변경시 로드한 어셋 자동 삭제하기 위해 아래 함수로 수정함
			var result = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<Texture>(_image);
			if (!result.IsUnityNull())
			{
				TutorialTexture = result;
			}
		}
		
		private void OnClose()
		{
			TutorialManager.Instance.TutorialClose();
		}
		
		public void OnClickNextBtn()
		{
			TutorialManager.Instance.OnClickNextBtn();
		}

		public void OnClickPrevBtn()
		{
			TutorialManager.Instance.OnClickPrevBtn();
		}
	}
}