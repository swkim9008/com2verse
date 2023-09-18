/*===============================================================
* Product:		Com2Verse
* File Name:	ObjectDissolve.cs
* Developer:	ljk
* Date:			2022-10-24 15:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.Rendering
{
	// Animation 으로 컨트롤됨.
	public sealed class MaterialEffectContol : MonoBehaviour
	{
		public MaterialEffectManager.eMaterialEffect _effect;
		public bool _isRunningGlobal = true;
		[HideInInspector]
		public float _propertyValue;
		private Action _onEffectEndTriggered;

		public void Run(Action onEffectEndTriggered,Action onEffectStart)
		{
			onEffectStart?.Invoke();
			_onEffectEndTriggered = onEffectEndTriggered;
			if(_isRunningGlobal)
				MaterialEffectManager.Instance.SetGlobalMaterialState(_effect,true);
		}
		
		private void OnDidApplyAnimationProperties()
		{
			if(_isRunningGlobal)
				MaterialEffectManager.Instance.SetGlobalMaterialState(_effect,_propertyValue);
		}

		private void OnDisable()
		{
			if(_isRunningGlobal)
				MaterialEffectManager.Instance.SetGlobalMaterialState(_effect,false);
			Destroy(gameObject);
		}

		public void OnAnimationTrigger()
		{
			_onEffectEndTriggered?.Invoke();
		}
	}
}
