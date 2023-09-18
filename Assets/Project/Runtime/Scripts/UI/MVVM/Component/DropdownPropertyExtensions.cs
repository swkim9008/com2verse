/*===============================================================
* Product:		Com2Verse
* File Name:	DropdownPropertyExtensions.cs
* Developer:	ksw
* Date:			2023-07-13 16:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] DropdownPropertyExtensions")]
	[RequireComponent(typeof(MetaverseDropdown))]
	public sealed class DropdownPropertyExtensions : MonoBehaviour
	{
		private MetaverseDropdown _dropdown;
		private void Awake()
		{
			_dropdown = GetComponent<MetaverseDropdown>();
		}

		[UsedImplicitly]
		public MetaverseDropdown Dropdown
		{
			get => _dropdown;
			set => _dropdown = value;
		}
	}
}
