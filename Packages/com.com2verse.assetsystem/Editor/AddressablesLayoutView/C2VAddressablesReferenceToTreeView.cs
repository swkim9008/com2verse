/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesReferenceToTreeView.cs
* Developer:	tlghks1009
* Date:			2023-03-23 11:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2VerseEditor.AssetSystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Com2Verse
{
	public sealed class C2VAddressablesReferenceToTreeView : C2VAddressablesLayoutTreeViewBase
	{
		public C2VAddressablesReferenceToTreeView(TreeViewState state) : base(state)
		{
			showAlternatingRowBackgrounds = true;
			rowHeight = EditorGUIUtility.singleLineHeight + 4;

			Reload();
		}


		protected override MultiColumnHeaderState.Column[] Columns
		{
			get
			{
				var assets = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("번들"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 600,
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
	}
}
