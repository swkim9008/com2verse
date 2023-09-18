/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadManager.cs
* Developer:	tlghks1009
* Date:			2022-08-23 10:29
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Protocols;

/*
 * MyPad ID 테이블 -
 * https://docs.google.com/spreadsheets/d/13yOmgejXRR_WMS3wUP1LGFVs-2Hiq3ml3eQWMFJQDmI/edit#gid=53286131
 * 사용법 -
 * https://jira.com2us.com/wiki/pages/viewpage.action?pageId=300549812
*/

namespace Com2Verse.UI
{
    public sealed class MyPadManager : Singleton<MyPadManager>, IDisposable
    {
        public enum eBasicApp
        {
            SETTING,
            CHATBOT,
            AVATAR
        }

        public enum eErrorPopup
        {
            INVALID_SERVICE,
            DUPLICATED,
            NEED_CLEAR_ALL
        }

        private readonly Dictionary<string, BaseMyPadAction> _myPadActionDict = new();

        private TableMyPad _tableOfMyPad;
        private List<MyPadAdvertisement> _listOfMyPadAdv = new();

        private CancellationTokenSource _timeCancellationTokenSource;
        private GUIView _myPadPopup;
        private MyPadViewModel _myPadViewModel;

        private bool _isOpen = false;
        public  bool IsOpen => _isOpen;

        /// <summary>
        /// Singleton Instance Creation
        /// </summary>
        [UsedImplicitly] private MyPadManager()
        {
            RegisterMyPadAction();
        }

        public void Dispose()
        {
            _myPadActionDict.Clear();
            _tableOfMyPad = null;

            if (_timeCancellationTokenSource != null)
            {
                _timeCancellationTokenSource.Cancel();
                _timeCancellationTokenSource = null;
            }

            if (!_myPadPopup.IsReferenceNull())
            {
                _myPadPopup.OnDestroyedEvent -= OnMyPadPopupClosed;
                _myPadPopup.OnClosedEvent -= OnMyPadPopupClosed;
            }
            _myPadPopup = null;
        }

#region MyPad
        public void CheckOpenMyPad()
        {
            C2VDebug.LogCategory("MyPad", $"Open MyPad");
            _onMyPadOpened?.Invoke();
        }

        public void CheckCloseMyPad()
        {
            if (_myPadPopup == null) return;
            if (_myPadPopup.VisibleState == GUIView.eVisibleState.CLOSED) return;

            C2VDebug.LogCategory("MyPad", $"Close MyPad");
            //SetActionMap();
            _myPadPopup.Hide();
        }

        public void OpenMyPad()
        {
            ResetTimer();
            UIManager.Instance.CreatePopup("UI_Popup_MyPad", (guiView) =>
            {
                _myPadPopup = guiView;

                _myPadPopup.OnDestroyedEvent += OnMyPadPopupClosed;
                _myPadPopup.OnClosedEvent += OnMyPadPopupClosed;

                _isOpen = true;
                _myPadPopup.Show();

                _myPadViewModel = MasterViewModel.Get<MyPadViewModel>();
                Mice.RedDotManager.SetTrigger(Mice.RedDotManager.RedDotData.TriggerKey.ShowMyPad);

                OnTimeUpdate().Forget();
            }).Forget();
        }

        private void OnMyPadPopupClosed(GUIView guiView)
        {
            guiView.OnClosedEvent -= OnMyPadPopupClosed;
            guiView.OnDestroyedEvent -= OnMyPadPopupClosed;

            _isOpen = false;
            _myPadPopup = null;
            _onMyPadClosed?.Invoke();

            ResetTimer();
        }
#endregion // MyPad

#region MyPadItem
        public void PlayAction(eBasicApp target) => PlayAction(GetLayoutMyPadItemList()[(int)target].AppID);
        public void PlayAction(string identifier)
        {
            if (!GetInfoFromIdentifier(identifier, out var targetApp, out var targetName, out var canUse))
            {
                // TableData가 정상적이지 않은 경우의 처리
                return;
            }

            if (!canUse)
            {
                // 현재 지역에서 사용 불가능한 앱 처리
                ShowErrorPopup(eErrorPopup.INVALID_SERVICE);
            }
            else if (!targetApp.IsStack)
            {
                // 스택으로 쌓을 수 없는 앱 실행
                // ShowErrorPopup(eErrorPopup.NEED_CLEAR_ALL, targetName, DoAction);
                UIStackManager.Instance.RemoveAll();
                DoAction();
            }
            else
            {
                // 스택으로 쌓을 수 있는 앱 실행
                DoAction();
            }

            void DoAction()
            {
                if (_myPadActionDict.TryGetValue(identifier, out var action))
                {
                    action?.ActionInvoke(targetApp.AppID, targetApp.IsActivation);
                    if (!_myPadPopup.IsUnityNull())
                    {
                        _myPadPopup.Hide();
                    }
                }
            }
        }

