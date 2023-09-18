/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_CharacterMove.cs
* Developer:	haminjeong
* Date:			2022-05-16 15:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
#region CharacterMove
        public void MoveCommand(Vector2 position)
        {
            if (!User.Instance.Standby) return;
            Protocols.GameMechanic.MoveCommand moveCommand = new()
            {
                ObjectId = User.Instance.CurrentUserData.ObjectID,
                Turn     = (int)position.x,
                Forward  = (int)position.y
            };
            NetworkManager.Instance.Send(moveCommand, Protocols.GameMechanic.MessageTypes.MoveCommand);
        }
                   
        public void MoveTo(Vector3 position)
        {
            if (!User.Instance.Standby) return;
            Protocols.GameMechanic.MoveTo moveTo = new()
            {
                ObjectId = User.Instance.CurrentUserData.ObjectID,
                Position = new()
                {
                    X = (int)(position.x * 100f),
                    Y = (int)(position.y * 100f),
                    Z = (int)(position.z * 100f)
                }
            };
            NetworkManager.Instance.Send (moveTo, Protocols.GameMechanic.MessageTypes.MoveTo);
        }

        public void MoveAction(bool sprint, bool jump)
        {
            if (!User.Instance.Standby) return;

            Protocols.GameMechanic.MoveAction moveAction = new()
            {
                ObjectId = User.Instance.CurrentUserData.ObjectID,
                IsRun    = sprint,
                IsJump   = jump
            };

            C2VDebug.Log($"Move Action: run {sprint} jump {jump}");
            NetworkManager.Instance.Send(moveAction, Protocols.GameMechanic.MessageTypes.MoveAction);
        }

        public void SetAnimatorID(int id)
        {
            if (!User.Instance.Standby) return;

            Protocols.ObjectCondition.SetAnimatorNotify setAnimatorID = new()
            {
                ObjectId   = User.Instance.CurrentUserData.ObjectID,
                AnimatorId = id
            };
            
            C2VDebug.Log($"SetAnimatorID: {id}");
            NetworkManager.Instance.Send(setAnimatorID, Protocols.ObjectCondition.MessageTypes.SetAnimatorNotify);
        }

        public void SetEmotion(int emotion, long targetID)
        {
            if (!User.Instance.Standby) return;

            var emotionState = new Protocols.EmotionState();
            emotionState.EmotionId = emotion;
            emotionState.EmotionTarget = targetID;
            
            Protocols.ObjectCondition.SetEmotionNotify setEmotion = new()
            {
                ObjectId     = User.Instance.CurrentUserData.ObjectID,
                EmotionState = emotionState
            };

            C2VDebug.Log($"SetEmotion: id {emotion} target {targetID}");
            NetworkManager.Instance.Send(setEmotion, Protocols.ObjectCondition.MessageTypes.SetEmotionNotify);
        }

        private Protocols.GameMechanic.UpdateCameraView _updateCameraView;
        public void UpdateCameraView(int angleIndex)
        {
            if (!User.Instance.Standby) return;

            if (_updateCameraView == null)
                _updateCameraView = new Protocols.GameMechanic.UpdateCameraView
                {
                    OwnerId   = User.Instance.CurrentUserData.ID,
                    ViewIndex = angleIndex
                };
            else
                _updateCameraView.ViewIndex = angleIndex;

            // C2VDebug.Log($"UpdateCameraView: angle id {angleIndex}");
            NetworkManager.Instance.Send(_updateCameraView, Protocols.GameMechanic.MessageTypes.UpdateCameraView);
        }

        public void MoveObjectCompleted()
        {
            Protocols.GameLogic.UpdateMovedObjectCompleted movedObjectCompleted = new()
            {
                AccountId = User.Instance.CurrentUserData.ID
            };
            C2VDebug.Log($"Send UpdateMovedObjectCompleted");
            NetworkManager.Instance.Send(movedObjectCompleted, Protocols.GameLogic.MessageTypes.UpdateMovedObjectCompleted);
        }

        public void TeleportActionCompletionNotify()
        {
            Protocols.WorldState.TeleportActionCompletionNotify teleportActionCompletionNotify = new()
            {
                AccountId = User.Instance.CurrentUserData.ID
            };
            C2VDebug.Log($"Send TeleportActionCompletionNotify");
            NetworkManager.Instance.Send(teleportActionCompletionNotify, Protocols.WorldState.MessageTypes.TeleportActionCompletionNotify);
        }
