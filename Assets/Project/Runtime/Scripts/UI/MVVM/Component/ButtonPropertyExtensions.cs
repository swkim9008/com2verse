/*===============================================================
* Product:		Com2Verse
* File Name:	ButtonPropertyExtensions.cs
* Developer:	eugene9721
* Date:			2022-10-16 16:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] ButtonPropertyExtensions")]
	[RequireComponent(typeof(MetaverseButton))]
	public sealed class ButtonPropertyExtensions : MonoBehaviour
	{
		private MetaverseButton _button;

		private void Awake()
		{
			_button = GetComponent<MetaverseButton>();
		}

		[UsedImplicitly]
		public bool ClickProperty
		{
			get => false;
			set
			{
				if(value) _button.onClick?.Invoke();
			}
		}
	}
}
