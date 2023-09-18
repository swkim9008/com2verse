/*===============================================================
* Product:		Com2Verse
* File Name:	C2VTreeViewBase.cs
* Developer:	tlghks1009
* Date:			2023-03-02 16:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public abstract class C2VTreeViewBase : TreeView, IDisposable
	{
		private readonly List<TreeViewItem> _treeViewItems = new();

		private TreeViewItem _treeViewItemRoot;

		public GenericMenu RightClickMenu { get; set; }

		public event Action<TreeViewItem> OnSelected;

		protected abstract MultiColumnHeaderState.Column[] Columns { get; }

		public C2VTreeViewBase(TreeViewState state) : base(state)
		{
			_treeViewItemRoot = new TreeViewItem()
			{
				id = -1,
				displayName = "Root",
				depth = -1
			};

			base.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(Columns));
		}


		protected override IList<TreeViewItem> BuildRows(TreeViewItem root) => base.BuildRows(root);

		protected override TreeViewItem BuildRoot() => _treeViewItemRoot;

		public override void OnGUI(Rect rect)
		{
			base.OnGUI(rect);

			// if (multiColumnHeader != null)
			// {
			// 	rect.height -= multiColumnHeader.height;
			// 	rect.y += multiColumnHeader.height;
			// }

			var e = Event.current;
			if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 1)
			{
				RightClickMenu?.ShowAsContext();
			}
		}


		protected override void RowGUI(RowGUIArgs args)
		{
			for (int i = 0; i < args.GetNumVisibleColumns(); i++)
			{
				var rect = args.GetCellRect(i);
				var column = args.GetColumn(i);

				CellGUI(rect, column, args);
			}
		}

		protected override void SelectionChanged(IList<int> selectedIds)
		{
			base.SelectionChanged(selectedIds);

			var treeViewItems = FindRows(selectedIds);
			if (treeViewItems != null && treeViewItems.Count != 0)
			{
				OnSelected?.Invoke(treeViewItems[0]);
			}
		}


		protected virtual void CellGUI(Rect cellRect, int columnIndex, RowGUIArgs args)
		{
			base.RowGUI(args);
		}


		public void AddItem(TreeViewItem item)
		{
			_treeViewItemRoot.AddChild(item);
			_treeViewItems.Add(item);

			Reload();
		}


		public void AddChild(TreeViewItem parent, TreeViewItem current)
		{
			parent.AddChild(current);
			_treeViewItems.Add(current);

			Reload();
		}


		public void RemoveItem(int id)
		{
			foreach (var treeViewItem in _treeViewItems)
			{
				if (treeViewItem.id == id)
				{
					RemoveItem(treeViewItem);
					return;
				}
			}
		}


		public void RemoveItem(TreeViewItem removeItem)
		{
			_treeViewItemRoot.children.Remove(removeItem);
			_treeViewItems.Remove(removeItem);

			Reload();
		}


		public IReadOnlyList<TreeViewItem> Items() => _treeViewItems;


		public void Clear()
		{
			_treeViewItems.Clear();
			_treeViewItemRoot.children?.Clear();
		}

		public void Dispose()
		{
			Clear();

			_treeViewItemRoot = null;
			RightClickMenu = null;
		}
	}
}
