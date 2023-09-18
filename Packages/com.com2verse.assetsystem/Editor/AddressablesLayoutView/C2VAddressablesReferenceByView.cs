/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesReferenceByView.cs
* Developer:	tlghks1009
* Date:			2023-03-23 10:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2VerseEditor.AssetSystem;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesReferenceByView : C2VAddressablesBaseLayoutView
	{
		public override void Show()
		{
			TreeViewState = new TreeViewState();
			ReferenceByViewTree = new C2VAddressablesReferenceByTreeView(TreeViewState);
		}

		public override void Hide() { }


		protected override void FindReferenceByGroup(C2VAddressableAssetLayoutTreeItem layoutTreeItem)
		{
			if (C2VAddressablesAssetSource.TryGetAddressableAssetInfos(layoutTreeItem.displayName, out var infos))
			{
				var duplicateCheck = new List<string>();

				foreach (var info in infos)
				{
					foreach (var bundleGroup in info.ReferenceBy)
					{
						if (!duplicateCheck.Contains(bundleGroup))
						{
							AddItem(bundleGroup);

							duplicateCheck.Add(bundleGroup);
						}
					}
				}
			}
		}


		protected override void FindReference(C2VAddressableAssetLayoutTreeItem layoutTreeItem)
		{
			if (C2VAddressablesAssetSource.TryGetAddressableAssetInfo(layoutTreeItem.Guid, out var info))
			{
				foreach (var bundleGroup in info.ReferenceBy)
				{
					AddItem(bundleGroup);
				}
			}
		}


		public override void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope())
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField(new GUIContent(" Reference By"), EditorStyles.boldLabel);

					EditorGUILayout.Space(10);

					ReferenceByViewTree.DrawSearchField();
				}
			}

			var rect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			ReferenceByViewTree.OnGUI(rect);
		}
	}
}
