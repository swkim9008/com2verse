/*===============================================================
* Product:		Com2Verse
* File Name:	NpcDialogueViewModel.cs
* Developer:	eugene9721
* Date:			2023-09-05 14:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Chat;
using Com2Verse.Contents;
using Com2Verse.Extension;
using Com2Verse.Network;
using UnityEngine;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("Npc")]
	public sealed class NpcDialogueViewModel : ViewModelBase
	{
#region Fieldss
		private string _npcName     = string.Empty;
		private string _npcDialogue = string.Empty;

		private bool _isOnAiLoading;
		private bool _isOnAiAnswer;

		private bool _isAiNpc;

		private Collection<NpcPrevDialogueViewModel> _prevDialogues = new();

		private StackRegisterer? _prevDialogueStackRegisterer;
#endregion Fields

#region Command Properties
		[UsedImplicitly] public CommandHandler<string> ClickQuestionButton { get; }

		[UsedImplicitly] public CommandHandler ExitButtonClicked             { get; }
		[UsedImplicitly] public CommandHandler StopAiAnswerButtonClicked     { get; }
		[UsedImplicitly] public CommandHandler ShowPrevDialogueButtonClicked { get; }
		[UsedImplicitly] public CommandHandler ExitPrevDialogueButtonClicked { get; }

		[UsedImplicitly]
		public StackRegisterer? PrevDialogueStackRegisterer
		{
			get => _prevDialogueStackRegisterer;
			set
			{
				_prevDialogueStackRegisterer = value;
				if (value.IsUnityNull()) return;
				_prevDialogueStackRegisterer!.WantsToQuit += OnPrevDialogueBackButtonClick;
			}
		}
#endregion Command Properties

#region Properties
		[UsedImplicitly]
		public string NpcName
		{
			get => _npcName;
			set => SetProperty(ref _npcName, value);
		}

		/// <summary>
		/// 현재 UI에 출력될 Npc의 대사 내용
		/// </summary>
		[UsedImplicitly]
		public string NpcDialogue
		{
			get => _npcDialogue;
			set => SetProperty(ref _npcDialogue, value);
		}

		/// <summary>
		/// 질문 입력 후 AI 답변을 기다리는 중인지 여부
		/// 질문 전송 이후 AI 서버에서 응답이 오기 전까지 질문 입력 영역은 비활성화 된다.
		/// </summary>
		[UsedImplicitly]
		public bool IsOnAiLoading
		{
			get => _isOnAiLoading;
			set
			{
				SetProperty(ref _isOnAiLoading, value);
				if (value) IsOnAiAnswer = false;
			}
		}

		/// <summary>
		/// AI 답변이 출력중인지 여부
		/// AI 서버에서 회신 된 답변이 출력될 때, 중지 버튼이 활성화 된다.
		/// </summary>
		[UsedImplicitly]
		public bool IsOnAiAnswer
		{
			get => _isOnAiAnswer;
			set
			{
				SetProperty(ref _isOnAiAnswer, value);
				if (value) IsOnAiLoading = false;
			}
		}

		[UsedImplicitly]
		public bool IsAiNpc
		{
			get => _isAiNpc;
			set => SetProperty(ref _isAiNpc, value);
		}

		[UsedImplicitly]
		public Collection<NpcPrevDialogueViewModel> PrevDialogues
		{
			get => _prevDialogues;
			set => SetProperty(ref _prevDialogues, value);
		}
#endregion Properties

#region Initialize
		public NpcDialogueViewModel()
		{
			ClickQuestionButton = new CommandHandler<string>(OnClickQuestionButton);

			ExitButtonClicked             = new CommandHandler(OnExitButtonClicked);
			StopAiAnswerButtonClicked     = new CommandHandler(OnStopAiAnswerButtonClicked);
			ShowPrevDialogueButtonClicked = new CommandHandler(OnShowPrevDialogueButtonClicked);
			ExitPrevDialogueButtonClicked = new CommandHandler(OnExitPrevDialogueButtonClicked);
		}

		public void Enable()
		{
			NpcName     = string.Empty;
			NpcDialogue = string.Empty;
		}

		public void Disable()
		{
			ClearPrevDialogue();
		}
#endregion Initialize

#region CommandHandler Callbacks
		public void OnClickQuestionButton(string question)
		{
			var user = User.InstanceOrNull;
			if (user == null) return;

			var character = user.CharacterObject;
			if (!character.IsUnityNull())
				ChatManager.InstanceOrNull?.CreateSpeechBubble(character!, question, ChatCoreBase.eMessageType.AREA);

			var userName = user.CurrentUserData.UserName ?? string.Empty;
			AddPrevDialogue(userName, question, false);
		}

		private void OnExitButtonClicked()
		{
			NpcManager.InstanceOrNull?.ExitDialogue();
		}

		private void OnStopAiAnswerButtonClicked()
		{
			Debug.Log("OnStopAiAnswerButtonClickedHandler");
		}

		private void OnShowPrevDialogueButtonClicked()
		{
			NpcManager.InstanceOrNull?.ShowPrevDialogue();
		}

		private void OnExitPrevDialogueButtonClicked()
		{
			NpcManager.InstanceOrNull?.HidePrevDialogue();
		}

		private void OnPrevDialogueBackButtonClick()
		{
			NpcManager.InstanceOrNull?.HidePrevDialogue();
		}
#endregion CommandHandler Callbacks

#region PrevDialouge
		/// <summary>
		/// 이전 대화 내용을 확인하기 위1해 내용을 저장하는 메서드.<br/>
		/// NpcDialogueViewer에서 데이터가 오는 경우 -> isNpc = true<br/>
		/// OnClickQuestionButton에서 데이터가 오는 경우 -> isNpc = false
		/// </summary>
		/// <param name="name">메시지를 보낸 대상의 이름</param>
		/// <param name="message">주고 받은 메시지 내용</param>
		/// <param name="isNpc">해당 메시지가 Npc의 답변인지</param>
		public void AddPrevDialogue(string name, string message, bool isNpc)
		{
			var item = new NpcPrevDialogueViewModel
			{
				Name         = name,
				Message      = message,
				IsNpcMessage = isNpc,
			};

			PrevDialogues.AddItem(item);
		}

		public void ClearPrevDialogue()
		{
			PrevDialogues.Reset();
		}
#endregion PrevDialouge
	}
}
