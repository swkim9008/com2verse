/*===============================================================
* Product:		Com2Verse
* File Name:	BadgeHierarchyTable.cs
* Developer:	NGSG
* Date:			2023-04-24 11:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Logger.UberDebug;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.UI
{
	// public enum eBadgeType
	// {
	// 	ROOT = 0,
	// 	
	// 	MAIN_MENU,
	// 	MAIN_MENU_SUB1,
	// 	MAIN_MENU_SUB1_TAB1,
	// 	MAIN_MENU_SUB1_TAB2,
	// 	MAIN_MENU_SUB1_TAB1_ITEM,
	// 	MAIN_MENU_SUB1_TAB2_ITEM,
	// 	
	// 	END,
	// }

	[System.Serializable]
	public class BadgeHierarchyTable : Singleton<BadgeHierarchyTable>
	{
		public static string ROOT = "ROOt";
		private static BadgeTreeNode<string> _root = null;
		
		private BadgeHierarchyTable()
		{
			// 배찌 하이라키 테이블을 만든다 
			// _root = new BadgeTreeNode<eBadgeType>(eBadgeType.ROOT);
			// {
			// 	BadgeTreeNode<eBadgeType> mainMenu = _root.AddChild(eBadgeType.MAIN_MENU);
			// 	{
			// 		BadgeTreeNode<eBadgeType> mainMenuSub  = mainMenu.AddChild(eBadgeType.MAIN_MENU_SUB1);
			// 		{
			// 			BadgeTreeNode<eBadgeType> mainMenutab1 = mainMenuSub.AddChild(eBadgeType.MAIN_MENU_SUB1_TAB1);
			// 			{
			// 				BadgeTreeNode<eBadgeType> mainMenuitem = mainMenutab1.AddChild(eBadgeType.MAIN_MENU_SUB1_TAB1_ITEM);	
			// 			}
			// 			BadgeTreeNode<eBadgeType> mainMenutab2 = mainMenuSub.AddChild(eBadgeType.MAIN_MENU_SUB1_TAB2);
			// 			{
			// 				BadgeTreeNode<eBadgeType> mainMenuitem = mainMenutab2.AddChild(eBadgeType.MAIN_MENU_SUB1_TAB2_ITEM);
			// 			}
			// 		}
			// 	}
			// }

			//LoadTable();
		}
		
		public static void LoadTable()
		{
			BadgeHierarchyTableMaker.Load(OnComplete);
		}

		public static async UniTask LoadTableAsync()
		{
			var loadedAsset = await BadgeHierarchyTableMaker.LoadAsync();
			if (loadedAsset != null)
				OnComplete(loadedAsset._root);
		}

		private static void OnComplete(BadgeTreeNode<string> loadRoot)
		{
			if (_root == null)
				_root = new BadgeTreeNode<string>(BadgeHierarchyTable.ROOT);

			CreateTree(_root, loadRoot);
			//C2VDebug.LogCategory("badge", "Loading OnComplete BadgeTable");
		}

		private static void CreateTree(BadgeTreeNode<string> destNode, BadgeTreeNode<string> sorcNode)
		{
			if (sorcNode.Children == null || sorcNode.Children.Count == 0)
				return;
				
			foreach (var ch in sorcNode.Children)
			{
				if(ch.Data == BadgeHierarchyTable.ROOT)
					continue;
				
				BadgeTreeNode <string> newNode = destNode.AddChild(ch.Data);
				CreateTree(newNode, ch);
			}
		}

		public BadgeTreeNode<string> GetNode(string ty)
		{
			return _root.FindTreeNode(node => node.Data == ty);
		}

		public void FindTableChildList(string bt, ref List<string> list)
		{
			BadgeTreeNode<string> node = GetNode(bt);
			FindTableChildList(node, ref list);
		}
		private void FindTableChildList(BadgeTreeNode<string> node, ref List<string> list)
		{
			if (node.Children == null || node.Children.Count == 0)
				return;
			
			// 부모순에서 차일드에게 전달해야 한다
			foreach (var child in node.Children)
			{
				list.Add(child.Data);
				FindTableChildList(child, ref list);
			}
		}

		public void FindTableParentList(string ty, ref List<string> list)
		{
			if (ty == BadgeHierarchyTable.ROOT)
				return;

			// 차일드에서 부모순으로 전달해야 한다
			BadgeTreeNode<string> node = GetNode(ty);
			string parentBadgeType = node.Parent.Data;
			list.Add(parentBadgeType);

			FindTableParentList(parentBadgeType, ref list);
		}


		// public string GetParentType(string ty)
		// {
		// 	BadgeTreeNode<string> node = GetNode(ty);
		// 	return node.Parent == null ? null : node.Parent.Data;
		// }
	}
}
