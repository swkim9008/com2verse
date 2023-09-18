/*===============================================================
* Product:		Com2Verse
* File Name:	MaterialEffectManager.cs
* Developer:	ljk
* Date:			2022-10-24 14:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Com2Verse.Rendering
{
	public sealed class MaterialEffectManager : MonoSingleton<MaterialEffectManager>
	{
		public enum eMaterialEffect
		{
			AVATAR_DISSOLVE
		}

		public class MaterialEffectInfo
		{
			public GlobalKeyword _keywordGlobal;
			public string _property;
			public float _propertyValue;
			public float _minPropertyValue;
			public float _maxPropertyValue;
			public bool _keywordStatus;

			public MaterialEffectInfo(string keyword)
			{
				_keywordGlobal = GlobalKeyword.Create(keyword);
			}
			
			public void Apply()
			{
				float value = Mathf.Clamp(_propertyValue, _minPropertyValue, _maxPropertyValue);
				Shader.SetGlobalFloat(_property,value);
			}

			public void Enable(bool flag)
			{
				_keywordStatus = flag;
				if (_keywordStatus)
				{
					Shader.EnableKeyword(_keywordGlobal);
				}
				else
				{
					Shader.DisableKeyword(_keywordGlobal);
				}
			}
		}
		
		private Dictionary<eMaterialEffect, MaterialEffectInfo> _materialStatesRunningGlobal;
		public void Initialize()
		{
			_materialStatesRunningGlobal = new Dictionary<eMaterialEffect, MaterialEffectInfo>()
			{
				{
					eMaterialEffect.AVATAR_DISSOLVE,
					new MaterialEffectInfo("_AVATAR_DISSOLVE")
					{
						_property = "_DISSOLVE_AMOUNT",
						_minPropertyValue = 0,
						_maxPropertyValue = 1
					}
				}
			};
		}

		public void RequestGlobalMaterialEffect(string materialEffectName,Action onEffectComplete,Action onEffectStart)
		{
			C2VAddressables.LoadAssetAsync<GameObject>(materialEffectName).OnCompleted += (operationHandle) =>
			{
				var returned = operationHandle.Result;
				GameObject objectInstance = Instantiate(returned);
				MaterialEffectContol control = objectInstance.GetComponent<MaterialEffectContol>();
				control.Run(onEffectComplete, onEffectStart);
			};
		}
		
		public void SetGlobalMaterialState(eMaterialEffect effect,float propertyValue)
		{
			if (_materialStatesRunningGlobal.TryGetValue(effect, out MaterialEffectInfo info))
			{
				info._propertyValue = propertyValue;
				
				info.Apply();
			}
		}

		public void SetGlobalMaterialState(eMaterialEffect effect, bool enable)
		{
			if (_materialStatesRunningGlobal.TryGetValue(effect, out MaterialEffectInfo info))
			{
				info._propertyValue = info._minPropertyValue;
				
				info.Enable(enable);
			}
		}
	}
}
