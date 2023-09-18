/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_Timer.cs
* Developer:	tlghks1009
* Date:			2022-08-25 12:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Com2Verse.UI
{
    public partial class UIManager : Singleton<UIManager>, IUpdateOrganizer
    {
        private IObjectPool<Timer> _timerPool;

        public class Timer
        {
            private IUpdateOrganizer _updateOrganizer;
            private Action<Timer> _onTimeFinished;
            private Func<bool> _validator = () => true;

            private float _limitTime = 0f;
            private float _elapsedTime = 0f;

            private bool _isTimeout;

            public Timer() { }

            public void Set(IUpdateOrganizer updateOrganizer, float limitTime, Action<Timer> onTimeFinished, Func<bool> validator)
            {
                _validator = validator;
                Set(updateOrganizer, limitTime, onTimeFinished);
            }

            public void Set(IUpdateOrganizer updateOrganizer, float limitTime, Action<Timer> onTimeFinished)
            {
                _updateOrganizer = updateOrganizer;
                _updateOrganizer.AddUpdateListener(OnUpdate, true);

                _limitTime = limitTime;
                _onTimeFinished = onTimeFinished;

                _isTimeout = false;
            }

            private void OnUpdate()
            {
                if (!_validator()) return;
                if (_isTimeout) return;

                _elapsedTime = Mathf.Min(_elapsedTime + Time.deltaTime, _limitTime);

                _isTimeout = _elapsedTime >= _limitTime;
                if (_isTimeout)
                {
                    _onTimeFinished?.Invoke(this);
                    Reset();
                }
            }


            public void Reset()
            {
                if (_updateOrganizer != null)
                {
                    _updateOrganizer.RemoveUpdateListener(OnUpdate);
                    _updateOrganizer = null;
                }

                _onTimeFinished = null;
                _elapsedTime = 0f;
                _isTimeout = true;
            }
        }

        private void InitializeTimer()
        {
            CreateTimerPool();
        }

        public void StartTimer(Timer timer, float time, Action onTimeFinished)
        {
            if (!_isUpdateReady)
            {
                onTimeFinished?.Invoke();
                return;
            }


            timer.Reset();
            timer.Set(this, time, (timeFinishedTimer) =>
            {
                onTimeFinished?.Invoke();
            });
        }

        public void StartTimer(float time, Action onTimeFinished)
        {
            if (!_isUpdateReady)
            {
                onTimeFinished?.Invoke();
                return;
            }

            var timer = _timerPool.Get();
            timer.Set(this, time, (timeFinishedTimer) =>
            {
                _timerPool.Release(timeFinishedTimer);
                onTimeFinished?.Invoke();
            });
        }

        private void CreateTimerPool()
        {
            _timerPool = new ObjectPool<Timer>(
                () =>
                {
                    var timer = new Timer();
                    return timer;
                }, maxSize: 5);
        }
    }
}
