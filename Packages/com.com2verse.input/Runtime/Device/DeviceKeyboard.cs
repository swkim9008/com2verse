/*===============================================================
* Product:    Com2Verse
* File Name:  DeviceKeyboard.cs
* Developer:  mikeyid77
* Date:       2022-02-25 09:22
* History:    2022-02-25 - Init
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Com2Verse.InputSystem
{
	public sealed class DeviceKeyboard : Device
	{
#region Constructor
		public DeviceKeyboard()
		{
			_currentScheme    = "Keyboard & Mouse";
			_excludingControl = "<Mouse>";
			_escapeBinding    = "<Keyboard>/escape";

			// TODO : Modifier 추가 시 해제
			// _targetModifier.Add("<Keyboard>/leftShift");
			// _targetModifier.Add("<Keyboard>/leftCtrl");
			// _targetModifier.Add("<Keyboard>/leftAlt");
			// _targetModifier.Add("<Keyboard>/rightShift");
			// _targetModifier.Add("<Keyboard>/rightCtrl");
			// _targetModifier.Add("<Keyboard>/rightAlt");
		}
#endregion Constructor

#region Override
		public override void FindModifier(string path)
		{
			if (path.Contains("Shift", StringComparison.OrdinalIgnoreCase)) _isModifier[0]     = true;
			else if (path.Contains("Ctrl", StringComparison.OrdinalIgnoreCase)) _isModifier[1] = true;
			else if (path.Contains("Alt", StringComparison.OrdinalIgnoreCase)) _isModifier[2]  = true;
		}

		public override bool CheckModifier(List<string> currentModifier)
		{
			return !(_isModifier[0] ^ currentModifier.Exists(modifier => modifier.EndsWith("Shift", StringComparison.OrdinalIgnoreCase))) &&
			       !(_isModifier[1] ^ currentModifier.Exists(modifier => modifier.EndsWith("Ctrl", StringComparison.OrdinalIgnoreCase))) &&
			       !(_isModifier[2] ^ currentModifier.Exists(modifier => modifier.EndsWith("Alt", StringComparison.OrdinalIgnoreCase)));
		}

		public override string PrintModifier(string path)
		{
			if (_isModifier.All(check => !check))
				return PrintPath(path);
			else
			{
				string modifier              = "";
				if (_isModifier[0]) modifier += "Shift + ";
				if (_isModifier[1]) modifier += "Ctrl + ";
				if (_isModifier[2]) modifier += "Alt + ";
				return modifier + "\n" + PrintPath(path);
			}
		}

		protected override string PrintPath(string path)
		{
			string tmp = path.Split('/')[1];
			string ret = "";

			if (tmp.Contains("Arrow", StringComparison.OrdinalIgnoreCase))
			{
				switch (tmp.Replace("Arrow", "", StringComparison.OrdinalIgnoreCase))
				{
					case "up":
						ret = "↑";
						break;
					case "down":
						ret = "↓";
						break;
					case "left":
						ret = "←";
						break;
					case "right":
						ret = "→";
						break;
				}
			}
			else
			{
				TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
				ret = ti.ToTitleCase(tmp);
			}

			return ret;
		}
#endregion Override
	}
}
