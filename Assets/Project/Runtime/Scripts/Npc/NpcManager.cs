/*===============================================================
* Product:		Com2Verse
* File Name:	NpcManager.cs
* Developer:	eugene9721
* Date:			2023-09-04 19:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.Project.InputSystem;
using JetBrains.Annotations;
using User = Com2Verse.Network.User;

namespace Com2Verse.Contents
{
	public sealed class NpcManager : Singleton<NpcManager>
	{
		private readonly NpcDialogueViewer _dialogueViewer = new();
		private readonly NpcDialogueCamera _dialogueCamera = new();

		[UsedImplicitly]
		private NpcManager() { }

		public void Initialize()
		{
			LoadTable();
			_dialogueCamera.Initialize();
		}

		private void LoadTable() { }

#region DialogueViewer
		// TODO: NpcInfo를 받아와서 셋팅해주는 버전 추가
		// - Npc 이름 추가 필요
		// - Npc ID(혹은 baseMapObject 객체) 추가 필요
		// - 현재 대화 그룹 멤버 추가 필요
		public void StartDialogue(bool isAiNpc)
		{
			const int testNpcId = 542;
			_dialogueViewer.StartDialogue(isAiNpc, testNpcId);
			SetCharacterRotationToNpc(testNpcId);
			SetCamera(testNpcId);
			InputSystemManagerHelper.ChangeState(eInputSystemState.UI);
		}

		private void SetCamera(long npcId)
		{
			var character     = User.InstanceOrNull?.CharacterObject;
			var mapController = MapController.InstanceOrNull;

			if (character.IsUnityNull() || mapController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "mapController or character is null");
				return;
			}

			var npc = mapController!.GetObjectByUserID(npcId) as ActiveObject;
			if (npc.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "npc is null");
				return;
			}

			_dialogueCamera.SetCamera(NpcDialogueCamera.eCameraTarget.DEFAULT, character!, npc!);
		}

		public void ExitDialogue()
		{
			InputSystemManagerHelper.ChangeState(eInputSystemState.CHARACTER_CONTROL);
			_dialogueViewer.ExitDialogue();
			SetCharacterCamera();
		}

		private void SetCharacterCamera()
		{
			_dialogueCamera.SetCharacterCamera();
		}

		private void SetCharacterRotationToNpc(long npcId)
		{
			var character     = User.InstanceOrNull?.CharacterObject;
			var mapController = MapController.InstanceOrNull;

			if (character.IsUnityNull() || mapController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "mapController or character is null");
				return;
			}

			var npc = mapController!.GetObjectByUserID(npcId);
			if (npc.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "npc is null");
				return;
			}

			var characterTransform = character!.transform;
			characterTransform.LookAt(npc!.transform);
			npc.transform.LookAt(characterTransform);
			// MapController에서 포지션 제어시 회전값을 적용하기 위함
			character.ForceSetCurrentPositionToState();
			npc.ForceSetCurrentPositionToState();

			// PlayerController 기반 이동시 회전값을 적용하기 위함
			PlayerController.Instance.Teleport(characterTransform.position, characterTransform.rotation);
		}

		public void SetDialogueString(string dialogueString)
		{
			_dialogueViewer.SetDialogueString(dialogueString);
		}

		public void SetIsSkip()
		{
			_dialogueViewer.IsSkip = true;
		}

		public void SetData(float speed, float speedSkip)
		{
			_dialogueViewer.SetData(speed, speedSkip);
		}

		public void SetData(float cameraDistance, float forceRotateYawThreshold, float pitchThreshold, float heightRatio, float distanceRatio, float inCameraBlendTime, float outCameraBlendTime)
		{
			_dialogueCamera.SetData(cameraDistance, forceRotateYawThreshold, pitchThreshold, heightRatio, distanceRatio, inCameraBlendTime, outCameraBlendTime);
		}

		public void ShowPrevDialogue()
		{
			_dialogueViewer.ShowPrevDialogue();
		}

		public void HidePrevDialogue()
		{
			_dialogueViewer.HidePrevDialogue();
		}
#endregion DialogueViewer
	}
}
