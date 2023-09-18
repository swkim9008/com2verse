/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesBaseLayoutView.cs
* Developer:	tlghks1009
* Date:			2023-03-23 10:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Com2VerseEditor.AssetSystem
{
	public abstract class C2VAddressablesBaseLayoutView
	{
		public C2VAddressablesLayoutWindow Window { get; set; }

		protected TreeViewState TreeViewState { get; set; }

		protected C2VAddressablesLayoutTreeViewBase ReferenceByViewTree { get; set; }

		public abstract void Show();

		public abstract void Hide();

		public virtual void OnGUI() { }


		public void SetSelectedItem(TreeViewItem item)
		{
			ReferenceByViewTree.Clear();

			var layoutTreeItem = item as C2VAddressableAssetLayoutTreeItem;

			switch (layoutTreeItem.LayoutItemType)
			{
				case eLayoutItemType.BUNDLE:
				{
					FindReferenceByGroup(layoutTreeItem);
				}
					break;
				case eLayoutItemType.ASSET:
				{
					FindReference(layoutTreeItem);
				}
					break;
			}
		}


		protected virtual void FindReferenceByGroup(C2VAddressableAssetLayoutTreeItem layoutTreeItem) { }

		protected virtual void FindReference(C2VAddressableAssetLayoutTreeItem layoutTreeItem) { }

		protected void AddItem(string displayName)
		{
			var item = ReferenceByViewTree.CreateLayoutTreeItem(eLayoutItemType.BUNDLE, displayName, 15, 0);

			ReferenceByViewTree.AddItem(item);
		}
	}
}
