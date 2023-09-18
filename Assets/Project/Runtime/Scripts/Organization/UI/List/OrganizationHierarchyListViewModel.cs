/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationHierarchyListViewModel.cs
* Developer:	jhkim
* Date:			2022-07-19 10:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using JetBrains.Annotations;
using UnityEngine;
using OrganizationTreeModel = Com2Verse.Organization.HierarchyTreeModel<long>; 

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationHierarchyListViewModel : ViewModelBase
	{
#region Variables
		private HierarchyTree<OrganizationTreeModel> _hierarchyTree;

		[NotNull] private string _textName;
		[NotNull] private string _textCount;
		[NotNull] private float _width;
		[NotNull] private Vector3 _rotationArrow;
		[NotNull] private bool _selected;
		[NotNull] private bool _showArrow;

		public int Index
		{
			get => _hierarchyTree.Index;
			set
			{
				InvokePropertyValueChanged(nameof(Index), value);
			}
		}

		public bool IsFold
		{
			get => _hierarchyTree.IsFold;
			set
			{
				InvokePropertyValueChanged(nameof(IsFold), value);
			}
		}
		public CommandHandler SelectItem { get; private set; }
		public int            GroupIndex => _hierarchyTree.Value.GroupIndex;

		private const float TabWidth = 30;
		private static readonly Vector3 RotationUnfold = new(0,0, Constant.HierarchyUnFoldAngle);
		private static readonly Vector3 RotationFold = new(0, 0, Constant.HierarchyFoldAngle);

		public delegate void OnClicked(int groupIndex, int index);
		private OnClicked _onClicked;
#endregion // Variables

#region Properties
		public string Name
		{
			get => _textName;
			set
			{
				_textName = value;
				InvokePropertyValueChanged(nameof(Name), value);
			}
		}

		public string Count
		{
			get => _textCount;
			set
			{
				_textCount = value;
				InvokePropertyValueChanged(nameof(Count), value);
			}
		}

		public float Width
		{
			get => _width;
			set
			{
				_width = value;
				InvokePropertyValueChanged(nameof(Width), value);
			}
		}

		public Vector3 RotationArrow
		{
			get => _rotationArrow;
			set
			{
				_rotationArrow = value;
				InvokePropertyValueChanged(nameof(RotationArrow), value);
			}
		}

		public bool Visible
		{
			get => _hierarchyTree.Visible;
			set
			{
				_hierarchyTree.Visible = value;
				InvokePropertyValueChanged(nameof(Visible), value);
			}
		}

		public bool Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				InvokePropertyValueChanged(nameof(Selected), value);
			}
		}

		public bool ShowArrow
		{
			get => _showArrow;
			set
			{
				_showArrow = value;
				InvokePropertyValueChanged(nameof(ShowArrow), value);
			}
		}

		private int Level
		{
			set
			{
				if (value < 0)
					return;
				Width = value * TabWidth;
			}
		}
#endregion // Properties

#region Initialize
		public OrganizationHierarchyListViewModel() { }

		public OrganizationHierarchyListViewModel(HierarchyTree<OrganizationTreeModel> hierarchyTree, OnClicked onClicked = null)
		{
			SetData(hierarchyTree, onClicked);
		}

		public void SetData(HierarchyTree<OrganizationTreeModel> hierarchyTree, OnClicked onClicked = null)
		{
			_hierarchyTree = hierarchyTree;
			_onClicked = onClicked;

			Level = hierarchyTree.Depth;
			Name = hierarchyTree.Value.Name;
			Visible = hierarchyTree.Visible;
			IsFold = hierarchyTree.IsFold;

			var count = hierarchyTree.Length - 1;
			Count = count > 0 ? Convert.ToString(count) : string.Empty;
			SelectItem = new CommandHandler(OnSelectHierarchyItem);

			RefreshUI();
		}
#endregion // Initialize

		private void OnSelectHierarchyItem()
		{
			_hierarchyTree.Toggle();
			IsFold = _hierarchyTree.IsFold;
			Selected = true;
			RefreshUI();

			_onClicked?.Invoke(_hierarchyTree.Value.GroupIndex, _hierarchyTree.Value.Index);
		}

		public void SetSelected(bool selected)
		{
			if (Selected != selected)
				Selected = selected;
			RefreshUI();
		}

		// public void SetVisible(bool visible)
		// {
		// 	if (Visible != visible)
		// 		Visible = visible;
		// }
		void RefreshUI()
		{
			RotationArrow = _hierarchyTree.IsFold ? RotationFold : RotationUnfold;
			Visible = _hierarchyTree.Visible;
		}
	}
}
