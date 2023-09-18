/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationPlayer.cs
* Developer:	tlghks1009
* Date:			2022-05-16 14:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Tutorial;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
    [AddComponentMenu("[CVUI]/[CVUI] AnimationPlayer")]
    [RequireComponent(typeof(UnityEngine.Animation))]
    public sealed class AnimationPlayer : MonoBehaviour, IDisposable
    {
        public class AnimationUnit
        {
            private Action<string> _onFinishedEvent;
            private string _animationName;

            public AnimationUnit(string animationName)
            {
                _animationName = animationName;
            }

            public void OnFinished(Action<string> onFinished)
            {
                _onFinishedEvent = onFinished;
            }

            public void AddHandler(Action<string> handler)
            {
                _onFinishedEvent += handler;
            }

            public void RemoveHandler(Action<string> handler)
            {
                _onFinishedEvent -= handler;
            }

            public void InvokeAnimationFinishedEvent()
            {
                _onFinishedEvent?.Invoke(_animationName);
            }
        }

        private readonly Dictionary<string, AnimationUnit> _animationUnitsDict = new();
        private UnityEngine.Animation _animation;
        private CancellationTokenSource _cancellationToken;
        private bool _isPlayable;
        [SerializeField] private bool _LoopOpenAnimation;


        private void Awake()
        {
            FindAnimation();

            if (_cancellationToken == null)
                _cancellationToken = new CancellationTokenSource();
        }

        public void Dispose()
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Dispose();
                _cancellationToken = null;
            }
        }


        private void OnDisable()
        {
            _isPlayable = false;
        }

        private void OnEnable()
        {
            _isPlayable = true;
        }

        public void ManualClear()
        {
            _animation.Stop();
            _animationUnitsDict.Clear();
        }

        public async UniTask StopAndPlayNext(int stopAniIndex, int goAniIndex)
        {
            if(_animation.isPlaying)
            {
                var animationClip = GetClipByIndex(stopAniIndex);
                
                await UniTask.WaitUntil(() =>
                {
                    AnimationState animState = _animation[animationClip.name];
                    float elapsedTime = animState.time;
                    float animLength = animState.length;
                    if (elapsedTime >= animLength || animState.enabled == false)
                    {
                        animState.normalizedTime = 1;
                        _animation.Stop();
                        TutorialManager.AniStopAndGo = true;
                        return true;
                    }
                    
                    return false;
                }, cancellationToken: _cancellationToken.Token);
                
                Play(goAniIndex);
            }
        }

        public AnimationUnit Play(int index)
        {
            if (!_isPlayable) return null;

            var animationClip = GetClipByIndex(index);
            return Play(animationClip.name);
        }


        public AnimationUnit Play(string animationName)
        {
            if (!_isPlayable) return null;

            if (_animation.IsReferenceNull())
                FindAnimation();

            RegisterAnimationEvents(animationName);

            _animation.Play(animationName);

            var animationUnit = new AnimationUnit(animationName);   

            if (!_animationUnitsDict.ContainsKey(animationName!))
            {
                _animationUnitsDict.Add(animationName!, animationUnit);
            }

            return animationUnit;
        }

        public bool IsPlaying(string animationName) => _animation.IsPlaying(animationName);

        public AnimationUnit GetAnimationUnit(string animationName) => _animationUnitsDict.TryGetValue(animationName!, out var animationUnit) ? animationUnit : null;

        private void RegisterAnimationEvents(string animationName)
        {
            var clip = _animation.GetClip(animationName);
            if (clip == null)
            {
                C2VDebug.LogError($"[AnimationPlayer] Can't find clip. AnimationName : {animationName}");
                return;
            }
    
            clip.events = null;

            var animationEndEvent = new AnimationEvent
            {
                time = clip.length,
                functionName = "OnAnimationFinishedHandler",
                stringParameter = animationName,
            };

            if (_LoopOpenAnimation)
            {
                // manual stop
            }
            else
            {
                clip.AddEvent(animationEndEvent);
            }
        }


        [UsedImplicitly]
        private void OnAnimationFinishedHandler(string animationName)
        {
            if (_animationUnitsDict.TryGetValue(animationName!, out var animationUnit))
            {
                animationUnit.InvokeAnimationFinishedEvent();

                _animationUnitsDict.Remove(animationName);
            }
        }

        public AnimationClip GetClipByIndex(int index)
        {
            if (_animation.IsReferenceNull())
                FindAnimation();

            int indexer = 0;
            foreach (AnimationState animationState in _animation)
            {
                if (index == indexer)
                    return animationState.clip;

                indexer++;
            }

            return null;
        }

        private void FindAnimation()
        {
            _animation = GetComponent<UnityEngine.Animation>();
        }

        public void AnimationStop() => _animation.Stop();
        public void AnimationRewind() => _animation.Rewind(_animation.name);
        private void OnDestroy() => Dispose();
        private void OnApplicationQuit() => Dispose();
    }
}
