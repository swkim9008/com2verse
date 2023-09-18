/*===============================================================
* Product:		Com2Verse
* File Name:	Tree.cs
* Developer:	jhkim
* Date:			2022-07-20 13:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com2Verse.Logger;

namespace Com2Verse.Organization
{
	/// <summary>
	/// 계층 트리를 생성하는 클래스입니다.
	/// 기능
	/// - 트리 내 노드 인덱싱 및 인덱스를 통한 노드 탐색
	/// - 각 노드별 Fold/UnFold 및 Visible 처리
	/// - 특정 노드에서부터 노드를 순회하는 IEnumerable&lt;T&gt; 제공
	/// - ForwardEnumerator
	/// - BackwardEnumerator
	/// - AscentEnumerator
	/// - DescentEnumerator
	/// </summary>
	/// <typeparam name="T">임의의 데이터 타입</typeparam>
	public class HierarchyTree<T> : IDisposable
	{
#region Variables
		private HierarchyTree<T> _parent;
		private List<HierarchyTree<T>> _children;
		private T _value;
		private int _index;
#endregion // Variables

#region Properties
		public HierarchyTree<T> Root
		{
			get
			{
				HierarchyTree<T> root = this;
				while (root._parent != null)
					root = root._parent;
				return root;
			}
		}
		/// <summary>
		/// 현재 노드의 부모 노드
		/// </summary>
		public HierarchyTree<T> Parent => _parent;
		/// <summary>
		/// 자식 노드가 있으면 true
		/// </summary>
		public bool HasChildren => _children?.Count > 0;
		/// <summary>
		/// 첫번째 자식 노드 반환 (없으면 null)
		/// </summary>
		public HierarchyTree<T> FirstChildren => _children?[0];
		/// <summary>
		/// 마지막 자식 노드 반환 (없으면 null)
		/// </summary>
		public HierarchyTree<T> LastChildren => _children?[^1];
		/// <summary>
		/// 현재 노드의 값 반환
		/// </summary>
		public T Value
		{
			get => _value;
			set => _value = value;
		}
		/// <summary>
		/// 현재 노드가 접혀있는지 여부
		/// </summary>
		public bool IsFold { get; private set; } = true;
		/// <summary>
		/// 최상위 노드로부터의 인덱스 (시작: 0)
		/// </summary>
		public int Index => _index;
		/// <summary>
		/// 트리 내 현재 노드의 깊이
		/// </summary>
		public int Depth { get; private set; }
		/// <summary>
		/// 자신을 포함한 현재 노드의 모든 Child 수
		/// </summary>
		public int Length { get; private set; } = 1;
		/// <summary>
		/// 현재 노드가 보이는지 여부
		/// </summary>
		public bool Visible { get; set; }
#endregion // Properties

#region Indexer
		public HierarchyTree<T> this[int idx]
		{
			get
			{
				if (_children == null)
					return null;

				if (idx < 0)
					idx += _children.Count;

				if (idx < _children.Count)
					return _children[idx];

				return null;
			}
		}
#endregion // Indexer

#region Initialize & Instantiate
		public static HierarchyTree<T> CreateNew(T item, HierarchyTree<T> parent = null) => new()
		{
			_value = item,
			_parent = parent,
			IsFold = true,
		};
		private HierarchyTree() { }
#endregion // Initialize & Instantiate

#region Function
		public HierarchyTree<T> AddChildren(params T[] items)
		{
			if (items == null) return this;

			// Add item(s)
			_children ??= new List<HierarchyTree<T>>();
			foreach (var item in items)
			{
				var treeItem = CreateNew(item, this);
				treeItem.Depth = Depth + 1;
				_children.Add(treeItem);
			}

			// Calculate node length
			Length += items.Length;
			var t = this.Parent;
			while (t != null)
			{
				t.Length += items.Length;
				t = t.Parent;
			}

			return this;
		}

		public void SetItemIndex()
		{
			// Set index
			int idx = 0;
			foreach (var item in Root.GetForwardEnumerator())
				item._index = idx++;
		}
		public void Toggle()
		{
			IsFold = !IsFold;
		}

		public void ResetFoldState() => IsFold = true;

		public void ResetFoldStateInChildren()
		{
			var nodes = GetForwardEnumerator();
			foreach (var node in nodes)
				node.ResetFoldState();
		}
		public static HierarchyTree<T> FindByIndex(HierarchyTree<T> root, int idx) => FindByIndexInternal(root, ref idx);

		private static HierarchyTree<T> FindByIndexInternal(HierarchyTree<T> hierarchyTree, ref int idx)
		{
			if (idx == 0)
				return hierarchyTree;
			if (hierarchyTree._children == null || idx < 0)
				return null;
			for (int i = 0; i < hierarchyTree._children.Count; ++i)
			{
				idx--;
				var result = FindByIndexInternal(hierarchyTree._children[i], ref idx);
				if (idx == 0)
					return result;
			}
			return null;
		}

		public static void FoldAll(HierarchyTree<T> root)
		{
			foreach (var tree in root.GetForwardEnumerator())
			{
				tree.IsFold = true;
				tree.Visible = tree == root;
			}
		}
		public static HierarchyTree<T> Pick(HierarchyTree<T> root, int idx)
		{
			var target = FindByIndex(root, idx);
			if (target != null)
			{
				SetVisibleChildren(target, true);

				foreach (var tree in target.GetAscentEnumerator())
				{
					tree.IsFold = false;
					tree.Visible = true;

					SetVisibleChildren(tree, true);
				}
			}
			return target;
		}

		private static void SetVisibleChildren(HierarchyTree<T> parent, bool visible)
		{
			if (parent.HasChildren)
				foreach (var child in parent._children)
					child.Visible = visible;
		}

		public void SetVisibleChildren(bool visible) => SetVisibleChildren(this, visible);
#endregion // Function

#region Navigate
		private HierarchyTree<T> Prev()
		{
			if (_parent == null) return null;

			var prev = PrevSibling();
			if (prev == null) return _parent;

			return FindPrev(prev);

			HierarchyTree<T> FindPrev(HierarchyTree<T> prevSibling) => !prevSibling.HasChildren ? prevSibling : FindPrev(prevSibling.LastChildren);
		}

		private HierarchyTree<T> Next()
		{
			if (HasChildren) return _children[0];
			if (_parent == null) return null;

			var next = NextSibling();
			if (next != null) return next;

			return FindNext();

			HierarchyTree<T> FindNext()
			{
				var parent = _parent;
				var next = parent.NextSibling();
				while (next == null)
				{
					parent = parent._parent;
					if (parent == null)
						return null;
					next = parent.NextSibling();
				}
				return next;
			}
		}

		private HierarchyTree<T> PrevSibling()
		{
			if (_parent == null) return null;

			var findMe = false;
			for (int i = _parent._children.Count - 1; i >= 0; --i)
			{
				var sibling = _parent._children[i];
				if (sibling.Equals(this))
				{
					findMe = true;
					continue;
				}

				if (findMe)
					return sibling;
			}

			return null;
		}

		private HierarchyTree<T> NextSibling()
		{
			if (_parent == null) return null;

			var findMe = false;
			foreach (var sibling in _parent._children)
			{
				if (sibling.Equals(this))
				{
					findMe = true;
					continue;
				}

				if (findMe)
					return sibling;
			}

			return null;
		}
#endregion // Navigate

#region Enumerator
		public ForwardEnumerator<HierarchyTree<T>> GetForwardEnumerator() => ForwardEnumerator<HierarchyTree<T>>.CreateNew(this);
		public BackwardEnumerator<HierarchyTree<T>> GetBackwardEnumerator() => BackwardEnumerator<HierarchyTree<T>>.CreateNew(this);
		public AscentEnumerator<HierarchyTree<T>> GetAscentEnumerator() => AscentEnumerator<HierarchyTree<T>>.CreateNew(this);
		public DescentEnumerator<HierarchyTree<T>> GetDescentEnumerator() => DescentEnumerator<HierarchyTree<T>>.CreateNew(this);
		public abstract class HierarchyEnumerator<HT> : IEnumerator<HT>, IEnumerable<HT> where HT : HierarchyTree<T>
		{
			private HT _root;
			protected HT _current;
			protected HierarchyEnumerator(HT root) => _root = _current = root;
			public virtual bool MoveNext() => false;
			public virtual void Reset() => _current = _root;
			object IEnumerator.Current => Current as object;
			public HT Current => _current;

			public IEnumerator<HT> GetEnumerator()
			{
				do
				{
					yield return _current;
				}
				while (MoveNext());
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public void Dispose()
			{
				_root = null;
				_current = null;
			}
		}

		public class BackwardEnumerator<HT> : HierarchyEnumerator<HT> where HT : HierarchyTree<T>
		{
			public static BackwardEnumerator<HT> CreateNew(HT root) => new(root);
			private BackwardEnumerator(HT root) : base(root) { }
			public override bool MoveNext()
			{
				var prev = _current.Prev() as HT;
				if (prev == null)
					return false;

				_current = prev;
				return true;
			}
		}

		public class ForwardEnumerator<HT> : HierarchyEnumerator<HT> where HT : HierarchyTree<T>
		{
			public static ForwardEnumerator<HT> CreateNew(HT root) => new(root);
			private ForwardEnumerator(HT root) : base(root) { }
			public override bool MoveNext()
			{
				var next = _current.Next() as HT;
				if (next == null)
					return false;

				_current = next;
				return true;
			}
		}

		public class AscentEnumerator<HT> : HierarchyEnumerator<HT> where HT : HierarchyTree<T>
		{
			public static AscentEnumerator<HT> CreateNew(HT root) => new(root);

			private AscentEnumerator(HT root) : base(root) { }
			public override bool MoveNext()
			{
				var next = _current.Parent as HT;
				if (next == null)
					return false;

				_current = next;
				return true;
			}
		}

		public class DescentEnumerator<HT> : HierarchyEnumerator<HT> where HT : HierarchyTree<T>
		{
			private int _lastSiblingIdx;
			public static DescentEnumerator<HT> CreateNew(HT root) => new(root);

			private DescentEnumerator(HT root) : base(root)
			{
				var item = root;
				while (item.HasChildren)
					item = item.LastChildren as HT;
				_lastSiblingIdx = item.Index;
			}

			public override bool MoveNext()
			{
				var next = _current.Next() as HT;
				if (next == null || next.Index > _lastSiblingIdx)
					return false;

				_current = next;
				return true;
			}
		}
#endregion // Enumerator

#region Debug
		public void PrintTree()
		{
			StringBuilder sb = new StringBuilder();
			WriteInfo(sb, this);
			C2VDebug.Log(sb.ToString());
		}

		private void WriteInfo(StringBuilder sb, HierarchyTree<T> item)
		{
			for (int i = 0; i < item.Depth; ++i)
				sb.Append('\t');
			sb.AppendLine($"{item._value.ToString()} ({item.Length}), IDX = {item.Index}");

			if (item._children == null)
				return;

			foreach (var child in item._children)
				WriteInfo(sb, child);
		}
#endregion // Debug
		public void Dispose()
		{
			var iter = GetDescentEnumerator();
			foreach (var child in iter)
				child?._children?.Clear();
			_children?.Clear();
		}
	}

	/// <summary>
	/// 계층 트리 확장
	/// </summary>
	public static class HierarchyTreeExtension
	{
		public static HierarchyTree<T> GetChild<T>(this HierarchyTree<T>[] tree, int groupIdx, int index)
		{
			if (tree == null) return null;

			if (index == 0)
				return tree[groupIdx];
			return tree[groupIdx].GetForwardEnumerator().Skip(index).First();
		}
	}
}