        public void CheckBeforeRemoveItem(string identifier)
        {
            if (!GetInfoFromIdentifier(identifier, out var targetApp, out var targetName, out var canUse)) return;

            UIManager.Instance.ShowPopupYesNo(MyPadString.DeletePopupTitle,
                                              MyPadString.DeletePopupContext(targetName),
                                              (view) => RemoveMyPadItem(identifier));
        }

        public void AddMyPadItem() => _myPadViewModel?.AddMyPadItem();
        public void RefreshMyPadItem() => _myPadViewModel?.RefreshMyPadItem();
        public void RemoveMyPadItem(string id) => _myPadViewModel?.RemoveMyPadItem(id);
        public void ResetMyPadItem() => _myPadViewModel?.ResetMyPadItem();
#endregion // MyPadItem

#region TableData
        private void RegisterMyPadAction()
        {
            _myPadActionDict.Clear();

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(MyPadElementAttribute)));

            foreach (var type in types)
            {
                var myPadElementAttribute = type.GetCustomAttribute<MyPadElementAttribute>();
                var myPadAction = Activator.CreateInstance(type) as BaseMyPadAction;

                _myPadActionDict.Add(myPadElementAttribute.Identifier, myPadAction);
            }
        }

        public void LoadTable() => _tableOfMyPad = TableDataManager.Instance.Get<TableMyPad>();
        public List<MyPad> GetTable() => _tableOfMyPad.Datas.Values.ToList();
        public List<MyPad> GetLayoutMyPadItemList() => _tableOfMyPad.Datas.Values.Where(myPad => myPad.Number < 0).ToList();
        public List<MyPad> GetMyPadListSuchAsAppType(eServiceType appType) => _tableOfMyPad.Datas.Values.Where(myPad => myPad.Type == appType).ToList();
        public void AddMyPadItemInList(eServiceType appType, List<MyPad> myPadList) => myPadList.AddRange(_tableOfMyPad.Datas.Values.Where(myPad => myPad.Type == appType));
        public void AddAllMyPadItemInList(List<MyPad> myPadList) => myPadList.AddRange(_tableOfMyPad.Datas.Values);

        public void SetAdvertisement(RepeatedField<MyPadAdvertisement> field)
        {
            C2VDebug.LogCategory("MyPad", $"{nameof(SetAdvertisement)} Count : {field.Count}");
            _listOfMyPadAdv = field.ToList();
        }
        public List<MyPadAdvertisement> GetAdvertisement(int language)
            => _listOfMyPadAdv.Where(target => target.LanguageType == language + 1 && CheckAdvertisementTime(target)).ToList();

        private bool CheckAdvertisementTime(MyPadAdvertisement myPadAdv)
        {
            var target = MetaverseWatch.NowDateTime;
            return myPadAdv.DisplayStartDatetime.ToDateTime() <= target && target <= myPadAdv.DisplayEndDatetime.ToDateTime();
        }
#endregion // TableData

#region Utils
        private bool GetInfoFromIdentifier(string id, out MyPad myPad, out string name, out bool canUse)
        {
            if (_tableOfMyPad?.Datas == null)
            {
                myPad = null;
                name = string.Empty;
                canUse = false;
                return false;
            }
            myPad = _tableOfMyPad.Datas[id];

            if (myPad == null)
            {
                name = string.Empty;
                canUse = false;
                return false;
            }
            name = Localization.Instance.GetString(myPad.Name);

            canUse = CheckEnableApp(myPad.Type);
            return true;
        }

        public bool CheckEnableApp(eServiceType type)
        {
            if (type == eServiceType.WORLD) return true;
            else return CurrentScene.ServiceType == type;
        }

        private async UniTask OnTimeUpdate()
        {
            string format = MyPadString.BaseDateFormat;

            _onCurrentTime?.Invoke(
                MetaverseWatch.NowDateTime.ToString(format),
                MyPadString.TranslateDayOfWeek(MetaverseWatch.NowDateTime.DayOfWeek));

            _timeCancellationTokenSource ??= new CancellationTokenSource();

            float currentTime = 1;
            while (true)
            {
                if (_timeCancellationTokenSource is {IsCancellationRequested: true})
                {
                    _timeCancellationTokenSource = null;
                    return;
                }

                currentTime -= Time.deltaTime;
                if (currentTime < 0)
                {
                    _onCurrentTime?.Invoke(
                        MetaverseWatch.NowDateTime.ToString(format),
                        MyPadString.TranslateDayOfWeek(MetaverseWatch.NowDateTime.DayOfWeek));

                    currentTime = 1;
                }

                await UniTask.Yield();
            }
        }

        private void ResetTimer()
        {
            _timeCancellationTokenSource?.Cancel();

            _onCurrentTime = null;
        }

        public void ShowErrorPopup(eErrorPopup type, string targetName = "", Action doAction = null)
        {
            switch (type)
            {
                case eErrorPopup.INVALID_SERVICE:
                    // 현재 지역에서 사용 불가능한 앱 처리
                    UIManager.Instance.ShowPopupConfirm(MyPadString.NoticePopupTitle,
                        MyPadString.NoticePopupContextCantOpen,
                        null,
                        MyPadString.CommonPopupConfirm);
                    break;
                case eErrorPopup.DUPLICATED:
                    // 중복 실행 시도하는 앱 처리
                    UIManager.Instance.ShowPopupConfirm(MyPadString.NoticePopupTitle,
                        MyPadString.NoticePopupContextAlreadyOpen,
                        null,
                        MyPadString.CommonPopupConfirm);
                    break;
                case eErrorPopup.NEED_CLEAR_ALL:
                    // 스택으로 쌓을 수 없는 앱 실행
                    UIManager.Instance.ShowPopupYesNo(MyPadString.NoticePopupTitle,
                        MyPadString.NoticePopupContextCheck(targetName),
                        (view) =>
                        {
                            UIStackManager.Instance.RemoveAll();
                            doAction?.Invoke();
                        });
                    break;
            }
        }
