/*===============================================================
* Product:		Com2Verse
* File Name:	NpcDialogueViewer.cs
* Developer:	eugene9721
* Date:			2023-09-06 13:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Contents
{
	public sealed class NpcDialogueViewer
	{
		private readonly string[] _ignoreControlViewList = { "UI_Avatar_Hud_Group" };

		private const string DialoguePopupResName     = "UI_Npc_Dialogue";
		private const string PrevDialoguePopupResName = "UI_Npc_Dialogue_Popup";

		private ActiveObjectManagerViewModel.eDisplayType _prevAvatarHudType = ActiveObjectManagerViewModel.eDisplayType.ALL;

		private readonly HashSet<long> _currentDialogueMemberList = new();

		private string _dialogueString    = string.Empty;
		private string _currentViewString = string.Empty;

		private NpcDialogueViewModel?    _viewModel;
		private CancellationTokenSource? _dialogueMessageCts;

		/// <summary>
		/// 현재 출력중인 대화 UI의 GUIView
		/// </summary>
		private GUIView? _lastDialogueView;

		/// <summary>
		/// 현재 출력중인 이전 대화 UI의 GUIView
		/// </summary>
		private GUIView? _lastPrevDialogueView;

		// TODO: TableData
		private float _npcSpeechSpeed     = 0.1f;
		private float _npcSpeechSpeedSkip = 0.02f;

		public bool IsSkip { get; set; }

#region Dialogue Ui
		private void PrewarmDialogue()
		{
			IsSkip = false;
			StopCurrentMessage();
		}

		public void StartDialogue(bool isAiNpc, long npcId)
		{
			if (!_lastDialogueView.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Already started dialogue");
				return;
			}
			PrewarmDialogue();

			_currentDialogueMemberList.Add(npcId);

			_viewModel = ViewModelManager.Instance.GetOrAdd<NpcDialogueViewModel>();
			_viewModel.Enable();
			_viewModel.IsAiNpc = isAiNpc;
			_viewModel.ClearPrevDialogue();

			UIStackManager.Instance.RemoveAll();
			UIManager.Instance.CreatePopup(DialoguePopupResName, (guiView) =>
			{
				guiView.Show();
				_lastDialogueView = guiView;

				UIManager.Instance.SetGuiViewActive(false, _ignoreControlViewList);
				UIManager.Instance.SetPopupLayerGuiViewActive(false, true, guiView);

				SetDialogueAvatarHudOption();
			}).Forget();
		}

		public void ExitDialogue()
		{
			UIManager.Instance.SetGuiViewActive(true, _ignoreControlViewList);
			if (!_lastDialogueView.IsUnityNull())
			{
				_lastDialogueView!.Hide();
				UIManager.Instance.SetPopupLayerGuiViewActive(true, false, _lastDialogueView);
			}
			_lastDialogueView = null;

			if (!_lastPrevDialogueView.IsUnityNull())
				_lastPrevDialogueView!.Hide();
			_lastPrevDialogueView = null;

			_viewModel?.Disable();
			_currentDialogueMemberList.Clear();
			SetPrevAvatarHudOption();
		}

		public void SetDialogueString(string dialogueString)
		{
			if (_lastDialogueView.IsReferenceNull())
				C2VDebug.LogCategory(GetType().Name, "Not started dialogue");

			PrewarmDialogue();

			_dialogueMessageCts = new CancellationTokenSource();
			_dialogueString     = dialogueString;
			_currentViewString  = string.Empty;

			_viewModel?.AddPrevDialogue("Npc이름", dialogueString, true);

			ShowDialogue(_dialogueMessageCts).Forget();
		}

		private async UniTask ShowDialogue(CancellationTokenSource cts)
		{
			while (_currentViewString.Length < _dialogueString.Length)
			{
				if (_viewModel == null)
					break;

				var delay    = IsSkip ? _npcSpeechSpeedSkip : _npcSpeechSpeed;
				var numChars = Mathf.Max((int)(Time.deltaTime / delay), 1);
				while (numChars > 0)
				{
					numChars--;
					if (_currentViewString.Length >= _dialogueString.Length) break;

					_currentViewString = $"{_currentViewString}{_dialogueString[_currentViewString.Length]}";
				}

				_viewModel.NpcDialogue = _currentViewString;

				await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token);
			}
		}

		private void StopCurrentMessage()
		{
			_dialogueMessageCts?.Cancel();
			_dialogueMessageCts?.Dispose();
			_dialogueMessageCts = null;
		}
#endregion Dialogue Ui

#region AvatarHud
		private void SetDialogueAvatarHudOption()
		{
			var viewModel = ViewModelManager.InstanceOrNull?.Get<ActiveObjectManagerViewModel>();
			if (viewModel == null)
				return;

			_prevAvatarHudType            = viewModel.DisplayType;
			viewModel.IsCustomDisplayFunc = IsDialogueAvatarDisplayFunc;
			viewModel.DisplayType         = ActiveObjectManagerViewModel.eDisplayType.CUSTOM;
		}

		private bool IsDialogueAvatarDisplayFunc(ActiveObject activeObject)
		{
			if (activeObject.IsUnityNull())
				return false;

			if (activeObject.IsMine)
				return true;

			return _currentDialogueMemberList.Contains(activeObject.OwnerID);
		}

		private void SetPrevAvatarHudOption()
		{
			var viewModel = ViewModelManager.InstanceOrNull?.Get<ActiveObjectManagerViewModel>();
			if (viewModel == null)
				return;

			viewModel.DisplayType         = _prevAvatarHudType;
			viewModel.IsCustomDisplayFunc = null;
		}
#endregion AvatarHud

#region Previous Dialogue
		public void ShowPrevDialogue()
		{
			UIManager.Instance.CreatePopup(PrevDialoguePopupResName, (guiView) =>
			{
				guiView.Show();
				_lastPrevDialogueView = guiView;
			}).Forget();
		}

		public void HidePrevDialogue()
		{
			if (!_lastPrevDialogueView.IsUnityNull())
				_lastPrevDialogueView!.Hide();
			_lastPrevDialogueView = null;
		}
#endregion Previous Dialogue

		// TODO: 테이블 데이터 값 이용, 매개변수 타입 변경
		public void SetData(float speed, float skipSpeedMultiple)
		{
			_npcSpeechSpeed     = speed;
			_npcSpeechSpeedSkip = speed / skipSpeedMultiple;
		}
	}
}
