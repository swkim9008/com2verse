/*===============================================================
* Product:		Com2Verse
* File Name:	ControlOptionNavigator.cs
* Developer:	mikeyid77
* Date:			2023-04-14 15:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.Option
{
	public sealed class ControlOptionNavigator : MonoBehaviour
	{
		public Toggle[] ToggleGroup;
		
		public int InterfaceSizeIndex
		{
			get => 0;
			set => ToggleGroup[value].isOn = true;
		}
	}
}
