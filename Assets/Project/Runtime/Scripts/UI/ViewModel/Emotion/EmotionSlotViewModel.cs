/*===============================================================
* Product:		Com2Verse
* File Name:	EmotionSlotViewModel.cs
* Developer:	haminjeong
* Date:			2022-10-12 18:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.PlayerControl;
using Com2Verse.Project.Animation;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Emotion")]
	public sealed class EmotionSlotViewModel : ViewModelBase
	{
		private string _shortcutType = string.Empty;
		private string _iconName = string.Empty;
		private Sprite _iconImage = null;

		private Data.Emotion _emotion;
		private int _emotionNumber;

		public CommandHandler IconButtonClick { get; }

		public string IconName
		{
			get => $"/{Localization.Instance.GetString(_emotion?.ChatCommand ?? string.Empty)}";
			set
			{
				_iconName = value;
				base.InvokePropertyValueChanged(nameof(IconName), value);
			}
		}

		public string ShortcutType
		{
			get => _shortcutType;
			set
			{
				_shortcutType = value;
				base.InvokePropertyValueChanged(nameof(ShortcutType), value);
			}
		}

		public Sprite IconImage
		{
			get => _iconImage;
			set
			{
				_iconImage = value;
				base.InvokePropertyValueChanged(nameof(IconImage), value);
			}
		}

		public EmotionSlotViewModel()
		{
			IconButtonClick = new CommandHandler(OnIconButtonClick);
		}

		public void SetSlotIcon(int number, string shortcutName, Data.Emotion emotion)
		{
			_emotion = emotion;

			SetIconImage().Forget();

			IconName = $"/{Localization.Instance.GetString(_emotion.ChatCommand)}";
			ShortcutType = shortcutName;
			_emotionNumber = number;
		}

		private async UniTaskVoid SetIconImage()
		{
			if (!PlayerController.InstanceExists) return;
			if (!await UniTaskHelper.WaitUntil(() => PlayerController.Instance.GestureHelper.IsAtlasDownloaded))
				return;

			IconImage = SpriteAtlasManager.Instance.GetSprite(GestureHelper.UIAtlasName, _emotion.IconName);
		}

		private void OnIconButtonClick()
		{
			if (PlayerController.InstanceExists)
				PlayerController.Instance.GestureHelper?.OnShortcut(_emotionNumber);
		}

		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			base.InvokePropertyValueChanged(nameof(IconName), IconName);
		}
	}
}
