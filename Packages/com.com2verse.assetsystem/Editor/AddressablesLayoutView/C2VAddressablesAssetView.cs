/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesAssetsView.cs
* Developer:	tlghks1009
* Date:			2023-03-23 10:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesAssetView : C2VAddressablesBaseLayoutView
	{
		private C2VAddressablesReferenceByView _referenceByView;
		private C2VAddressablesReferenceToView _referenceToView;

		private float _splitterTree = 0.333f;
		private float _splitterReferences = 0.5f;

		private string _searchString;

		public override void Show()
		{
			Window.OnGUIHandler += OnGUI;

			if (!C2VAddressablesAssetSource.TryLoad())
			{
				return;
			}

			_referenceByView = Window.FindView<C2VAddressablesReferenceByView>();
			_referenceToView = Window.FindView<C2VAddressablesReferenceToView>();

			Window.ShowView(_referenceByView);
			Window.ShowView(_referenceToView);

			TreeViewState = new TreeViewState();
			ReferenceByViewTree = new C2VAddressablesAssetTreeView(TreeViewState);

			ReferenceByViewTree.OnSelected += item =>
			{
				_referenceByView.SetSelectedItem(item);
				_referenceToView.SetSelectedItem(item);
			};
		}


		public override void Hide()
		{
			Window.OnGUIHandler -= OnGUI;
		}


		public override void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope())
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("메뉴", EditorStyles.toolbarDropDown, GUILayout.Width(80)))
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("새로고침"), false, () =>
						{
							C2VAddressablesAssetSource.Refresh();
							this.Show();
						});
						menu.ShowAsContext();
					}

					GUILayout.FlexibleSpace();

					ReferenceByViewTree?.DrawSearchField();
				}

				var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

				ReferenceByViewTree?.OnGUI(rect);
			}

			_splitterTree = C2VSplitterGUI.VerticalSplitter(nameof(_splitterTree).GetHashCode(), _splitterTree, 0.2f, 0.8f, base.Window);

			using (new EditorGUILayout.HorizontalScope(GUILayout.Height(base.Window.position.height * _splitterTree)))
			{
				using (new EditorGUILayout.VerticalScope(GUILayout.Width(base.Window.position.width * (1 - _splitterReferences))))
				{
					_referenceToView?.OnGUI();
				}

				_splitterReferences = C2VSplitterGUI.HorizontalSplitter(nameof(_splitterReferences).GetHashCode(), _splitterReferences, 0.3f, 0.7f, base.Window);

				using (new EditorGUILayout.VerticalScope(GUILayout.Width(base.Window.position.width * _splitterReferences)))
				{
					_referenceByView?.OnGUI();
				}
			}
		}
	}
}
