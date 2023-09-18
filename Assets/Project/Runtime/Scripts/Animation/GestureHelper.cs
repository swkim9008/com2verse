/*===============================================================
* Product:		Com2Verse
* File Name:	GestureHelper.cs
* Developer:	eugene9721
* Date:			2022-10-05 11:54
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.AvatarAnimation;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Com2Verse.PlayerControl;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Com2Verse.Project.Animation
{
	public sealed class GestureHelper
	{
		public const string UIAtlasName = "Atlas_EmotionUI";
		private static readonly int    GestureRepeatDelay = 100;

		private TableEmotion _tableEmotion;
		public TableEmotion TableEmotion => _tableEmotion;

		private List<Emotion>       _randomList;
		private List<List<Emotion>> _emotionList; // Model

		private EmotionShortCutViewModel _shortCutViewModel;
		private bool                     _isSelectChanging;
		private bool                     _isActioningEmotion;
		private bool                     _isControlPressed;

		private bool _atlasDowndloaded = false;
		public  bool IsAtlasDownloaded   => _emotionList?.Count            > 0 && _atlasDowndloaded;

		private Action         _onUIOpened;
		private Action         _onUIClosed;

		public event Action OnUIOpenedEvent
		{
			add
			{
				_onUIOpened -= value;
				_onUIOpened += value;
			}
			remove => _onUIOpened -= value;
		}

		public event Action OnUIClosedEvent
		{
			add
			{
				_onUIClosed -= value;
				_onUIClosed += value;
			}
			remove => _onUIClosed -= value;
		}

		public GestureHelper()
		{
			_randomList             = new();
			_emotionList            = new();
		}

		public void Initialize()
		{
			LoadTable();
		}

		private void LoadTable(bool withSetSlot = false)
		{
			_tableEmotion = TableDataManager.Instance.Get<TableEmotion>();

			if (withSetSlot && _emotionList?.Count == 0)
				SyncGestureListWithSlot();
		}

		public void Enable()
		{
			_shortCutViewModel = ViewModelManager.Instance.GetOrAdd<EmotionShortCutViewModel>();
			// CheckEmotionList();
			if (!IsAtlasDownloaded)
				SpriteAtlasManager.Instance.LoadSpriteAtlasAsync(UIAtlasName, _ => _atlasDowndloaded = true);
			SyncGestureListWithSlot();
			AddActions();
		}

		public void Disable()
		{
			_shortCutViewModel?.ResetProperties();
			_shortCutViewModel = null;
			_emotionList?.ForEach((slot)=>slot?.Clear());
			_emotionList?.Clear();
			_randomList?.Clear();
			RemoveActions();
		}

		private void SyncGestureListWithSlot()
		{
			if (!User.InstanceExists || User.Instance.CharacterObject.IsUnityNull()) return;
			var avatarController = User.Instance.CharacterObject.AvatarController;
			if (avatarController.IsReferenceNull()) return;

			_shortCutViewModel.GestureSlotCollection.Reset();
			_shortCutViewModel.EmoticonSlotCollection.Reset();
			if (_tableEmotion == null)
			{
				LoadTable(true);
				return;
			}
			var emotionTable = _tableEmotion.Datas.Values.ToList();
			for (int i = 0; i < emotionTable.Count; ++i)
			{
				if (emotionTable[i].EmotionType == eEmotionType.RANDOM_EMOTION)
				{
					_randomList.Add(emotionTable[i]);
					continue;
				}
				var data = emotionTable.Find((emotion) => emotion != null && (emotion.AvatarType == eAvatarType.NONE || avatarController.Info.AvatarType == emotion.AvatarType) &&
				                                          emotion.EmotionType != eEmotionType.RANDOM_EMOTION && emotion.SortOrder == _emotionList.Count + 1);
				if (data == null) continue;
				var slotList = new List<Data.Emotion>();
				slotList.Add(data);
				_emotionList.Add(slotList); // 감정표현 설정 UI 제작 전 임시 처리
			}

			for (int i=0; i<_emotionList.Count; ++i)
			{
				var emotion = _emotionList[i];
				var slotViewModel = new EmotionSlotViewModel();
				if (emotion[0].EmotionType == eEmotionType.GESTURE)
				{
					slotViewModel.SetSlotIcon(i, $"{(i + 1) % 10}", emotion[0]);
					_shortCutViewModel.GestureSlotCollection.AddItem(slotViewModel);
				}
				else
				{
					slotViewModel.SetSlotIcon(i, $"Ctrl+{(i - 9) % 10}", emotion[0]);
					_shortCutViewModel.EmoticonSlotCollection.AddItem(slotViewModel);
				}
			}
		}

#region AddAction

		private void AddActions()
		{
			var actionMapCharacterControl = InputSystemManager.Instance.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl == null)
			{
				C2VDebug.LogError($"[CameraSystem] ActionMap Not Found");
				return;
			}

			actionMapCharacterControl.Shortcut1Action += OnShortcut1Action;
			actionMapCharacterControl.Shortcut2Action += OnShortcut2Action;
			actionMapCharacterControl.Shortcut3Action += OnShortcut3Action;
			actionMapCharacterControl.Shortcut4Action += OnShortcut4Action;
			actionMapCharacterControl.Shortcut5Action += OnShortcut5Action;
			actionMapCharacterControl.Shortcut6Action += OnShortcut6Action;
			actionMapCharacterControl.Shortcut7Action += OnShortcut7Action;
			actionMapCharacterControl.Shortcut8Action += OnShortcut8Action;
			actionMapCharacterControl.Shortcut9Action += OnShortcut9Action;
			actionMapCharacterControl.Shortcut0Action += OnShortcut0Action;
			actionMapCharacterControl.EmotionAction   += OnEmotionAction;
			actionMapCharacterControl.ControlAction   += OnControlButton;
		}

		private void RemoveActions()
		{
			var actionMapCharacterControl = InputSystemManager.InstanceOrNull?.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl == null) return;

			actionMapCharacterControl.Shortcut1Action -= OnShortcut1Action;
			actionMapCharacterControl.Shortcut2Action -= OnShortcut2Action;
			actionMapCharacterControl.Shortcut3Action -= OnShortcut3Action;
			actionMapCharacterControl.Shortcut4Action -= OnShortcut4Action;
			actionMapCharacterControl.Shortcut5Action -= OnShortcut5Action;
			actionMapCharacterControl.Shortcut6Action -= OnShortcut6Action;
			actionMapCharacterControl.Shortcut7Action -= OnShortcut7Action;
			actionMapCharacterControl.Shortcut8Action -= OnShortcut8Action;
			actionMapCharacterControl.Shortcut9Action -= OnShortcut9Action;
			actionMapCharacterControl.Shortcut0Action -= OnShortcut0Action;
			actionMapCharacterControl.EmotionAction   -= OnEmotionAction;
			actionMapCharacterControl.ControlAction   -= OnControlButton;
		}

		private void OnShortcut1Action() => OnShortcut(_isControlPressed ? 10 : 0);
		private void OnShortcut2Action() => OnShortcut(_isControlPressed ? 11 : 1);
		private void OnShortcut3Action() => OnShortcut(_isControlPressed ? 12 : 2);
		private void OnShortcut4Action() => OnShortcut(_isControlPressed ? 13 : 3);
		private void OnShortcut5Action() => OnShortcut(_isControlPressed ? 14 : 4);
		private void OnShortcut6Action() => OnShortcut(_isControlPressed ? 15 : 5);
		private void OnShortcut7Action() => OnShortcut(_isControlPressed ? 16 : 6);
		private void OnShortcut8Action() => OnShortcut(_isControlPressed ? 17 : 7);
		private void OnShortcut9Action() => OnShortcut(_isControlPressed ? 18 : 8);
		private void OnShortcut0Action() => OnShortcut(_isControlPressed ? 19 : 9);

		private void OnEmotionAction()
		{
			if (_shortCutViewModel!.IsUIOn)
				CloseEmotionUI();
			else
				OpenEmotionUI();
		}

#endregion AddAction

		public void OnShortcut(int index)
		{
			if (InputFieldExtensions.IsExistFocused) return;
			if (!User.InstanceExists || PlayerController.Instance.IsReferenceNull()) return;
			if (!PlayerController.Instance.CanEmotion) return;
			if (_isActioningEmotion) return;
			ActionOnEmotion(index).Forget();
		}

		private async UniTaskVoid ActionOnEmotion(int index)
		{
			_isActioningEmotion = true;

			// TODO: 감정표현 설정 UI 확인 후 설정된 제스처 목록 가져오기
			if (_emotionList[index].Count == 1)
			{
				EmotionCommand(_emotionList[index][0]);
			}
			else
			{
				int randomIndex = Random.Range(0, _emotionList[index].Count);
				EmotionCommand(_emotionList[index][randomIndex]);
			}

			await UniTask.Delay(GestureRepeatDelay);
			_isActioningEmotion = false;
		}

		public void EmotionCommand(Data.Emotion emotion)
		{
			// TODO: Commander를 보내기 전 DB에 저장된 감정표현 보유 여부 체크 필요할듯 함(CellState에서 체크시 부하가 큼)
			if (emotion == null) return;

			// 이모션 테이블 구조에 랜덤 범위를 가진 자료형을 발견하면 주어진 범위에서 랜덤을 돌리는 로직
			if (!string.IsNullOrEmpty(emotion.RandomID))
			{
				string[] randomIDs = emotion.RandomID.Split('|');
				if (randomIDs is { Length: > 0 })
				{
					// 주어진 범위의 ID자체를 랜덤 돌리는 방법을 사용
					var index = Random.Range(0, randomIDs.Length);
					if (index >= 0 && index < randomIDs.Length)
					{
						if (int.TryParse(randomIDs[index], out int parseResult))
						{
							var randomEmotion = _randomList.Find(emo => emo.ID == parseResult);
							if (randomEmotion != null)
							{
								EmotionCommand(randomEmotion);
								return;
							}
						}
					}
				}
			}

			var currentAnimationState = User.Instance.CharacterObject.CurrentAnimationState;
			switch (currentAnimationState)
			{
				case eAnimationState.STAND:
					if (emotion.Stand) Commander.Instance.SetEmotion(emotion.ID, User.Instance.CharacterObject.ObjectID);
					break;
				case eAnimationState.SIT:
					if (emotion.Sit) Commander.Instance.SetEmotion(emotion.ID, User.Instance.CharacterObject.ObjectID);
					break;
				case eAnimationState.WALK:
					if (emotion.Walk) Commander.Instance.SetEmotion(emotion.ID, User.Instance.CharacterObject.ObjectID);
					break;
				case eAnimationState.RUN:
					if (emotion.Run) Commander.Instance.SetEmotion(emotion.ID, User.Instance.CharacterObject.ObjectID);
					break;
				case eAnimationState.JUMP:
					if (emotion.Jump) Commander.Instance.SetEmotion(emotion.ID, User.Instance.CharacterObject.ObjectID);
					break;
			}
		}

		private bool CheckEmotionList()
		{
			bool hasRandomGesture = false;
			bool hasEmptyGesture  = false;
			// int idx = 0;
			foreach (var emotion in _emotionList)
			{
				if (emotion.Count >= 2) hasRandomGesture = true;
				if (emotion.Count <= 0) hasEmptyGesture  = true;
			}
			if (!hasRandomGesture)
			{
				C2VDebug.LogWarning($"[{nameof(GestureHelper)}] No random gesture");
				return false;
			}
			if (hasEmptyGesture)
			{
				C2VDebug.LogWarning($"[{nameof(GestureHelper)}] Has Empty gesture");
				return false;
			}
			return true;
		}

		public void CloseEmotionUI()
		{
			if (_shortCutViewModel == null) return;
			if (InputFieldExtensions.IsExistFocused) return;
			UIStackManager.Instance.RemoveByName(nameof(GestureHelper));
			_shortCutViewModel.IsUIOn = false;
			_isSelectChanging = false;
			_onUIClosed?.Invoke();
		}

		public void OpenEmotionUI()
		{
			if (InputFieldExtensions.IsExistFocused) return;
			if (_isSelectChanging) return;
			ChangeOnState().Forget();
		}

		private async UniTaskVoid ChangeOnState()
		{
			if (_shortCutViewModel == null) return;
			_isSelectChanging         = true;
			UIStackManager.Instance.AddByName(nameof(GestureHelper), CloseEmotionUI, eInputSystemState.CHARACTER_CONTROL, true);
			_shortCutViewModel.IsUIOn = true;
			await UniTask.NextFrame();
			_isSelectChanging = false;
			_onUIOpened?.Invoke();
		}

		private void OnControlButton(bool isPressed)
		{
			_isControlPressed = isPressed;
		}
	}
}
