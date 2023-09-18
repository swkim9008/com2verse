/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesViewerTreeView.cs
* Developer:	tlghks1009
* Date:			2023-03-21 12:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.IMGUI.Controls;

namespace Com2VerseEditor.AssetSystem
{
	public enum eLayoutItemType
	{
		ASSET,
		BUNDLE
	}

	public sealed class C2VAddressableAssetLayoutTreeItem : TreeViewItem
	{
		public eLayoutItemType LayoutItemType { get; set; }
		public string Guid { get; set; }
		public string AssetPath { get; set; }
		public int RectX { get; set; }
	}

	public abstract class C2VAddressablesLayoutTreeViewBase : C2VTreeViewBase
	{
		public C2VAddressablesLayoutTreeViewBase(TreeViewState state) : base(state) { }

		public C2VAddressableAssetLayoutTreeItem CreateLayoutTreeItem(eLayoutItemType layoutItemType, string displayName, int rectX, int depth)
		{
			var item = new C2VAddressableAssetLayoutTreeItem();
			item.id = item.GetHashCode();
			item.LayoutItemType = layoutItemType;
			item.displayName = displayName;
			item.RectX = rectX;
			item.depth = depth;

			return item;
		}


		public void DrawSearchField()
		{
			searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarSearchField, GUILayout.Width(500));
		}
	}


	public sealed class C2VAddressablesAssetTreeView : C2VAddressablesLayoutTreeViewBase
	{
		public C2VAddressablesAssetTreeView(TreeViewState state) : base(state)
		{
			showAlternatingRowBackgrounds = true;
			rowHeight = EditorGUIUtility.singleLineHeight + 4;

			InitializeTreeItems();

			Reload();
		}


		protected override MultiColumnHeaderState.Column[] Columns
		{
			get
			{
				var assets = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("에셋"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 500,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				return new[] {assets};
			}
		}


		protected override void CellGUI(Rect cellRect, int columnIndex, RowGUIArgs args)
		{
			cellRect.height -= 4;
			cellRect.y += 2;

			var item = (C2VAddressableAssetLayoutTreeItem) args.item;
			cellRect.x += item.RectX;

			switch (columnIndex)
			{
				case 0:
				{
					EditorGUI.LabelField(cellRect, item.displayName);
				}
					break;
			}
		}

		private void InitializeTreeItems()
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;

			foreach (var bundleGroup in settings.groups)
			{
				var groupItem = CreateLayoutTreeItem(eLayoutItemType.BUNDLE, bundleGroup.name, 15, 0);

				base.AddItem(groupItem);

				foreach (var entry in bundleGroup.entries)
				{
					var entryItem = CreateLayoutTreeItem(eLayoutItemType.ASSET, entry.address, 30, 1);
					entryItem.Guid = entry.guid;
					entryItem.AssetPath = entry.AssetPath;

					base.AddChild(groupItem, entryItem);
				}
			}
		}
	}
}
