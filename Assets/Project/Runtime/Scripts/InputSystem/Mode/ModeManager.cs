/*===============================================================
* Product:		Com2Verse
* File Name:	ModeManager.cs
* Developer:	haminjeong
* Date:			2022-05-25 15:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.Logger;

namespace Com2Verse.Project.InputSystem
{
	public enum eModeType
	{
		NORMAL = 0,
		CONFERENCE,
	}

	public sealed class ModeManager : MonoSingleton<ModeManager>
	{
		private readonly Dictionary<eModeType, BaseModeAction> _modeDataDic = new();

		private bool           _isInitialize = false;
		private BaseModeAction _currentMode;
		public  BaseModeAction CurrentMode => _currentMode;

		public void Initialize()
		{
			if (_isInitialize)
				return;
			_isInitialize = true;
			var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(InputModeAttribute)));
			RegisterModeActions(types);
		}

		private void RegisterModeActions(IEnumerable<Type> types)
		{
			C2VDebug.Log("REGISTER Mode Actions");
			foreach (var type in types)
			{
				C2VDebug.Log($"-> {type.Name}");
				BaseModeAction modeAction = Activator.CreateInstance(type) as BaseModeAction;
				modeAction.Initialize();
				_modeDataDic.TryAdd(type.GetCustomAttribute<InputModeAttribute>().InputModeType, modeAction);
				C2VDebug.Log($"-> {type.Name}...DONE");
			}
		}

		public eModeType SetMode(eModeType type, Action onCameraChangeComplete = null)
		{
			if (!_modeDataDic.TryGetValue(type, out var value)) return eModeType.NORMAL;
			_currentMode?.RestoreSetting();
			var prevMode = _currentMode?.CurrentMode ?? eModeType.NORMAL;
			value.ApplyMode(onCameraChangeComplete);
			_currentMode = value;
			return prevMode;
		}
	}
}
