/*===============================================================
* Product:		Com2Verse
* File Name:	GraphicsSettingsViewModel.cs
* Developer:	ljk
* Date:			2022-07-22 16:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Rendering;
using Com2Verse.Rendering.GraphicsSettings;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class GraphicsSettingsViewModel : ViewModelBase
	{
		public string TitleString
		{
			get => "그래픽 설정";
			set { }
		}

		public string SaveString
		{
			get => "저장";
			set { }
		}

		public string DiscardString
		{
			get => "원래대로";
			set { }
		}
		public CommandHandler<bool> onWindowClose { get; }
		public CommandHandler       onSaveSetting { get; }

		private Collection<GraphicsSettingsItemViewModel> _settingItemsCollection = new();

		public Collection<GraphicsSettingsItemViewModel> SettingItemsCollection
		{
			get => _settingItemsCollection;
			set => base.InvokePropertyValueChanged(nameof(_settingItemsCollection), value);
		}

		public override void OnInitialize()
		{
			base.OnInitialize();
			_settingItemsCollection.Reset();
			Init();
			GraphicsSettingsManager.Instance.Prepare();
		}

		public GraphicsSettingsViewModel()
		{
			onWindowClose = new CommandHandler<bool>(OnWindowClose);
			onSaveSetting = new CommandHandler(OnSaveSetting);
		}

		private void Init()
		{
			for (int i = 0; i < GraphicsSettingsManager.Instance.EditableGraphicAttributeInfos.Count; i++)
			{
				GraphicAttributeInfo attrinfo = GraphicsSettingsManager.Instance.EditableGraphicAttributeInfos[i];
				GraphicsSettingsItemInfo iteminfo = new GraphicsSettingsItemInfo();
				object propertyValue = GraphicsSettingsManager.Instance.GetRenderingProperty(attrinfo._propertyName);
				try
				{
					if (propertyValue.GetType().IsEnum)
					{
						var parsed = Enum.Parse(propertyValue.GetType(), propertyValue.ToString());
						iteminfo._currentValue = (int)parsed;
						iteminfo._recommendedValueRange = new Vector2(0,Enum.GetNames(propertyValue.GetType()).Length - 1);
					}
					else
					{
						iteminfo._recommendedValueRange = attrinfo._recommendedValueRange;
						if (propertyValue.GetType().Equals(typeof(bool)))
						{
							iteminfo._currentValue = (bool)propertyValue ? 1:0;
							iteminfo._recommendedValueRange = new Vector2(0, 1);
						}
						else if (propertyValue.GetType().Equals(typeof(float)))
							iteminfo._currentValue = (float)propertyValue;
						else if (propertyValue.GetType().Equals(typeof(double)))
							iteminfo._currentValue = (float)(double)propertyValue;
						else if (propertyValue.GetType().Equals(typeof(int)))
							iteminfo._currentValue = (int)propertyValue;
					}
				}
				catch (Exception)
				{
					iteminfo._currentValue = -999;
				}
				
				iteminfo._displayName = attrinfo._attributeDisplayName;
				iteminfo._propertyName = attrinfo._propertyName;
				iteminfo._valueType = propertyValue.GetType();
				
				_settingItemsCollection.AddItem(new GraphicsSettingsItemViewModel(iteminfo,OnGraphicsSettingChanged));
			}
		}

		private void OnWindowClose(bool save)
		{
			GraphicsSettingsManager.Instance.DiscardModifiedAndRestore();
		}

		private void OnSaveSetting()
		{
			GraphicsSettingsManager.Instance.SaveModified();
		}

		private void OnQualityChangeComplete()
		{
			_settingItemsCollection.Reset();
			GraphicsSettingsManager.Instance.RefreshQualityAsset();
			Init();
		}
		private void OnGraphicsSettingChanged(GraphicsSettingsItemInfo changedinfo)
		{
			if (changedinfo._valueType.IsEnum)
			{
				var valueToOriginal = Enum.Parse(changedinfo._valueType,""+(int)changedinfo._currentValue);
				GraphicsSettingsManager.Instance.SetRenderingProperty(changedinfo._propertyName,valueToOriginal);
			}
			else
			{
				object valueToOriginal = new object();
				if (changedinfo._valueType.Equals(typeof(bool)))
					valueToOriginal = changedinfo._currentValue == 0 ? false : true;
				else if(changedinfo._valueType.Equals(typeof(int)))
					valueToOriginal = (int)changedinfo._currentValue;
				else if (changedinfo._valueType.Equals(typeof(double)))
					valueToOriginal = (double)changedinfo._currentValue;
				else
					valueToOriginal = changedinfo._currentValue;
				GraphicsSettingsManager.Instance.SetRenderingProperty(changedinfo._propertyName,valueToOriginal);

				if (changedinfo._propertyName.Equals("qualityPreset"))
				{
					GraphicsSettingsManager.Instance._onQualitySettingChanged = OnQualityChangeComplete;
				}
			}
		}
	}
}