#endregion

#region Teleport
        public void TeleportUserRequest(long mapId, long fieldId)
        {
            //if (!User.Instance.Standby) return;
            Protocols.GameLogic.TeleportUserRequest teleportUserRequest = new()
            {
                // MapId = mapId,
                FieldId = (int)fieldId
            };
            C2VDebug.Log($"Sending TeleportUserRequest. FieldId : {fieldId}, mapId : {mapId}");
            User.Instance.DiscardPacketBeforeStandBy();
            NetworkUIManager.Instance.TargetFieldID = fieldId;
            NetworkManager.Instance.Send(teleportUserRequest, Protocols.GameLogic.MessageTypes.TeleportUserRequest);
        }

        public void TeleportUserSpaceRequest(string spaceId)
        {
            //if (!User.Instance.Standby) return;
            Protocols.GameLogic.TeleportUserSpaceRequest teleportUserSpaceRequest = new()
            {
                SpaceId = spaceId
            };

            User.Instance.DiscardPacketBeforeStandBy();
            NetworkUIManager.Instance.TargetFieldID = -1;
            NetworkManager.Instance.Send(teleportUserSpaceRequest,
                                         Protocols.GameLogic.MessageTypes.TeleportUserSpaceRequest,
                                         Protocols.Channels.WorldState,
                                         (int)Protocols.WorldState.MessageTypes.TeleportUserStartNotify,
                                         timeoutAction: () =>
                                         {
                                             PlayerController.Instance.SetStopAndCannotMove(false);
                                             User.Instance.RestoreStandBy();
                                             UIManager.Instance.SendToastMessage(Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError), toastMessageType: UIManager.eToastMessageType.WARNING);
                                         });
        }

        public event Action OnEscapeEvent;
        public void EscapeUserRequest()
        {
            OnEscapeEvent?.Invoke();
            OnEscapeEvent = null;
            Protocols.CommonLogic.EscapeUserRequest escapeUserRequest = new();
            C2VDebug.Log($"Sending EscapeUserRequest.");
            NetworkManager.Instance.Send(escapeUserRequest, Protocols.CommonLogic.MessageTypes.EscapeUserRequest);
            EscapeNotifyToServer();
        }

        private void EscapeNotifyToServer()
        {
            Protocols.GameLogic.EscapeToWorldNotify escapeToWorldNotify = new();
            C2VDebug.Log($"Sending EscapeToWorldNotify.");
            NetworkManager.Instance.Send(escapeToWorldNotify, Protocols.GameLogic.MessageTypes.EscapeToWorldNotify);
        }

        public void EscapeAcceptableCheckRequest()
        {
            Protocols.CommonLogic.EscapeAcceptableCheckRequest escapeAcceptableCheckRequest = new();
            C2VDebug.Log($"Sending EscapeAcceptableCheckRequest.");
            NetworkManager.Instance.Send(escapeAcceptableCheckRequest, Protocols.CommonLogic.MessageTypes.EscapeAcceptableCheckRequest);
        }

        public void RequestWarpPosition(long id)
        {
            Protocols.CommonLogic.WarpPositionRequest warpPositionRequest = new()
            {
                TargetId = id,
            };
            C2VDebug.Log($"Sending WarpPositionRequest. WarpId : {id}");
            PlayerController.Instance.SetStopAndCannotMove(true);
            User.Instance.DiscardPacketBeforeStandBy();
            NetworkManager.Instance.Send(warpPositionRequest, Protocols.CommonLogic.MessageTypes.WarpPositionRequest);
        }

        /// <summary>
        /// 클라이언트에서 State 서버로 직접 호출하는 것은 원칙상 맞지 않으므로 테스트 코드로만 사용
        /// </summary>
        /// <param name="fieldId"></param>
        public void WarpUserRequest(long fieldId)
        {
            Protocols.WorldState.WarpUserRequest warpUserRequest = new()
            {
                AccountId = User.Instance.CurrentUserData.ID,
                FieldId   = fieldId,
            };
            C2VDebug.Log($"Sending WarpUserRequest. FieldId : {fieldId}");
            User.Instance.DiscardPacketBeforeStandBy();
            NetworkUIManager.Instance.TargetFieldID = fieldId;
            NetworkManager.Instance.Send(warpUserRequest, Protocols.WorldState.MessageTypes.WarpUserRequest);
        }
#endregion
	}
}
