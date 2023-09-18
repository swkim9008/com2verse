/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarHUDFactory.cs
* Developer:	tlghks1009
* Date:			2023-09-06 10:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Tweener;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
    [Serializable]
    public class AvatarHudLocator
    {
        [field: SerializeField] public Transform Root         { get; private set; }
        [field: SerializeField] public string    PropertyName { get; private set; }
        [field: SerializeField] public string    Address      { get; private set; }

        public (C2VAsyncOperationHandle<GameObject>, GameObject) CreateAsset()
        {
            var handle = C2VAddressables.LoadAsset<GameObject>($"{Address}.prefab");

            if (handle == null)
                return (null, null);

            ShowRootObj();

            return (handle, GameObject.Instantiate(handle.Result, Root, false));
        }


        public (C2VAsyncOperationHandle<GameObject>, GameObject) CreateAsset(TweenController speakingTweenController, TweenController speakerTweenController)
        {
            (C2VAsyncOperationHandle<GameObject> handle, GameObject obj) = CreateAsset();

            if (handle == null)
                return (null, null);

            var avatarVoiceTweenReference = obj.GetComponent<AvatarHudVoiceTweenReference>();

            if (!avatarVoiceTweenReference.IsUnityNull())
                avatarVoiceTweenReference.SetTweenController(speakingTweenController, speakerTweenController);

            return (handle, obj);
        }

        public void ShowRootObj() => Root.gameObject.SetActive(true);

        public void HideRootObj() => Root.gameObject.SetActive(false);
    }


    [Serializable]
    public class AvatarHudPrefabCreator
    {
        public class CachingHandle
        {
            public string     Address      { get; }
            public string     PropertyName { get; }
            public GameObject GameObject   { get; }

            private readonly C2VAsyncOperationHandle<GameObject> _handle;
            private readonly Transform                           _root;

            public CachingHandle(AvatarHudLocator avatarHudLocator, C2VAsyncOperationHandle<GameObject> handle, GameObject gameObject)
            {
                Address      = avatarHudLocator.Address;
                PropertyName = avatarHudLocator.PropertyName;
                GameObject = gameObject;

                _handle     = handle;
                _root       = avatarHudLocator.Root;
            }

            public void DestroyAsset()
            {
                if (_handle.IsValid())
                {
                    _handle.Release();
                    GameObject.Destroy(GameObject);

                    _root.gameObject.SetActive(false);
                }
            }
        }

        [SerializeField] private List<AvatarHudLocator> _avatarHudPrefabLocators;

        [SerializeField] private TweenController _speakingTweenController;
        [SerializeField] private TweenController _speakerTweenController;

        /// <summary>
        /// key : propertyName , value : list Handle~
        /// </summary>
        private Dictionary<string, List<CachingHandle>> _cachingHandles = new();

        public bool TryGetCachingHandle(string propertyName, out List<CachingHandle> cachingHandles)
        {
            return _cachingHandles.TryGetValue(propertyName, out cachingHandles);
        }


        public AvatarHudLocator GetAvatarHudLocator(string propertyName)
        {
            foreach (var avatarHudLocator in _avatarHudPrefabLocators)
            {
                if (avatarHudLocator.PropertyName == propertyName)
                    return avatarHudLocator;
            }

            return null;
        }

        public void Create(string propertyName)
        {
            foreach (var avatarHudLocator in _avatarHudPrefabLocators)
            {
                if (avatarHudLocator.PropertyName != propertyName)
                    continue;

                if (IsCached(propertyName, avatarHudLocator.Address))
                    continue;


                (C2VAsyncOperationHandle<GameObject> handle, GameObject obj) = avatarHudLocator.CreateAsset(_speakingTweenController, _speakerTweenController);

                AddCachingHandle(propertyName, new CachingHandle(avatarHudLocator, handle, obj));
            }
        }


        public void Destroy(string propertyName)
        {
            HideRootObj(propertyName);

            RemoveCachingHandle(propertyName);
        }


        private void AddCachingHandle(string propertyName, CachingHandle cachingHandle)
        {
            if (_cachingHandles.TryGetValue(propertyName, out var cachingHandles))
                cachingHandles.Add(cachingHandle);
            else
            {
                cachingHandles = new List<CachingHandle> {cachingHandle};

                _cachingHandles.Add(propertyName, cachingHandles);
            }
        }


        private void RemoveCachingHandle(string propertyName)
        {
            if (_cachingHandles.TryGetValue(propertyName, out var cachingHandles))
            {
                foreach (var cachingHandle in cachingHandles)
                    cachingHandle.DestroyAsset();

                cachingHandles.Clear();
                _cachingHandles.Remove(propertyName);
            }
        }


        private void HideRootObj(string propertyName)
        {
            foreach (var avatarHudLocator in _avatarHudPrefabLocators)
            {
                if (avatarHudLocator.PropertyName == propertyName)
                    avatarHudLocator.HideRootObj();
            }
        }


        private bool IsCached(string propertyName, string address)
        {
            if (_cachingHandles.TryGetValue(propertyName, out var cachingHandles))
            {
                foreach (var cachingHandle in cachingHandles)
                {
                    if (cachingHandle.Address == address)
                        return true;
                }
            }
            return false;
        }
    }


    public sealed class AvatarHudObjectManager : MonoBehaviour, IViewModelContainerBridge
    {
        [SerializeField] private AvatarHudPrefabCreator _avatarHudPrefabCreator;

        public ViewModelContainer ViewModelContainer { get; private set; } = new();

        private readonly Dictionary<GameObject, Binder[]> _binders = new();

        [UsedImplicitly]
        public bool IsOnHandsUp
        {
            get => false;
            set => OnStateChanged(nameof(IsOnHandsUp), value);
        }

        [UsedImplicitly]
        public bool UseCameraView
        {
            // binding을 위해 있는 임시 getter(사용X)
            get => false;
            set => OnStateChanged(nameof(UseCameraView), value);
        }

        [UsedImplicitly]
        public bool UseCameraObserver
        {
            // binding을 위해 있는 임시 getter(사용X)
            get => false;
            set => OnStateChanged(nameof(UseCameraObserver), value);
        }

        [UsedImplicitly]
        public bool UseVoiceView
        {
            // binding을 위해 있는 임시 getter(사용X)
            get => false;
            set => OnStateChanged(nameof(UseVoiceView), value);
        }

        [UsedImplicitly]
        public bool UseVoiceIcon
        {
            // binding을 위해 있는 임시 getter(사용X)
            get => false;
            set => OnStateChanged(nameof(UseVoiceIcon), value);
        }

        [UsedImplicitly]
        public bool UseAuditoriumSpeakerNameplate
        {
            // binding을 위해 있는 임시 getter(사용X)
            get => false;
            set => OnStateChanged(nameof(UseAuditoriumSpeakerNameplate), value);
        }

        [UsedImplicitly]
        public bool UseMiceSpeakerNameplate
        {
            // binding을 위해 있는 임시 getter(사용X)
            get => false;
            set => OnStateChanged(nameof(UseMiceSpeakerNameplate), value);
        }

        public Transform GetTransform() => this.transform;

        public void SetViewModelContainer(ViewModelContainer viewModelContainer) => ViewModelContainer = viewModelContainer;

        private void OnStateChanged(string propertyName, bool value)
        {
            if (value)
            {
                CreateAsset(propertyName);
                BindOverriden(propertyName);
            }
            else
            {
                UnbindOverriden(propertyName);
                DestroyAsset(propertyName);
            }                                                       
        }


        private void CreateAsset(string propertyName) => _avatarHudPrefabCreator.Create(propertyName);

        private void DestroyAsset(string propertyName) => _avatarHudPrefabCreator.Destroy(propertyName);


        private void BindOverriden(string propertyName)
        {
            if (_avatarHudPrefabCreator.TryGetCachingHandle(propertyName, out var cachingHandles))
            {
                foreach (var cachingHandle in cachingHandles)
                {
                    if (cachingHandle.PropertyName != propertyName)
                        continue;

                    if (_binders.TryGetValue(cachingHandle.GameObject!, out var binders))
                    {
                        BindInternal(binders);
                        return;
                    }

                    binders = cachingHandle.GameObject.GetComponentsInChildren<Binder>(true);
                    BindInternal(binders);

                    _binders.Add(cachingHandle.GameObject, binders);
                }
            }

            void BindInternal(Binder[] binders)
            {
                foreach (var binder in binders)
                    binder.SetViewModelContainer(ViewModelContainer).Bind();
            }
        }

        private void UnbindOverriden(string propertyName)
        {
            if (_avatarHudPrefabCreator.TryGetCachingHandle(propertyName, out var cachingHandles))
            {
                foreach (var cachingHandle in cachingHandles)
                {
                    if (cachingHandle.PropertyName != propertyName)
                        continue;

                    if (_binders.TryGetValue(cachingHandle.GameObject, out var binders))
                    {
                        foreach (var binder in binders)
                            binder.Unbind();
                    }

                    _binders.Remove(cachingHandle.GameObject);
                }
            }
        }

        public void Bind()   { }
        public void Unbind() { }
    }
}
