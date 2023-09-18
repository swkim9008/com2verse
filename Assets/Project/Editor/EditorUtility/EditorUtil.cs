// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ReadOnlyPageArray.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-11 오전 11:11
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor
{
	public static class EditorUtil
	{
		public class PagedArrayContext
		{
			public int CurrentPageNumber = 1;
			public int CachedPageNumber;
			public SerializedProperty[] PropertiesInCurrentPage;
		}
		
		private static GUIStyle _foldoutStyle = new GUIStyle(EditorStyles.foldout)
		{
			fontStyle = FontStyle.Bold,
			margin = new RectOffset(0, 0, 3, 3)
		};

		private static GUIStyle _listBoxStyle = new GUIStyle(EditorStyles.helpBox)
		{
			margin = new RectOffset(24, 0, 0, 0)
		};

		private static GUIStyle _pageTextStyle = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter
		};
		
		public static PagedArrayContext PagedArray(SerializedProperty property, PagedArrayContext context, int pageSize = 10)
		{
			if (context == null) context = new();
			return PagedArrayCommon(property, context, pageSize, false);
		}
		
		public static PagedArrayContext ReadOnlyPagedArray(SerializedProperty property, PagedArrayContext context, int pageSize = 10)
		{
			if (context == null) context = new();
			return PagedArrayCommon(property, context, pageSize, true);
		}

		private static PagedArrayContext PagedArrayCommon(SerializedProperty property, PagedArrayContext context, int pageSize, bool readOnly)
		{
			if (!property.isArray) EditorGUILayout.HelpBox("ReadOnlyPagedArray can only be used with array type property", MessageType.Error);

			int pageCount = (property.arraySize - 1) / pageSize + 1;
			context.CurrentPageNumber = Math.Min(pageCount, context.CurrentPageNumber);

			context = ShowPageContent(property, context, pageSize, !readOnly);
			if (property.isExpanded)
			{
				if (!readOnly)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), EditorStyles.miniButtonRight))
					{
						property.InsertArrayElementAtIndex(property.arraySize);
						context.CachedPageNumber = -1;
					}
					EditorGUILayout.EndHorizontal();
				}

				context.CurrentPageNumber = ShowPageList(context.CurrentPageNumber, pageCount);
			}

			return context;
		}

		private static PagedArrayContext ShowPageContent(SerializedProperty property, PagedArrayContext context, int pageSize, bool showRemove)
		{
			int size = property.arraySize;
			property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, $"{property.displayName} (Count : {size})", _foldoutStyle);
			if (!property.isExpanded) return context;

			if (size == 0)
			{
				EditorGUILayout.HelpBox("No Elements", MessageType.Info);
			}
			else
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginVertical(_listBoxStyle);

				if (context.PropertiesInCurrentPage == null || context.CurrentPageNumber != context.CachedPageNumber || (context.PropertiesInCurrentPage.Length > 0 && context.PropertiesInCurrentPage[0].IsUnityNull()))
				{
					int initialIndex = Math.Max(0, (context.CurrentPageNumber - 1) * pageSize);
					int maxIndex = Math.Min(size, context.CurrentPageNumber * pageSize);
					context.PropertiesInCurrentPage = new SerializedProperty[maxIndex - initialIndex];

					for (int i = initialIndex; i < Math.Min(size, context.CurrentPageNumber * pageSize); i++)
					{
						context.PropertiesInCurrentPage[i - initialIndex] = property.GetArrayElementAtIndex(i);
					}

					context.CachedPageNumber = context.CurrentPageNumber;
				}
				
				for (int i = 0; i < context.PropertiesInCurrentPage.Length; i++)
				{
					var targetProperty = context.PropertiesInCurrentPage[i];
					EditorGUILayout.PropertyField(targetProperty);

					if (showRemove && targetProperty.isExpanded)
					{
						if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash")))
						{
							property.DeleteArrayElementAtIndex(i);
							context.CachedPageNumber = -1;
							break;
						}
					}
				}

				EditorGUILayout.EndVertical();
				EditorGUI.indentLevel--;
			}

			return context;
		}

		private static int ShowPageList(int currentPage, int pageCount)
		{
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("First"))
			{
				return 1;
			}

			if (GUILayout.Button(EditorGUIUtility.IconContent("tab_prev")))
			{
				return Math.Max(1, currentPage - 1);
			}

			EditorGUILayout.LabelField($"Page {currentPage} / {pageCount}", _pageTextStyle, GUILayout.MinWidth(0));
			
			if (GUILayout.Button(EditorGUIUtility.IconContent("tab_next")))
			{
				return Math.Min(pageCount, currentPage + 1);
			}

			if (GUILayout.Button("Last"))
			{
				return pageCount;
			}

			EditorGUILayout.EndHorizontal();

			return currentPage;
		}
	}
}
