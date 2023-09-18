/*===============================================================
* Product:		Com2Verse
* File Name:	LayoutExtensions.cs
* Developer:	eugene9721
* Date:			2023-05-03 11:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] LayoutExtensions")]
	public sealed class LayoutExtensions : MonoBehaviour
	{
		[UsedImplicitly]
		public bool SetForceRebuild
		{
			get => transform is RectTransform;
			set
			{
				if (!SetForceRebuild || !value) return;

				LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
			}
		}

		[ContextMenu("SetForceRebuildImmediate")]
		public void SetForceRebuildImmediate()
		{
			if (!SetForceRebuild) return;

			LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
		}
	}
}
