/*===============================================================
* Product:		Com2Verse
* File Name:	OverrideInputCompositionString.cs
* Developer:	eugene9721
* Date:			2023-06-17 17:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.InputSystem
{
	public sealed class OverrideInputCompositionString : BaseInput
	{
		private bool _enableCompositionString = true;

		public bool EnableCompositionString
		{
			get => _enableCompositionString;
			set
			{
				_enableCompositionString = value;
				// FIXME: CompositionString 비활성화와 동시에 CompositionString Clear필요
				// imeCompositionMode = value ? IMECompositionMode.On : IMECompositionMode.Auto;
			}
		}

		public override string compositionString => EnableCompositionString ? Input.compositionString : string.Empty;
	}
}
