/*===============================================================
* Product:		Com2Verse
* File Name:	GraphicsSettingsItemViewModel.cs
* Developer:	ljk
* Date:			2022-07-22 18:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Rendering.GraphicsSettings;

namespace Com2Verse.UI
{
	public sealed class GraphicsSettingsItemViewModel : ViewModelBase
	{
		private float _propertyValue;
		private string _propertyName;
		private string _propertyRange;
		private GraphicsSettingsItemInfo _itemInfo;
		private Action<GraphicsSettingsItemInfo> _onValueChanged;

		public GraphicsSettingsItemViewModel(GraphicsSettingsItemInfo itemInfo,Action<GraphicsSettingsItemInfo> onValueChanged)
		{
			_itemInfo = itemInfo;
			_onValueChanged = onValueChanged;
		}
		
		public bool IsSliderMovesHoleNumber
		{
			get
			{
				return (!_itemInfo._valueType.Equals(typeof(float)));
			}
			set { }
		}

		public float SliderMaxValue
		{
			get => _itemInfo._recommendedValueRange.y;
			set { }
		}

		public float SliderMinValue
		{
			get => _itemInfo._recommendedValueRange.x;
			set { }
		}

		public string ItemName
		{
			get => _itemInfo._displayName;
			set { }
		}

		public string ValueDescription
		{
			get
			{
				if (IsSliderMovesHoleNumber)
					return (int)_itemInfo._recommendedValueRange.x + " - " + (int)_itemInfo._recommendedValueRange.y;
				
				return _itemInfo._recommendedValueRange.x.ToString("F3") + " - " + _itemInfo._recommendedValueRange.y.ToString("F3");
			} 
			set {}
		}

		public float SliderValue
		{
			get => _itemInfo._currentValue;
			set
			{
				_itemInfo._currentValue = value;
				_onValueChanged(_itemInfo);
				base.InvokePropertyValueChanged(nameof(SliderValue), value);
				base.InvokePropertyValueChanged(nameof(SliderValueDescriptive), SliderValueDescriptive);
			}
		}

		public string SliderValueDescriptive
		{
			get
			{
				if (_itemInfo._currentValue <= -999)
				{
					return "Init-Failed";
				}

				if (_itemInfo._valueType.IsEnum)
				{
					return Enum.GetNames(_itemInfo._valueType)[(int)_itemInfo._currentValue];
				}

				if (_itemInfo._valueType.Equals(typeof(bool)))
				{
					return (int)_itemInfo._currentValue == 0 ? "false" : "true";
				}

				if(_itemInfo._valueType.Equals(typeof(int))) // description set
					return ""+(int)_itemInfo._currentValue;

				return _itemInfo._currentValue.ToString("F3");
			}
			set { }
		}
	}
}