#endregion // Utils

#region Events
        private Action _onMyPadOpened;
        private Action _onMyPadClosed;
        private Action<string, string> _onCurrentTime;

        public event Action OnMyPadOpenedEvent
        {
            add
            {
                _onMyPadOpened -= value;
                _onMyPadOpened += value;
            }
            remove => _onMyPadOpened -= value;
        }

        public event Action OnMyPadClosedEvent
        {
            add
            {
                _onMyPadClosed -= value;
                _onMyPadClosed += value;
            }
            remove => _onMyPadClosed -= value;
        }

        public event Action<string, string> OnCurrentTimeEvent
        {
            add
            {
                _onCurrentTime -= value;
                _onCurrentTime += value;
            }
            remove => _onCurrentTime -= value;
        }
#endregion
    }

    public static class MyPadString
    {
        public static string CommonPopupConfirm                   => Localization.Instance.GetString("UI_Common_Btn_OK");
        public static string CommonPopupYes                       => Localization.Instance.GetString("UI_Common_Btn_Yes");
        public static string CommonPopupNo                        => Localization.Instance.GetString("UI_Common_Btn_No");
        public static string CommonPopupCancel                    => Localization.Instance.GetString("UI_Common_Btn_Cancel");
        public static string LogoutPopupTitle                     => Localization.Instance.GetString("UI_MyPad_LogOut_Popup_Title");
        public static string LogoutPopupContext                   => Localization.Instance.GetString("UI_MyPad_LogOut_Popup_Desc");
        public static string QuitPopupTitle                       => Localization.Instance.GetString("UI_MyPad_Quit_Popup_Title");
        public static string QuitPopupContext                     => Localization.Instance.GetString("UI_MyPad_Quit_Popup_Desc");
        public static string NoticePopupTitle                     => Localization.Instance.GetString("UI_MyPad_Notice_Popup_Title");
        public static string NoticePopupContextCantOpen           => Localization.Instance.GetString("UI_MyPad_Notice_Popup_Desc3");
        public static string NoticePopupContextAlreadyOpen        => Localization.Instance.GetString("UI_MyPad_Notice_Popup_Desc2");
        public static string NoticePopupContextCheck(string name) => Localization.Instance.GetString("UI_MyPad_Notice_Popup_Desc1", name);
        public static string DeletePopupTitle                     => Localization.Instance.GetString("UI_MyPad_Delete_Popup_Title");
        public static string DeletePopupContext(string name)      => Localization.Instance.GetString("UI_MyPad_Delete_Popup_Desc", name);

        // FIXME: Localization
        public static string EscapePopupTitle      => Localization.Instance.GetString("UI_Escape_Desc_02", Localization.Instance.GetString("UI_Escape_Place_Plaza")); // [{0}] - 광장으로 고정
        public static string EscapePopupCanEscape  => Localization.Instance.GetString("UI_Escape_Timer_Now"); // 임시로 텍스트 고정
        public static string EscapePopupCantEscape => Localization.Instance.GetString("UI_Escape_Desc_03", Localization.Instance.GetString("UI_Escape_Timer"));
        public static string EscapePopupText       => Localization.Instance.GetString("UI_Escape_Btn");

        public static string BaseDateFormat => "yy MM dd HH:mm";
        public static string DisplayDateFormat(string year, string month, string date, string day) => Localization.Instance.GetString("UI_DisplayTypeDate101", year, month, date, day);
        public static string TranslateDayOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:    return Localization.Instance.GetString("UI_Common_Monday");
                case DayOfWeek.Tuesday:   return Localization.Instance.GetString("UI_Common_Tuesday");
                case DayOfWeek.Wednesday: return Localization.Instance.GetString("UI_Common_Wednesday");
                case DayOfWeek.Thursday:  return Localization.Instance.GetString("UI_Common_Thursday");
                case DayOfWeek.Friday:    return Localization.Instance.GetString("UI_Common_Friday");
                case DayOfWeek.Saturday:  return Localization.Instance.GetString("UI_Common_Saturday");
                case DayOfWeek.Sunday:    return Localization.Instance.GetString("UI_Common_Sunday");
                default:                  return string.Empty;
            }
        }
    }
}
