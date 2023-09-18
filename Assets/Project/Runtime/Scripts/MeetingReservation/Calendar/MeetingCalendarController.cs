/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingCalendarController_BindingContainer.cs
* Developer:	tlghks1009
* Date:			2022-09-02 14:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.UI
{
    public sealed class MeetingCalendarController : MonoBehaviour, IBindingContainer
    {
        public ViewModelContainer ViewModelContainer { get; } = new();
        public Transform GetTransform() => this.transform;

        private Binder[] _binders;
        private GameObject _calendarObject;

        private bool _isOpened;

        public bool SetActiveCalendar
        {
            get => false;
            set
            {
                if (value == _isOpened) return;
                if (value) Bind();
                else Unbind();

                _isOpened = value;
            }
        }

        public MeetingCalendarViewModel MeetingCalendarViewModel
        {
            get => ViewModelContainer.GetViewModel<MeetingCalendarViewModel>();
            set
            {
                if (value == null) return;

                ViewModelContainer.ClearAll();
                ViewModelContainer.AddViewModel(value);
            }
        }


        public void Bind()
        {
            ViewModelContainer.InitializeViewModel();

            if (_calendarObject.IsReferenceNull())
            {
                _calendarObject = this.transform.Find("UI_ConnectingApp_Calendar")?.gameObject;
                if (_calendarObject.IsReferenceNull())
                {
                    _calendarObject = this.transform.Find("UI_Reservation_Calendar")?.gameObject;
                    if (_calendarObject.IsReferenceNull())
                    {
                        _calendarObject = transform.Find("UI_ConnectingApp_Reservation_Calendar")?.gameObject;
                        if (_calendarObject.IsReferenceNull())
                            _calendarObject = transform.Find("UI_ConnectingApp_Inquiry_Calendar")?.gameObject;
                    }
                }
            }

            if (_calendarObject.IsReferenceNull())
            {
                C2VDebug.LogError("MeetingCalendarController Calendar Object is Null!");
            }
            
            _calendarObject.SetActive(true);
            
            _binders ??= GetComponentsInChildren<Binder>();
            
            foreach (var binder in _binders)
            {
                if (binder.gameObject == this.gameObject)
                    continue;
            
                binder.SetViewModelContainer(ViewModelContainer, true);
            
                binder.Bind();
            }
        }

        public void Unbind()
        {
            foreach (var binder in _binders)
            {
                if (binder.IsUnityNull()) continue;

                if (binder.gameObject == this.gameObject) continue;


                binder.Unbind();
            }

            _calendarObject.SetActive(false);

            if (MeetingCalendarViewModel != null)
                MeetingCalendarViewModel.SetActiveCalendar = false;

            ViewModelContainer.ClearAll();
        }

        private void OnDestroy()
        {
            _binders = null;
            ViewModelContainer.ClearAll();
        }
    }
}
