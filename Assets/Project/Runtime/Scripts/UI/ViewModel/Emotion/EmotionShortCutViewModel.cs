/*===============================================================
* Product:		Com2Verse
* File Name:	EmotionShortCutViewModel.cs
* Developer:	haminjeong
* Date:			2022-10-12 17:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.PlayerControl;

namespace Com2Verse.UI
{
	[ViewModelGroup("Emotion")]
	public sealed class EmotionShortCutViewModel : ViewModelBase
	{
		private Collection<EmotionSlotViewModel> _emoticonSlotCollection = new();
		private Collection<EmotionSlotViewModel> _gestureSlotCollection  = new();
		private string                           _targetName             = string.Empty;
		private bool                             _isUIOn                 = false;
		private bool                             _activeGesture          = true;
		private bool                             _activeEmoticon         = false;

		public long TargetID { get; private set; } = 0;

		public CommandHandler SettingButtonClick { get; }
		public CommandHandler CloseButtonClick   { get; }
		public CommandHandler GestureTabButtonClick   { get; }
		public CommandHandler EmoticonTabButtonClick   { get; }

		public string TargetName
		{
			get => _targetName;
			set
			{
				_targetName = value;
				base.InvokePropertyValueChanged(nameof(TargetName), value);
			}
		}

		public bool IsUIOn
		{
			get => _isUIOn;
			set
			{
				_isUIOn = value;
				base.InvokePropertyValueChanged(nameof(IsUIOn), value);
			}
		}

		public bool ActiveGesture
		{
			get => _activeGesture;
			set
			{
				_activeGesture = value;
				base.InvokePropertyValueChanged(nameof(ActiveGesture), value);
			}
		}

		public bool ActiveEmoticon
		{
			get => _activeEmoticon;
			set
			{
				_activeEmoticon = value;
				base.InvokePropertyValueChanged(nameof(ActiveEmoticon), value);
			}
		}

		public Collection<EmotionSlotViewModel> EmoticonSlotCollection
		{
			get => _emoticonSlotCollection;
			set
			{
				_emoticonSlotCollection = value;
				base.InvokePropertyValueChanged(nameof(EmoticonSlotCollection), value);
			}
		}

		public Collection<EmotionSlotViewModel> GestureSlotCollection
		{
			get => _gestureSlotCollection;
			set
			{
				_gestureSlotCollection = value;
				base.InvokePropertyValueChanged(nameof(GestureSlotCollection), value);
			}
		}

		public EmotionShortCutViewModel()
		{
			SettingButtonClick     = new CommandHandler(OnSettingButtonClick);
			CloseButtonClick       = new CommandHandler(OnCloseButtonClick);
			GestureTabButtonClick  = new CommandHandler(OnGestureTabButtonClick);
			EmoticonTabButtonClick = new CommandHandler(OnEmoticonTabButtonClick);
		}

		public void ResetProperties()
		{
			_emoticonSlotCollection.DestroyAll();
			_gestureSlotCollection.DestroyAll();
		}

		private void OnSettingButtonClick()
		{
			// TODO: 단축키 설정 UI
		}

		private void OnCloseButtonClick()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
				playerController!.GestureHelper.CloseEmotionUI();
		}

		private void OnGestureTabButtonClick()
		{
			if (ActiveGesture) return;
			ActiveGesture  = true;
			ActiveEmoticon = false;
		}

		private void OnEmoticonTabButtonClick()
		{
			if (ActiveEmoticon) return;
			ActiveGesture  = false;
			ActiveEmoticon = true;
		}

		public void SetTarget(long id, string name)
		{
			TargetID = id;
			TargetName = name;
		}
	}
}
