/*===============================================================
* Product:		Com2Verse
* File Name:	PrizeDrawingMachineObject_Process.cs
* Developer:	sprite
* Date:			2023-07-11 17:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Com2Verse.Mice;
using System.Threading;
using Com2Verse.CameraSystem;
using Com2Verse.UI;
using Com2Verse.Extension;
using TMPro;
using UnityEngine.Playables;
using Cinemachine;
using UnityEngine.Timeline;

namespace Com2Verse.Mice.NamedLoggerTag
{
    public class Sprite : LoggerTag<Sprite>
    {
        public static bool IsEnable = true;
    }
}

namespace Com2Verse.Network
{
    public sealed partial class GachaMachineObject   // Drawing and Result Process
        : INamedLogger<Mice.NamedLoggerTag.Sprite>
    {
        [SerializeField] private Animator _capsuleToyBoxAnimator;
        [SerializeField] private FixedCameraJig _fixedCameraJig;
        [SerializeField] private Button _buttonPrizeList;
        [SerializeField] private Button _buttonPlay;
        [SerializeField] private TMP_Text _textCount;

        [SerializeField] private PlayableDirector _playableDirector;

        private FixedCameraJig _lastActiveCameraJig;
        private CancellationTokenSource _ctsPickup;


        partial void PartialPickupProcessAwake()
        {
            _buttonPrizeList.onClick.AddListener(this.OnPrizeListButtonClick);
            _buttonPlay.onClick.AddListener(()=>this.OnPlayButtonClick().Forget());

            // 초기화한다.
            _playableDirector.time = 0;
            _playableDirector.Evaluate();

        }

        partial void PartialPickupProcessDestroy()
        {
            _buttonPrizeList.onClick.RemoveAllListeners();

            this.CancelProcess();
        }

        void UpdateCountUI()
        {
            _textCount.text = _isActiveMode ? $"{this.gachaInfo.MyTryCount}" : "";
        }

        private void OnPrizeListButtonClick()
        {
            if (!_isActiveMode) return;

            Sound.SoundManager.Instance.PlayUISound("SE_MICE_Gacha_InfoButton.wav");

            // 현재 뽑기 진행중이면 닫지 않는다.
            if (_ctsPickup != null) { return; }

            async UniTask OnAsyncPrizeListButtonClick()
            {
                // 데이타 동기화
                await SyncPrizeInfo(_prizeDrawingEventKey);

                await MiceUIGachaMachineGiftInfoViewModel.ShowView(this.prizeInfo);
            }

            OnAsyncPrizeListButtonClick().Forget();

            
        }


        private async UniTask OnPlayButtonClick()
        {
            if (!_isActiveMode) return;

            Sound.SoundManager.Instance.PlayUISound("SE_MICE_Gacha_ClickButton.wav");

            // 연출 진행
            if (_ctsPickup != null)
            {
                this.Log("Already Processing!");
                return;
            }

            // 이전 상품의 배송정보를 보내지 않았다면
            if (this.resultPrizeInfo.PersonalInfoNeeded)
            {
                UIManager.Instance.ShowPopupConfirm(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_IncompleteDrawing.ToLocalizationString(),
                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_IncompleteDrawing.ToLocalizationString(), () =>
                    {
                        MiceUIPrizeDrawingMachineInputInfoViewModel.ShowView(this.resultPrizeInfo).Forget();
                    });
                return;
            }

            // 남은 뽑기 횟수 체크
            if (!this.gachaInfo.HasTryCount)
            {
                UIManager.Instance.ShowPopupConfirm(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_NoCountsLeft.ToLocalizationString(),
                                                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_NoCountsLeft.ToLocalizationString());
                return;
            }

            // 가차 가능 시간 체크
            if (!this.gachaInfo.IsInPlayTime())
            {
                UIManager.Instance.ShowPopupConfirm(Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Title_UsingTime.ToLocalizationString(),
                                                    Data.Localization.eKey.MICE_UI_CapsuleDrawingMachine_Popup_Msg_UsingTime.ToLocalizationString());
                return;
            }

            CancellationToken token;

            try
            {
                _ctsPickup = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                token = _ctsPickup.Token;

                // 서버 결과 받기
                var result = await this.SendPrizeTry(_prizeDrawingEventKey, token);

                if (result)
                {
                    // 티켓 소모
                    this.gachaInfo.UseTicket();
                    // UI갱신
                    UpdateCountUI();

                    // 캡슐 뽑기 연출...
                    await this.DrawingProcess(token);

                    // 결과창
                    MiceUIPrizeDrawingMachineResultViewModel.ShowView(this.resultPrizeInfo).Forget();
                }
            }
            finally
            {
                _ctsPickup?.Dispose();
                _ctsPickup = null;
            }
        }

        private void CancelProcess()
            => _ctsPickup?.Cancel();

        private async UniTask DrawingProcess(CancellationToken cancellationToken = default)
        {
            // 초기화한다.
            _playableDirector.time = 0;
            _playableDirector.Evaluate();

            try
            {
                // 카메라 바인딩
                var camera = CameraManager.Instance.MainCamera;
                _playableDirector.SetGameObjectBinding("Cinemachine Track", camera.gameObject);

                _playableDirector.Play();
                while (_playableDirector.state == PlayState.Playing)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                }
            }
            finally
            {
                // 카메라 바인딩 해제
                _playableDirector.RemoveBinding("Cinemachine Track");

                // 초기화한다.
                _playableDirector.time = 0;
                _playableDirector.Evaluate();
            }
        }
    }
}


