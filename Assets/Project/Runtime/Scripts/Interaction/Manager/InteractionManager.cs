// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	InteractionManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-15 오후 3:18
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.Data;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Interaction
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        [UsedImplicitly] private InteractionManager() { }

        public const string InteractionIconSpriteAtlasName = "Icon_Interaction";
        private const string InteractionLinkIdFormat = "{0}{1}0{2}";

        private Dictionary<long, InteractionLink> _interactionLinkTemplates;
        private Dictionary<eLogicType, Data.Interaction> _interactionTemplates;
        private InteractionUIListHolder _interactionUIListHolder = null;
        private bool _iconResLoaded = false;

        private readonly Dictionary<Transform, InteractionUIListViewModel> _triggerListViewModelMapping = new ();
        private readonly Dictionary<eLogicType, Sprite> _logicImageMap = new();
        private readonly Dictionary<eLogicType, bool> _logicActiveMap = new();

        private InteractionUIListHolder InteractionUIListHolder
        {
            get
            {
                if (_interactionUIListHolder == null)
                    _interactionUIListHolder = ViewModelManager.Instance.Get<InteractionUIListHolder>();

                return _interactionUIListHolder;
            }
        }
        
        public void Initialize()
        {
            SceneManager.Instance.BeforeSceneChanged += OnBeforeSceneChanged;
            Reset();

            LoadInteractionTemplate();

            SpriteAtlasManager.Instance.LoadSpriteAtlasAsync(InteractionIconSpriteAtlasName, (handle) => { _iconResLoaded = true; }, true);
        }

        public void LoadInteractionTemplate()
        {
            var interactionData = TableDataManager.Instance.Get<TableInteraction>();
            _interactionTemplates = interactionData.Datas;

            var interactionLinkData = TableDataManager.Instance.Get<TableInteractionLink>();
            _interactionLinkTemplates = interactionLinkData.Datas;
        }
        
        private void OnBeforeSceneChanged(SceneBase currentScene, SceneBase newScene)
        {
            Reset();
        }

        private void Reset()
        {
            _interactionUIListHolder = null;
            _triggerListViewModelMapping.Clear();
            _logicActiveMap.Clear();
        }
        
        public Data.Interaction GetActionType(eLogicType logicType)
        {
            if (_interactionTemplates.TryGetValue(logicType, out Data.Interaction interaction))
            {
                return interaction;
            }

            return null;
        }

        public string GetInteractionNameKey(eLogicType logicType)
        {
            if (_interactionTemplates.TryGetValue(logicType, out Data.Interaction interaction))
            {
                return interaction.InteractionName;
            }

            return null;
        }

        public eTriggerValidationType GetValidationType(eLogicType logicType)
        {
            if (_interactionTemplates.TryGetValue(logicType, out Data.Interaction interaction))
            {
                return interaction.TriggerValidationType;
            }

            return eTriggerValidationType.CLIENT;
        }

        public int GetUserCountLimit(eLogicType logicType)
        {
            if (_interactionTemplates.TryGetValue(logicType, out Data.Interaction interaction))
            {
                return interaction.UseCountlimit;
            }

            return -1;
        }

        public InteractionLink GetInteractionLink(long linkId)
        {
            return _interactionLinkTemplates.TryGetValue(linkId, out var link) ? link : null;
        }
        
        public string GetInteractionValue(List<Protocols.ObjectInteractionMessage> interactionValues, int triggerIndex, int callbackIndex, int parameterIndex)
        {
            if (interactionValues == null) return string.Empty;
            
            foreach (var interactionValue in interactionValues)
            {
                if (_interactionLinkTemplates.TryGetValue(interactionValue.InteractionLinkId, out var link))
                {
                    if (link.TriggerIndex == triggerIndex + 1 && link.CallbackIndex == callbackIndex + 1 && interactionValue.InteractionNo == parameterIndex + 1)
                    {
                        return interactionValue.InteractionValue;
                    }
                }
            }

            return string.Empty;
        }
        
        public void SetInteractionUI(C2VEventTrigger sourceTrigger, int callbackIndex, Action clickAction, InteractionStringParameterSource source = null)
        {
            if (_triggerListViewModelMapping.TryGetValue(sourceTrigger.transform.parent, out var interactionList))
            {
                if (!interactionList.HasInteraction(sourceTrigger, callbackIndex))
                {
                    InitializeInteractionUI(sourceTrigger, callbackIndex, clickAction, interactionList, source);
                }
            }
            else
            {
                var newListViewModel = new InteractionUIListViewModel();
                _triggerListViewModelMapping.Add(sourceTrigger.transform.parent, newListViewModel);
                InteractionUIListHolder.InteractionListCollection.AddItem(newListViewModel);
                InitializeInteractionUI(sourceTrigger, callbackIndex, clickAction, newListViewModel, source);
            }
        }

        private void InitializeInteractionUI(C2VEventTrigger sourceTrigger, int callbackIndex, Action clickAction, InteractionUIListViewModel listViewModel, InteractionStringParameterSource source)
        {
            eLogicType logicType = (eLogicType)sourceTrigger.Callback[callbackIndex].Function;
            listViewModel.InteractionPopup = InteractionUIListHolder.InteractionCanvas;
            listViewModel.TargetTriggerTransform = sourceTrigger.transform.parent;

            var newViewModel = new InteractionUIViewModel()
            {
                TargetCanvas = InteractionUIListHolder.InteractionCanvas,
                ParentModel = listViewModel,
                ParameterSource = source
            };

            if (source != null)
                source.InteractionViewModel = newViewModel;

            string iconName = string.Empty;
            if (_interactionTemplates.TryGetValue(logicType, out var interactionTemplate))
            {
                newViewModel.Description = interactionTemplate.Name;
                iconName = Path.ChangeExtension(interactionTemplate.IconRes, null);
            }

            if (!_logicImageMap.TryGetValue(logicType, out Sprite image) && _iconResLoaded)
            {
                image = SpriteAtlasManager.Instance.GetSprite(InteractionIconSpriteAtlasName, iconName);
                _logicImageMap.Add(logicType, image);
            }

            newViewModel.IconImage = image;
            newViewModel.LogicType = logicType;
            newViewModel.LastAction = clickAction;

            listViewModel.SetInteraction(sourceTrigger, callbackIndex, newViewModel);
            listViewModel.RefreshChildrenActive();
        }

        public void SetLogicTypeVisible(eLogicType logicType, bool active)
        {
            if (_logicActiveMap.ContainsKey(logicType))
            {
                _logicActiveMap[logicType] = active;
            }
            else
            {
                _logicActiveMap.Add(logicType, active);
            }

            foreach (var listview in _triggerListViewModelMapping.Values)
            {
                listview.RefreshChildrenActive();
            }
        }

        public bool IsVisible(InteractionUIViewModel model)
        {
            if (_logicActiveMap.TryGetValue(model.LogicType, out bool active))
            {
                if (!active)
                {
                    return false;
                }
            }

            return true;
        }
        public void UnsetInteractionUI(C2VEventTrigger sourceTrigger, int callbackIndex)
        {
            Transform triggerParent = sourceTrigger.transform.parent;
            if (_triggerListViewModelMapping.TryGetValue(triggerParent, out var listViewModel))
            {
                listViewModel.UnsetInteraction(sourceTrigger, callbackIndex);
                _triggerListViewModelMapping.Remove(triggerParent);

                if (_triggerListViewModelMapping.Count == 0)
                {
                    InteractionUIListHolder.InteractionListCollection.RemoveItem(listViewModel);
                }

                listViewModel.RefreshChildrenActive();
            }
        }
        public void UnsetInteractionUIAll()
        {
            foreach (var kvp in _triggerListViewModelMapping)
            {
                var triggerParent = kvp.Key;
                var listViewModel = kvp.Value;

                var sourceTriggers = triggerParent.GetComponentsInChildren<C2VEventTrigger>();
                foreach (var sourceTrigger in sourceTriggers)
                {
                    for (int callbackIndex = 0; callbackIndex < sourceTrigger.Callback.Length; callbackIndex++)
                    {
                        listViewModel.UnsetInteraction(sourceTrigger, callbackIndex);
                        InteractionUIListHolder.InteractionListCollection.RemoveItem(listViewModel);
                        listViewModel.RefreshChildrenActive();
                    }
                }
            }
            _triggerListViewModelMapping.Clear();
        }
        public static long GetInteractionLinkId(long baseObjectId, int triggerIndex, int callbackIndex)
        {
            return long.Parse(ZString.Format(InteractionLinkIdFormat, baseObjectId, triggerIndex + 1, callbackIndex + 1));
        }
    }
}