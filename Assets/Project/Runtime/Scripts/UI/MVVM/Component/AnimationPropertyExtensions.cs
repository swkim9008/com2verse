/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationPropertyExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-13 11:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
    [RequireComponent(typeof(AnimationPlayer))]
    public sealed class AnimationPropertyExtensions : MonoBehaviour
    {
        [SerializeField] private bool _isPlayOnEnable = false;
        [SerializeField] private string _animationName;
        [SerializeField] private UnityEvent _onAnimationFinished;

        private AnimationPlayer _animationPlayer;

        private void OnEnable()
        {
            if (_isPlayOnEnable) AnimationPlayOnNextFrameProxy(true).Forget();
        }

        public AnimationPropertyExtensions GetAnimationPropertyExtensions
        {
            get => this;
            set => GetAnimationPropertyExtensions = value;
        }

        [UsedImplicitly]
        public bool AnimationPlay
        {
            get => _animationPlayer.IsPlaying(_animationName);
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                if (_animationPlayer.IsUnityNull())
                    FindAnimationPlayer();

                _animationPlayer.Play(_animationName)?.OnFinished(OnAnimationFinished);
            }
        }

        [UsedImplicitly]
        // for execute AnimationPlay on OnEnable Event
        public bool AnimationPlayOnNextFrame
        {
            get => _animationPlayer.IsPlaying(_animationName);
            set => AnimationPlayOnNextFrameProxy(value).Forget();
        }

        [UsedImplicitly]
        public string AnimationName
        {
            get => _animationName;
            set => _animationName = value;
        }

        [UsedImplicitly]
        public int PlayAnimationByIdx
        {
            get => -1;
            set
            {
                if (TryGetAnimationNameByIdx(value, out var name))
                {
                    AnimationName = name;
                    AnimationPlay = true;
                }
            }
        }
        private async UniTask AnimationPlayOnNextFrameProxy(bool value)
        {
            await UniTask.NextFrame();
            AnimationPlay = value;
        }

        private void OnAnimationFinished(string animationName)
        {
            _onAnimationFinished?.Invoke();
        }

        public void SetFuntion(string aniName, UnityEvent uevent)
        {
            AnimationName = aniName;
            _onAnimationFinished = uevent;
        }

        private void FindAnimationPlayer()
        {
            _animationPlayer = gameObject.GetComponent<AnimationPlayer>();
        }

        public void AnimationStop() => _animationPlayer.AnimationStop();
        public void AnimationRewind() => _animationPlayer.AnimationRewind();

        private bool TryGetAnimationNameByIdx(int idx, out string name)
        {
            name = string.Empty;

            if (_animationPlayer.IsUnityNull())
                FindAnimationPlayer();

            var clip = _animationPlayer?.GetClipByIndex(idx);
            if (clip == null) return false;

            name = clip.name;
            return true;
        }
    }
}
