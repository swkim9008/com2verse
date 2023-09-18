/*===============================================================
* Product:    Com2Verse
* File Name:  DeviceGamepad.cs
* Developer:  mikeyid77
* Date:       2022-02-25 09:22
* History:    2022-02-25 - Init
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.InputSystem
{
	public sealed class DeviceGamepad : Device
	{
#region Constructor
		public DeviceGamepad()
		{
			_currentScheme    = "GamePad";
			_excludingControl = "<Mouse>";
			_escapeBinding    = "<Gamepad>/select";

			_targetModifier.Add("<Gamepad>/leftTrigger");
			_targetModifier.Add("<Gamepad>/leftShoulder");
			_targetModifier.Add("<Gamepad>/rightTrigger");
			_targetModifier.Add("<Gamepad>/rightShoulder");
		}
#endregion Constructor

#region Override
		public override void FindModifier(string path)
		{
			if (path.Equals("LeftTrigger", StringComparison.OrdinalIgnoreCase)) _isModifier[0]        = true;
			else if (path.Equals("LeftShoulder", StringComparison.OrdinalIgnoreCase)) _isModifier[1]  = true;
			else if (path.Equals("RightTrigger", StringComparison.OrdinalIgnoreCase)) _isModifier[2]  = true;
			else if (path.Equals("RightShoulder", StringComparison.OrdinalIgnoreCase)) _isModifier[3] = true;
		}

		public override bool CheckModifier(List<string> currentModifier)
		{
			return !(_isModifier[0] ^ currentModifier.Contains("<Gamepad>/leftTrigger")) &&
			       !(_isModifier[1] ^ currentModifier.Contains("<Gamepad>/leftShoulder")) &&
			       !(_isModifier[2] ^ currentModifier.Contains("<Gamepad>/rightTrigger")) &&
			       !(_isModifier[3] ^ currentModifier.Contains("<Gamepad>/rightShoulder"));
		}

		public override string PrintModifier(string path)
		{
			if (_isModifier.All(check => !check))
				return PrintPath(path);
			else
			{
				string modifier              = "";
				if (_isModifier[0]) modifier += "LT + ";
				if (_isModifier[1]) modifier += "LS + ";
				if (_isModifier[2]) modifier += "RT + ";
				if (_isModifier[3]) modifier += "RS + ";
				return modifier + "\n" + PrintPath(path);
			}
		}

		protected override string PrintPath(string path)
		{
			return path.Split('/')[1];
		}
#endregion Override
	}
}
