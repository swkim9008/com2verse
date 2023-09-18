/*===============================================================
* Product:		Com2Verse
* File Name:	CanvasGroupPropertyExtensions.cs
* Developer:	urun4m0r1
* Date:			2023-04-10 10:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("[DB]/[DB] CanvasGroupPropertyExtensions")]
	public class CanvasGroupPropertyExtensions : MonoBehaviour
	{
		public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>()!;

		private CanvasGroup? _canvasGroup;

		public bool Interactable
		{
			get => CanvasGroup.interactable;
			set => CanvasGroup.interactable = value;
		}

		public bool BlocksRaycasts
		{
			get => CanvasGroup.blocksRaycasts;
			set => CanvasGroup.blocksRaycasts = value;
		}

		public bool IgnoreParentGroups
		{
			get => CanvasGroup.ignoreParentGroups;
			set => CanvasGroup.ignoreParentGroups = value;
		}

		public float Alpha
		{
			get => CanvasGroup.alpha;
			set => CanvasGroup.alpha = value;
		}

		public bool InteractableAndBlocksRaycasts
		{
			get => CanvasGroup is { interactable: true, blocksRaycasts: true };
			set
			{
				CanvasGroup.interactable   = value;
				CanvasGroup.blocksRaycasts = value;
			}
		}

		public bool InteractableReverse
		{
			get => !Interactable;
			set => Interactable = !value;
		}

		public bool BlocksRaycastsReverse
		{
			get => !BlocksRaycasts;
			set => BlocksRaycasts = !value;
		}

		public bool IgnoreParentGroupsReverse
		{
			get => !IgnoreParentGroups;
			set => IgnoreParentGroups = !value;
		}

		public bool InteractableAndBlocksRaycastsReverse
		{
			get => !InteractableAndBlocksRaycasts;
			set => InteractableAndBlocksRaycasts = !value;
		}
	}
}
