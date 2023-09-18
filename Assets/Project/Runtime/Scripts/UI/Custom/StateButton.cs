/*===============================================================
* Product:    Com2Verse
* File Name:  StateButton.cs
* Developer:  haminjeong
* Date:       2022-07-29 18:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.UI
{
    [Serializable]
    public class StateButtonEvent : UnityEvent<int> { }

    [AddComponentMenu("[CVUI]/[CVUI] StateButton")]
    [ExecuteInEditMode]
    public class StateButton : UIBehaviour, IPointerClickHandler
    {
        public enum Transition
        {
            Color = 0,
            Sprite,
            Color_N_Sprite
        }

        public bool Interactable { get; set; }

        [SerializeField] private List<string> _stateList = new List<string>();

        [SerializeField] private Graphic _targetGraphic;

        [SerializeField] [HideInInspector]
        private List<Sprite> _stateSpriteList = new List<Sprite>();

        [SerializeField] [HideInInspector]
        private List<Color> _stateColorList = new List<Color>();

        [SerializeField] private Graphic _subGraphic;

        [SerializeField] [HideInInspector]
        private List<Sprite> _stateSubSpriteList = new List<Sprite>();

        [SerializeField] [HideInInspector]
        private List<Color> _stateSubColorList = new List<Color>();

        [SerializeField] [HideInInspector]
        private int _transition = 0;

        [SerializeField] [HideInInspector]
        private int _subTransition = 0;

        [SerializeField] [HideInInspector]
        private int _currentIndex = -1;

        public StateButtonEvent onClick;
        public StateButtonEvent onValueChanged;

        private Action<int> _clickCallback;

        public string State
        {
            get
            {
                if (_stateList.Count < 0) return "NONE";
                return _stateList[_currentIndex];
            }
            set
            {
                int newIdx = _stateList.FindIndex((iter) => iter.Equals(value));
                if (newIdx >= 0)
                    Index = newIdx;
            }
        }

        public int Index
        {
            get { return _currentIndex; }
            set
            {
                if (value < 0 || value >= _stateList.Count) return;
                if (_currentIndex != value)
                    onValueChanged?.Invoke(value);
                _currentIndex = value;
                switch ((Transition)_transition)
                {
                    case Transition.Color:
                        SetColorByState(_targetGraphic);
                        break;
                    case Transition.Sprite:
                        SetSpriteByState(_targetGraphic);
                        break;
                    case Transition.Color_N_Sprite:
                        SetSpriteByState(_targetGraphic);
                        SetColorByState(_targetGraphic);
                        break;
                }

                switch ((Transition)_subTransition)
                {
                    case Transition.Color:
                        SetColorByState(_subGraphic);
                        break;
                    case Transition.Sprite:
                        SetSpriteByState(_subGraphic);
                        break;
                    case Transition.Color_N_Sprite:
                        SetSpriteByState(_subGraphic);
                        SetColorByState(_subGraphic);
                        break;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Interactable == false) return;
            onClick.Invoke(Index);
        }

        private void DefaultOnClick()
        {
            Index = Index + 1 >= _stateList.Count ? 0 : Index + 1;
            _clickCallback?.Invoke(Index);
        }

        public void SetOnClick(Action<int> callback)
        {
            _clickCallback = callback;
        }

        public void SetOnValueChanged(Action<int> callback)
        {
            onValueChanged.AddListener((idx) => callback?.Invoke(Index));
        }

        protected override void Awake()
        {
            onClick = new StateButtonEvent();
            onClick.AddListener((idx) => DefaultOnClick());
            if (_stateList.Count == 0)
            {
                _stateList.Add("Default");
                State = "Default";
            }
        }

        private void SetColorByState(Graphic graphic)
        {
            if (_currentIndex < 0 || _currentIndex >= _stateColorList.Count) return;
            if (!graphic.IsReferenceNull())
                graphic.color = graphic == _targetGraphic ? _stateColorList[_currentIndex] : _stateSubColorList[_currentIndex];
        }

        private void SetSpriteByState(Graphic graphic)
        {
            if (_currentIndex < 0 || _currentIndex >= _stateSpriteList.Count) return;
            if (graphic.IsReferenceNull()) return;
            Image image = graphic as Image;
            if (!image.IsReferenceNull())
            {
                image.sprite = image == _targetGraphic ? _stateSpriteList[_currentIndex] : _stateSubSpriteList[_currentIndex];
                image.SetNativeSize();
            }

            graphic.color = Color.white;
        }
    }
}
