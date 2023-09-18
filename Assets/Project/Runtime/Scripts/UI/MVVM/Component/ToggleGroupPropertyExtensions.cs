/*===============================================================
* Product:		Com2Verse
* File Name:	ToggleGroupPropertyExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-14 17:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(ToggleGroup))]
	public sealed class ToggleGroupPropertyExtensions : MonoBehaviour
	{
		private ToggleGroup _toggleGroup;
		
		private void Awake()
		{
			_toggleGroup = GetComponent<ToggleGroup>();
		}
	}
}
