/*===============================================================
* Product:		Com2Verse
* File Name:	HierarchyIconController.cs
* Developer:	tlghks1009
* Date:			2023-05-23 18:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Com2verseEditor.UI
{
	public sealed class HierarchyIconController
	{
		private static List<HierarchyIconDrawer> _hierarchyIconDrawers;

		private static int _iconSpacing   = 16;
		private static int _iconStartXPos = 32;
		private static int _iconWidth     = 16;


		[UnityEditor.InitializeOnLoadMethod]
		private static void ApplyHierarchyIcon()
		{
			if (_hierarchyIconDrawers == null)
			{
				_hierarchyIconDrawers = new List<HierarchyIconDrawer>();

				AddDrawer(new SpriteDrawer(EditorGUIUtility.IconContent("SpriteAtlas Icon").image));

				AddDrawer(new BinderDrawer(EditorGUIUtility.IconContent("editicon.sml").image,
				                           EditorGUIUtility.IconContent("d_console.erroricon").image));
			}

			EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierarchyIcon;
		}


		private static void AddDrawer(HierarchyIconDrawer hierarchyIconDrawer)
		{
			_hierarchyIconDrawers.Add(hierarchyIconDrawer);
		}


		private static void OnDrawHierarchyIcon(int instanceID, Rect selectionRect)
		{
			var iconRect = new Rect(selectionRect)
			{
				x     = _iconStartXPos,
				width = _iconWidth,
			};

			foreach (var iconDrawer in _hierarchyIconDrawers)
			{
				if (!iconDrawer.TryInitialize(instanceID))
				{
					continue;
				}

				if (!iconDrawer.TryDrawHierarchyIcon(iconRect))
				{
					continue;
				}

				iconRect.x += _iconSpacing;
			}
		}


		public abstract class HierarchyIconDrawer
		{
			protected Texture DefaultTexture { get; }

			protected int InstanceId { get; set; }

			protected HierarchyIconDrawer(Texture defaultTexture) => DefaultTexture = defaultTexture;

			public abstract bool TryInitialize(int instanceId);

			public abstract bool TryDrawHierarchyIcon(Rect selectedRect);
		}
	}
}
