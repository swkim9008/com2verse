/*===============================================================
* Product:		Com2Verse
* File Name:	PrizeDrawingMachineObject.cs
* Developer:	sprite
* Date:			2023-07-10 16:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.Data;
using Com2Verse.Mice;
using Com2Verse.EventTrigger;
using Com2Verse.CameraSystem;
using Com2Verse.Interaction;
using Com2Verse.UI;

namespace Com2Verse.Network
{
    [Serializable]
    public sealed class GachaMachineTag
    {
        public long PrizeDrawingEventKey;
    }

    public sealed partial class GachaMachineObject : MonoBehaviour
    {
        private Dictionary<string, object> _tags = new();

        private TriggerInEventParameter _triggerEvent;

        private long _prizeDrawingEventKey = 1;
        private bool _isActiveMode = false;

        partial void PartialPickupProcessAwake();
        partial void PartialPickupProcessDestroy();

        private void Awake()
        {
            this.PartialPickupProcessAwake();

            UpdateCountUI();
        }

        private void OnDestroy()
        {
            this.PartialPickupProcessDestroy();
        }

        private GachaMachineTag AddOrUpadteTagValue(string json)
        {
            bool result = true;
            GachaMachineTag tagValue = null;
            try
            {
                tagValue = JsonUtility.FromJson<GachaMachineTag>(json);
            }
            catch (Exception e)
            {
                C2VDebug.LogError(e);

                result = false;
            }

            if (result)
            {
                var typeName = nameof(GachaMachineTag);
                if (_tags.ContainsKey(typeName))
                {
                    _tags[typeName] = tagValue;
                }
                else
                {
                    _tags.Add(typeName, tagValue);
                }
            }

            return tagValue;
        }

        public void InitTagValueFromJson(string json)
        {
            var tagValue = this.AddOrUpadteTagValue(json);

            // 서버와 동기화 한다.
            _prizeDrawingEventKey = tagValue.PrizeDrawingEventKey;
        }


        public async UniTask StartInteraction(TriggerInEventParameter triggerInParameter)
        {
            C2VDebug.LogMethod(GetType().Name);
            _triggerEvent = triggerInParameter;

            // 데이타 동기화
            var result = await SyncGachaInfo(_prizeDrawingEventKey);
            if (!result) return;

            var target = triggerInParameter?.ParentMapObject.transform ?? this.transform;
            var vCam = target.GetComponentInChildren<FixedCameraJig>();
            CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA);
            FixedCameraManager.Instance.SwitchCamera(vCam);

            if(triggerInParameter != null)
            {
                InteractionManager.Instance.UnsetInteractionUI(triggerInParameter.SourceTrigger, triggerInParameter.CallbackIndex);
            }

            // UI를 감춘다.ss
            MiceService.Instance.SetUserInteractionState(eMiceUserInteractionState.WithWorldObject);

            // 툴바에 있던 UI들을 닫는다.
            ViewModelManager.Instance.Get<MiceToolBarViewModel>()?.CloseAllUI();

            await UIPopupExitServerObjectViewModel.ShowView(this.gameObject);

            _isActiveMode = true;

            UpdateCountUI();

            // 당첨 후 배송 정보를 입력하지 않았을 경우
            if (this.resultPrizeInfo.PersonalInfoNeeded)
            {
                UIManager.Instance.ShowPopupConfirm(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_IncompleteDrawing.ToLocalizationString(),
                                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_IncompleteDrawing.ToLocalizationString(),
                                    () => {
                                        MiceUIPrizeDrawingMachineInputInfoViewModel.ShowView(this.resultPrizeInfo).Forget();
                                    });
                return;
            }
        }


        public bool OnCloseAction()
        {
            // 현재 뽑기 진행중이면 닫지 않는다.
            if (_ctsPickup != null) {return false;}

            MiceService.Instance.SetUserInteractionState(eMiceUserInteractionState.None);
            CameraManager.Instance.ChangeState(eCameraState.FOLLOW_CAMERA);

            if (_triggerEvent != null)
            {
                InteractionManager.Instance.SetInteractionUI(_triggerEvent.SourceTrigger, _triggerEvent.CallbackIndex, () =>
                {
                    StartInteraction(_triggerEvent).Forget();
                });
            }

            _isActiveMode = false;

            UpdateCountUI();

            return true;
        }


    }
}

