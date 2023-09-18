/*===============================================================
* Product:    Com2Verse
* File Name:  NavigationBinder.cs
* Developer:  tlghks1009
* Date:       2022-04-05 12:20
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2Verse.UI
{
    [AddComponentMenu("[DB]/[DB] Navigation Binder")]
    [RequireComponent(typeof(MetaverseButton))]
    public class NavigationBinder : Binder
    {
        [Serializable]
        public class Sequence
        {
            public eCommandType commandType;
            public eTargetType targetType;
            public int priority;
            public string sceneName;
            public AssetReference assetReference;
        }

        public enum eCommandType
        {
            NONE,
            VIEW_SHOW,
            VIEW_HIDE,
            VIEW_FIXED,
            VIEW_DESTROY,
        }

        public enum eTargetType
        {
            ROOT,
            SELF,
            OTHER
        }


        [HideInInspector] [SerializeField] private MetaverseButton _metaverseButton;
        [HideInInspector] [SerializeField] private List<Sequence> _sequenceList = new();

        private readonly List<GUIView> _fixedViewList = new();
        private GUIView _rootView;

        private Queue<Sequence> _sequenceQueue;

        public override void Bind()
        {
            base.Bind();

            _metaverseButton.onClick.RemoveListener(Execute);
            _metaverseButton.onClick.AddListener(Execute);

            _rootView = GetComponentInParent<GUIView>();
            _fixedViewList.Clear();
        }

        public override void Unbind()
        {
            base.Unbind();

            _metaverseButton.onClick.RemoveListener(Execute);
        }


        public override void Execute()
        {
            LoadViewInSequenceList(() =>
            {
                _sequenceQueue = _sequenceList.ToQueue();

                ExecuteSequence();
            });
        }


        private void LoadViewInSequenceList(Action onLoadCompleted)
        {
            int toLoadCount = _sequenceList.Count;
            foreach (var sequence in _sequenceList)
            {
                if (!sequence.assetReference.IsValid())
                {
                    OnNextStep();
                    continue;
                }

                UIManager.Instance.LoadAsync(sequence.assetReference, OnNextStep).Forget();
            }

            void OnNextStep()
            {
                toLoadCount--;
                if (toLoadCount <= 0)
                {
                    onLoadCompleted?.Invoke();
                }
            }
        }


        private void ExecuteSequence()
        {
            if (_sequenceQueue.Count == 0)
            {
                return;
            }


            var sequence = _sequenceQueue.Dequeue();

            FindTargetViewAsync(sequence, view => { ExecuteCommand(sequence, view); });
        }


        private void ExecuteCommand(Sequence sequence, GUIView targetView)
        {
            if (ReferenceEquals(targetView, null))
                return;

            switch (sequence.commandType)
            {
                case eCommandType.VIEW_FIXED:
                {
                    _fixedViewList.Add(targetView);
                    ExecuteSequence();
                }
                    break;

                case eCommandType.VIEW_HIDE:
                {
                    targetView.OnClosedEvent += OnClosedEventCallback;
                    targetView.Hide();
                }
                    break;

                case eCommandType.VIEW_SHOW:
                {
                    targetView.OnOpenedEvent += OnOpenedEventCallback;
                    targetView.Show();
                }
                    break;

                case eCommandType.VIEW_DESTROY:
                {
                    UIManager.Instance.Destroy(targetView);
                    ExecuteSequence();
                }
                    break;

                case eCommandType.NONE:
                    ExecuteSequence();
                    break;
            }
        }

        private void OnClosedEventCallback(GUIView guiView)
        {
            guiView.OnClosedEvent -= OnClosedEventCallback;

            ExecuteSequence();
        }

        private void OnOpenedEventCallback(GUIView guiView)
        {
            guiView.OnOpenedEvent -= OnOpenedEventCallback;

            ExecuteSequence();
        }


        private void FindTargetViewAsync(Sequence sequence, Action<GUIView> onCompleted)
        {
            switch (sequence.targetType)
            {
                case eTargetType.ROOT:
                case eTargetType.SELF:
                {
                    var targetView = _rootView;
                    onCompleted(targetView);
                }
                    break;
                case eTargetType.OTHER:
                {
                    UIManager.Instance.CreatePopup(sequence.assetReference, (guiView) =>
                    {
                        var targetView = guiView;
                        onCompleted(targetView);
                    }).Forget();
                }
                    break;
            }
        }
    }
}
