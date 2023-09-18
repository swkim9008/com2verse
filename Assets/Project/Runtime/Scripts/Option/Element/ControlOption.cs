/*===============================================================
* Product:		Com2Verse
* File Name:	ControlOption.cs
* Developer:	mikeyid77
* Date:			2023-04-14 12:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.Option
{
	[Serializable] [MetaverseOption("ControlOption")]
	public sealed class ControlOption : BaseMetaverseOption
	{
		private const int Init = 2;
		private static Vector2 _targetVector = new(1920f, 1080f);
		private Dictionary<int, Vector2> _interfaceSizeDict = new()
		{
			[0] = _targetVector * 1.2f,
			[1] = _targetVector * 1.1f,
			[2] = _targetVector * 1.0f,
			[3] = _targetVector * 0.95f,
			[4] = _targetVector * 0.9f,
		};
		
		public int _interfaceSizeIndex = Init;

		public int InterfaceSizeIndex
		{
			get => _interfaceSizeIndex;
			set
			{
				_interfaceSizeIndex = value;
				UIManager.Instance.ResizeInterface();
			}
		}

		public Vector2 GetInterfaceSizeVector() => _interfaceSizeDict[InterfaceSizeIndex];
		public int Reset() => Init;
	}
}
