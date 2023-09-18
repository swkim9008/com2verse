/*===============================================================
* Product:    Com2Verse
* File Name:  Device.cs
* Developer:  mikeyid77
* Date:       2022-02-25 09:22
* History:    2022-02-25 - Init
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Logger;

namespace Com2Verse.InputSystem
{
	public sealed class DeviceFactory
	{
#region Fields
		private readonly Dictionary<Type, Device> _factory = new();
#endregion Fields

#region Methods
		public Device SetDevice<T>() where T : Device, new()
		{
			if (_factory.ContainsKey(typeof(T)))
				return _factory[typeof(T)];

			C2VDebug.Log($"create {typeof(T).Name}");
			Device ret = new T();
			_factory.Add(typeof(T), ret);
			return ret;
		}
#endregion Methods
	}

	public abstract class Device
	{
#region Fields
		protected          string       _currentScheme;
		protected          string       _excludingControl;
		protected          string       _escapeBinding;
		protected readonly List<string> _targetModifier = new();
		protected readonly bool[]       _isModifier     = new bool[4];
#endregion Fields

#region Properties
		public string       CurrentScheme    => _currentScheme;
		public string       ExcludingControl => _excludingControl;
		public string       EscapeBinding    => _escapeBinding;
		public List<string> TargetModifier   => _targetModifier;
#endregion Properties

#region Methods
		protected abstract string PrintPath(string path);

		public abstract void   FindModifier(string path);
		public abstract bool   CheckModifier(List<string> currentModifier);
		public abstract string PrintModifier(string path);

		public void ResetModifier()
		{
			for (int i = 0; i < _isModifier.Length; i++) _isModifier[i] = false;
		}
#endregion Methods
	}
}
