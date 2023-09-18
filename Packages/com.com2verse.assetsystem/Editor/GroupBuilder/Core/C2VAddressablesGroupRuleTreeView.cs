/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressableGroupRuleTreeView.cs
* Developer:	tlghks1009
* Date:			2023-03-02 16:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesGroupRuleTreeView : C2VTreeViewBase
	{
		private enum eCellType
		{
			PROJECT_ROOT_PATH,
			ROOT_PATH,
			RULE,
			GROUP_NAME,
			ADDRESSABLE_TYPE,
			LABEL,
		}

		public C2VAddressablesGroupRuleTreeView(TreeViewState treeViewState) : base(treeViewState)
		{
			showAlternatingRowBackgrounds = true;
			rowHeight = EditorGUIUtility.singleLineHeight + 8;

			Reload();
		}


		protected override MultiColumnHeaderState.Column[] Columns
		{
			get
			{
				var projectRootFolder = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("프로젝트 루트 경로"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 150,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				var rootPath = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("루트 경로"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 200,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				var pathRule = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("규칙"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 300,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				var groupName = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("그룹 이름"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 200,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				var addressType = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("주소 타입"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 200,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				var label = new MultiColumnHeaderState.Column()
				{
					headerContent = new GUIContent("에셋 번들"),
					headerTextAlignment = TextAlignment.Center,
					canSort = true,
					width = 200,
					minWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				};

				return new[] {projectRootFolder, rootPath, pathRule, groupName, addressType, label};
			}
		}


		protected override void CellGUI(Rect cellRect, int columnIndex, RowGUIArgs args)
		{
			cellRect.height -= 4;
			cellRect.y += 2;

			var cellType = (eCellType) columnIndex;
			var item = (C2VAddressableGroupRuleTreeItem) args.item;

			switch (cellType)
			{
				case eCellType.PROJECT_ROOT_PATH:
					item.ProjectRootPath = (eProjectRootPath) EditorGUI.EnumPopup(cellRect, item.ProjectRootPath);
					break;

				case eCellType.ROOT_PATH:
					item.RootPath = EditorGUI.TextField(cellRect, item.RootPath);
					break;

				case eCellType.RULE:
					item.PathRule = EditorGUI.TextField(cellRect, item.PathRule);
					break;

				case eCellType.GROUP_NAME:
					item.GroupName = EditorGUI.TextField(cellRect, item.GroupName);
					break;

				case eCellType.ADDRESSABLE_TYPE:
					item.AddressType = (eAddressType) EditorGUI.EnumPopup(cellRect, item.AddressType);
					break;

				case eCellType.LABEL:
					item.Label = (eAssetBundleType) EditorGUI.EnumPopup(cellRect, item.Label);
					break;
			}
		}
	}
}
