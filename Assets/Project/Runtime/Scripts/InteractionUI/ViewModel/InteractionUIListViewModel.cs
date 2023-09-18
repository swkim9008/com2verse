using System.Collections.Generic;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.PhysicsAssetSerialization;
using UnityEngine;

namespace Com2Verse.UI
{
    [ViewModelGroup("EventTrigger")]
    public class InteractionUIListViewModel : ViewModelBase
    {
        private const float InteractionButtonWidth = 170;
        private const float InteractionButtonHeight = 50;
        private const float InteractionButtonVerticalSpacing = 10;
    
#region Variable
        private Collection<InteractionUIViewModel> _interactionButtonCollection = new();
        private Transform _targetTriggerPosition;
        private Dictionary<C2VEventTrigger, Dictionary<int, InteractionUIViewModel>> _triggerViewModelMap = new();

        private bool _active = true;
#endregion

#region Property
        public float InteractionButtonSpacing { get; set; } = InteractionButtonVerticalSpacing;
        public Transform InteractionPopup { get; set; }

        public Transform TargetTriggerTransform
        {
            set => _targetTriggerPosition = value;
        }

        public Collection<InteractionUIViewModel> InteractionButtonCollection
        {
            get => _interactionButtonCollection;
            set
            {
                _interactionButtonCollection = value;
                base.InvokePropertyValueChanged(nameof(InteractionButtonCollection), _interactionButtonCollection);
            }
        }

        private Vector3 TriggerPosition
        {
            get
            {
                if (!_targetTriggerPosition.IsUnityNull())
                {
                    return _targetTriggerPosition.position;
                }

                return Vector3.zero;
            }
        }

        public bool HasInteraction(C2VEventTrigger trigger, int callbackIndex)
        {
            if (!_triggerViewModelMap.TryGetValue(trigger, out var callbackMap))
            {
                return false;
            }

            return callbackMap.ContainsKey(callbackIndex);
        }

        public void SetInteraction(C2VEventTrigger trigger, int callbackIndex, InteractionUIViewModel newViewModel)
        {
            if (_triggerViewModelMap.TryGetValue(trigger, out var callbackMap))
            {
                callbackMap.Add(callbackIndex, newViewModel);
            }
            else
            {
                _triggerViewModelMap.Add(trigger, new Dictionary<int, InteractionUIViewModel>()
                {
                    {callbackIndex, newViewModel}
                });
            }
            InteractionButtonCollection.AddItem(newViewModel);

            ViewModelManager.Instance.OnUpdateHandler += OnUpdate;
        }

        public void UnsetInteraction(C2VEventTrigger trigger, int callbackIndex)
        {
            if (_triggerViewModelMap.TryGetValue(trigger, out var callbackMap))
            {
                if (callbackMap.TryGetValue(callbackIndex, out var viewModel))
                {
                    InteractionButtonCollection.RemoveItem(viewModel);
                    _triggerViewModelMap.Remove(trigger);
                }
            }
        }

        public Vector3 InteractionListAnchoredPosition
        {
            get
            {
                Camera mainCam = CameraManager.InstanceOrNull?.MainCamera;
                if (mainCam.IsUnityNull() || InteractionPopup.IsUnityNull()) return Vector3.zero;
                
                RectTransform canvasTransform = InteractionPopup.transform as RectTransform;
                Vector3 normalized = mainCam.WorldToViewportPoint(TriggerPosition);

                if (normalized.z < 1 - float.Epsilon)
                {
                    normalized.x = canvasTransform.rect.width * 10;
                    normalized.y = canvasTransform.rect.height * 10;
                }
                else
                {
                    normalized = Vector3.Max(Vector3.zero, Vector3.Min(Vector3.one, normalized));
                    normalized.x *= canvasTransform.rect.width;
                    normalized.y *= canvasTransform.rect.height;

                    float interactionListHeight = (_interactionButtonCollection.CollectionCount * (InteractionButtonHeight + InteractionButtonSpacing) - InteractionButtonSpacing) / 2;
                    float interactionListWidth = InteractionButtonWidth / 2;
                    normalized.x = Mathf.Max(Mathf.Min(canvasTransform.rect.width - interactionListWidth, normalized.x), interactionListWidth);
                    normalized.y = Mathf.Max(Mathf.Min(canvasTransform.rect.height - interactionListHeight, normalized.y), interactionListHeight);
                }

                return normalized;
            }
            set { C2VDebug.LogWarning("Do not set position by this property."); }
        }

        public bool InteractionUIActive
        {
            get => _active;
            set
            {
                _active = value;
                InvokePropertyValueChanged(nameof(InteractionUIActive), _active);
            }
        }
#endregion

        public void Show(bool active)
        {
            InteractionUIActive = active;
        }

        public void RefreshChildrenActive()
        {
            foreach (var item in (IEnumerable<InteractionUIViewModel>) _interactionButtonCollection.ItemsSource)
            {
                item.Refresh();
            }
        }

        private void OnUpdate()
        {
            InvokePropertyValueChanged(nameof(InteractionListAnchoredPosition), InteractionListAnchoredPosition);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            ViewModelManager.Instance.OnUpdateHandler -= OnUpdate;
        }
    }
}
