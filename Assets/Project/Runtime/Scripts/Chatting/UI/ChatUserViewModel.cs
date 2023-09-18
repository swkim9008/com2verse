/*===============================================================
* Product:		Com2Verse
* File Name:	ChatUserViewModel.cs
* Developer:	eugene9721
* Date:			2023-06-08 11:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.IO;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
using Com2Verse.Mice;

namespace Com2Verse.UI
{
	// 메세지 폰트 색상 RGB
	// 일반 25 25 27
	// 지역 0 141 90
	// 스피커 140 36 246
	// 운영(스탭, 오퍼레이터) 255 29 146

	/// <summary>
	/// 특정 유저가 채팅 메시지/이모지를 보낸 경우 생성되는 뷰모델
	/// </summary>
	[UsedImplicitly, ViewModelGroup("Chat")]
	public sealed class ChatUserViewModel : InitializableViewModel<MapObject>
	{
		public static ChatUserViewModel Empty { get; } = new(null);

		private const float MaxFontSize    = 16f;
		private const float MediumFontSize = 12f;
		private const float MinFontSize    = 10f;

		private CancellationTokenSource? _closeAnimationCts;

#region Variables
		private int _sortOrder;

		// Gesture
		private Transform? _gestureRootTransform;
		private bool       _isOnGesture;
		private bool       _isOnHandsUp;

		// Chat Balloon
		private Transform? _chatBalloonTransform;
		private bool       _isOnChatBalloon;
		private bool       _isOnChatEmoticon;
		private string     _chatMessage = string.Empty;
		private float      _fontSize    = MinFontSize;
		private bool       _isOnCloseAnimation;

		private ChatCoreBase.eMessageType _messageType;
#endregion Variables

#region Properties
		[UsedImplicitly]
		public int SortOrder
		{
			get => _sortOrder;
			set => SetProperty(ref _sortOrder, value);
		}

		[UsedImplicitly]
		public Transform? GestureRootTransform
		{
			get => _gestureRootTransform;
			set => SetProperty(ref _gestureRootTransform, value!);
		}

		[UsedImplicitly]
		public bool IsOnGesture
		{
			get => _isOnGesture;
			set => SetProperty(ref _isOnGesture, value);
		}

		[UsedImplicitly]
		public bool IsOnHandsUp
		{
			get => _isOnHandsUp;
			set => SetProperty(ref _isOnHandsUp, value);
		}

		[UsedImplicitly]
		public Transform? ChatBalloonTransform
		{
			get => _chatBalloonTransform;
			set => SetProperty(ref _chatBalloonTransform, value!);
		}

		[UsedImplicitly]
		public bool IsOnChatBalloon
		{
			get => _isOnChatBalloon;
			set
			{
				_closeAnimationCts?.Cancel();
				_closeAnimationCts?.Dispose();
				_closeAnimationCts = null;

				_isOnCloseAnimation = false;

				SetProperty(ref _isOnChatBalloon, value);
			}
		}

		[UsedImplicitly]
		public bool IsOnChatEmoticon
		{
			get => _isOnChatEmoticon;
			set => SetProperty(ref _isOnChatEmoticon, value);
		}

		[UsedImplicitly]
		public string ChatMessage
		{
			get => _chatMessage;
			set => SetProperty(ref _chatMessage, value);
		}

		[UsedImplicitly]
		public float FontSize
		{
			get => _fontSize;
			set => SetProperty(ref _fontSize, value);
		}

		[UsedImplicitly]
		public bool IsOnCloseAnimation
		{
			get => _isOnCloseAnimation;
			set => SetProperty(ref _isOnCloseAnimation, value);
		}

		[UsedImplicitly]
		public ChatCoreBase.eMessageType MessageType
		{
			get => _messageType;
			set => SetProperty(ref _messageType, value);
		}

		public float PlayBackTime { get; set; }
#endregion Properties

		public ChatUserViewModel(MapObject? mapObject)
		{
			Initialize(mapObject);

			if (!mapObject.IsUnityNull())
				OnChangeHudCullingState(mapObject, mapObject!.HudCullingState);
		}

		public void DisableChat()
		{
			IsOnChatEmoticon = false;
			IsOnChatBalloon  = false;
			IsOnGesture      = false;
		}

		public override void RefreshViewModel()
		{
			InvokePropertyValueChanged(nameof(GestureRootTransform), GestureRootTransform);
			InvokePropertyValueChanged(nameof(IsOnGesture),          IsOnGesture);
			InvokePropertyValueChanged(nameof(IsOnHandsUp),          IsOnHandsUp);

			InvokePropertyValueChanged(nameof(ChatBalloonTransform), ChatBalloonTransform);
			InvokePropertyValueChanged(nameof(IsOnChatBalloon),      IsOnChatBalloon);
			InvokePropertyValueChanged(nameof(IsOnChatEmoticon),     IsOnChatEmoticon);
			InvokePropertyValueChanged(nameof(ChatMessage),          ChatMessage);
		}

		protected override void OnPrevValueUnassigned(MapObject value)
		{
			value.OnChangeHudCullingState -= OnChangeHudCullingState;
		}

		protected override void OnCurrentValueAssigned(MapObject value)
		{
			value.OnChangeHudCullingState += OnChangeHudCullingState;
		}

		private void OnChangeHudCullingState(MapObject? mapObject, eHudCullingState state)
		{
			switch (state)
			{
				case eHudCullingState.SPEECH_BUBBLE_DISPLAY_MAX:
					FontSize = MaxFontSize;
					break;
				case eHudCullingState.SPEECH_BUBBLE_DISPLAY_MEDIUM:
					FontSize = MediumFontSize;
					break;
				case eHudCullingState.ENABLE_HUD_DISPLAY:
				case eHudCullingState.DISABLE_HUD_DISPLAY:
					FontSize = MinFontSize;
					break;
			}
		}

		public async UniTask PlayCloseAnimation()
		{
			if (_closeAnimationCts != null)
				return;

			_closeAnimationCts = new CancellationTokenSource();
			IsOnCloseAnimation = true;
			await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: _closeAnimationCts.Token);
			IsOnChatBalloon = false;
		}

		public void SetEmotionID(int id)
		{
			if (GestureRootTransform.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(GetType().Name, "GestureRootTransform is null");
				return;
			}

			for (int i = GestureRootTransform!.childCount - 1; i >= 0; --i)
			{
				var child = GestureRootTransform.GetChild(i);
				child.gameObject.SetActive(false);
			}

			if (id == -1)
			{
				IsOnGesture = false;
				return;
			}

			if (!PlayerController.Instance.GestureHelper.TableEmotion.Datas.TryGetValue(id, out var emotion)) return;
			string assetName        = $"{emotion!.ResName}.prefab";
			string objectName       = Path.GetFileNameWithoutExtension(assetName);
			var    emotionTransform = GestureRootTransform.Find(objectName);
			if (emotionTransform.IsUnityNull())
			{
				C2VAddressables.LoadAssetAsync<GameObject>(assetName).OnCompleted += (operationHandle) =>
				{
					var loadedAsset = operationHandle.Result;
					if (loadedAsset.IsUnityNull()) return;
					IsOnGesture           = true;
					emotionTransform      = Object.Instantiate(loadedAsset, GestureRootTransform).transform;
					emotionTransform.name = objectName;
					var spriteAnimation = emotionTransform.GetComponentInChildren<UISpriteAnimation>();
					spriteAnimation.LoopCount = emotion.LoopCount;
					DelayedCloseEmoticon().Forget();
					operationHandle.Release();
				};
			}
			else
			{
				emotionTransform.gameObject.SetActive(true);
				IsOnGesture = true;
				DelayedCloseEmoticon().Forget();
			}

			if (!string.IsNullOrEmpty(emotion.SoundName))
				if (Value is ActiveObject activeObject && !activeObject.AnimatorController.IsUnityNull())
					activeObject.AnimatorController!.PlaySound($"{emotion.SoundName}.wav");
		}

		private async UniTask DelayedCloseEmoticon()
		{
			var elapsedTime = 0f;
			while (IsOnChatEmoticon && IsOnGesture && !Value.IsUnityNull())
			{
				elapsedTime += Time.deltaTime;
				if (elapsedTime > GeneralData.General!.EmoticonDisplayTime)
					break;
				await UniTask.Yield();
			}
			IsOnGesture = false;
		}

		public void UpdateChatBalloonSize(float distance)
		{
			if (ChatBalloonTransform.IsUnityNull()) return;
			ChatBalloonTransform!.localScale = Vector3.one;
		}
	}
}
