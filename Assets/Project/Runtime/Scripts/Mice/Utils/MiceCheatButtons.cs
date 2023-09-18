/*===============================================================
* Product:		Com2Verse
* File Name:	MiceCheatButtons.cs
* Developer:	sprite
* Date:			2023-08-29 10:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Network;
using Com2Verse.UI;
using System;
using System.Reflection;
using Protocols.CommonLogic;
using Com2Verse.PlayerControl;
using Com2Verse.EventTrigger;
using Protocols.GameLogic;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR || ENABLE_CHEATING
namespace Com2Verse.Mice
{
	public sealed class MiceCheatButtons : MonoBehaviour
	{
        private GUIView _guiView;
        private GUIStyle _styleButton;

        private void OnEnable()
        {
            this.TryGetComponent(out _guiView);
        }

        private void OnGUI()
        {
            if
            (
                _guiView == null || !_guiView ||
                (_guiView.VisibleState != GUIView.eVisibleState.OPENING && _guiView.VisibleState != GUIView.eVisibleState.OPENED)
            )
            {
                return;
            }

            if (_styleButton == null)
            {
                _styleButton = new GUIStyle(GUI.skin.button);
                _styleButton.fontSize = 20;
                _styleButton.fontStyle = FontStyle.Bold;
                _styleButton.alignment = TextAnchor.MiddleCenter;
                _styleButton.wordWrap = true;
            }

            using (new GUILayout.AreaScope(new Rect(10, 10, 1900, 1060)))
            using (new GUILayout.VerticalScope())
            {
                if (!MiceService.InstanceExists || !MiceService.Instance.ServiceEnabled)
                {
                    this.DrawCheatButton("Start Mice Service", "StartMiceService");
                }

                if (MiceService.InstanceExists)
                {
                    if (MiceService.Instance.ServiceEnabled)
                    {
                        this.DrawCheatButton("Stop Mice Service", "StopMiceService");
                    }

                    if (MiceService.Instance.CurrentAreaType == eMiceAreaType.LOUNGE || MiceService.Instance.CurrentAreaType == eMiceAreaType.FREE_LOUNGE)
                    {
                        GUILayout.Space(20);
                        this.DrawCheatButton("Enter Mice Lobby", "EnterMiceLobby");
                        this.DrawCheatButton("Show Nearest Kiosk", this.OnShowNearestKiosk);
                    }

                    if (MiceService.Instance.ServiceEnabled)
                    {
                        GUILayout.Space(20);
                        this.DrawCheatButton("Escape", this.OnEscapeButtonClicked);
                    }
                }


                if (!MiceService.InstanceExists || !MiceService.Instance.ServiceEnabled)
                {
                    GUILayout.FlexibleSpace();

                    this.DrawCheatButton("PostCode Test", "TestPostCodeWebView");
                    this.DrawCheatButton("Normal PL", () => MiceParticipantListManager.Instance.ShowAsNormal());
                    this.DrawCheatButton("Conference PL", () => MiceParticipantListManager.Instance.ShowAsConference());

                    GUILayout.Space(100);
                }
            }
        }

        private void DrawCheatButton(string caption, Action onClick)
        {
            if (GUILayout.Button(caption, _styleButton, GUILayout.Width(220), GUILayout.Height(60)) && onClick != null)
            {
                onClick.Invoke();
            }
        }

        private void DrawCheatButton(string caption, string cheatMethod, params object[] parameters)
            => this.DrawCheatButton(caption, () => this.CallCheatMethod(cheatMethod, parameters));

        private void CallCheatMethod(string methodName, params object[] parameters)
        {
            var type = typeof(Com2Verse.Cheat.CheatKey);
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod);
            method?.Invoke(null, parameters);
        }

        private void OnEscapeButtonClicked()
        {
            Network.CommonLogic.PacketReceiver.Instance.EscapeAcceptableCheckResponse += OnEscapeAcceptableCheckResponse;
            Commander.Instance.EscapeAcceptableCheckRequest();
        }

        private void OnEscapeAcceptableCheckResponse(EscapeAcceptableCheckResponse response)
        {
            Network.CommonLogic.PacketReceiver.Instance.EscapeAcceptableCheckResponse -= OnEscapeAcceptableCheckResponse;
            if (response.IsSuccess)
                GoToEscapeProcess();
            else
                UIManager.Instance.ShowPopupYesNo(UI.Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
                                                  UI.Localization.Instance.GetString("UI_AccessDelay_Notice_Popup_Desc"),
                                                  _ => GoToEscapeProcess(),
                                                  yes: UI.Localization.Instance.GetString("UI_Common_Btn_Move"),
                                                  no: UI.Localization.Instance.GetString("UI_Common_Btn_Cancel"));
        }

        private void GoToEscapeProcess()
        {
            PlayerController.Instance.SetStopAndCannotMove(true);
            User.Instance.DiscardPacketBeforeStandBy();
            Commander.Instance.EscapeUserRequest();
        }

        private void OnShowNearestKiosk()
        {
#region 메인 카메라로부터 가장 가까운 KioskObject를 찾는다.
            var basePos = Camera.main.transform.position;

            KioskObject kiosk = null;

            float minDist = float.MaxValue;
            var kiosks = GameObject.FindObjectsOfType<KioskObject>();
            for (int i = 0, cnt = kiosks?.Length ?? 0; i < cnt; i++)
            {
                var item = kiosks[i];
                if (item == null || !item) continue;
                var sqrDist = (item.transform.position - basePos).sqrMagnitude;
                if (minDist > sqrDist)
                {
                    kiosk = kiosks[i];
                    minDist = sqrDist;
                }
            }
#endregion // 메인 카메라로부터 가장 가까운 KioskObject를 찾는다.

            if (kiosk != null && kiosk)
            {
                MiceService.Instance.ShowKioskMenu(kiosk).Forget();
            }
            else
            {
                "적절한 키오스크를 찾을 수 없습니다.".ShowAsToast();
            }
        }
    }
}
#endif
