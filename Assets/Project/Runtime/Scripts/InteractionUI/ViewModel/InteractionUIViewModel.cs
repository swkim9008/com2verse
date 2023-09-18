using System;
using Com2Verse.Data;
using Com2Verse.Interaction;
using UnityEngine;

namespace Com2Verse.UI
{
    [ViewModelGroup("EventTrigger")]
    public class InteractionUIViewModel : ViewModelBase
    {
#region Variables
        private string _description;
        private Sprite _iconImage;
        private Color _color;

        private eLogicType _logicType;
        private bool _active = true;
        private RectTransform _targetCanvas;
#endregion // Variables
        
#region Property
        public InteractionUIListViewModel ParentModel { private get; set; }

        public UIInfo.eInfoType InteractionInfoType { get; set; }
        public InteractionStringParameterSource ParameterSource { get; set; }

        public Vector3 CanvasCenter => new Vector3(_targetCanvas.sizeDelta.x / 2, _targetCanvas.sizeDelta.y / 2, 0);
        public virtual bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        public RectTransform TargetCanvas
        {
            set { _targetCanvas = value; }
        }

        public eLogicType LogicType
        {
            get => _logicType;
            set => SetProperty(ref _logicType, value);
        }

        public string Description
        {
            get
            {
                if (ParameterSource != null)
                {
                    switch (ParameterSource.Length)
                    {
                        case 1:
                            return Localization.Instance.GetString(_description, ParameterSource.GetParameter(0));
                        case 2:
                            return Localization.Instance.GetString(_description, ParameterSource.GetParameter(0), ParameterSource.GetParameter(1));
                        case 3:
                            return Localization.Instance.GetString(_description, ParameterSource.GetParameter(0), ParameterSource.GetParameter(1), ParameterSource.GetParameter(2));
                    }
                }

                return Localization.Instance.GetString(_description);
            }
            set => SetProperty(ref _description, value);
        }

        public Sprite IconImage
        {
            get => _iconImage;
            set => SetProperty(ref _iconImage, value);
        }
#endregion

        public CommandHandler Command_ExecuteTrigger { get; }
        
        public Action LastAction { get; set; }
        
        public InteractionUIViewModel()
        {
            Command_ExecuteTrigger = new CommandHandler(OnClick);
        }

        public void Refresh()
        {
            InvokePropertyValueChanged(nameof(Active), Active);
            InvokePropertyValueChanged(nameof(Description), Description);
        }

        private void OnClick()
        {
            LastAction.Invoke();
        }

        public override void OnLanguageChanged()
        {
            Refresh();
        }
    }
}
