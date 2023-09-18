/*===============================================================
* Product:		Com2Verse
* File Name:	StringListSearchProvider.cs
* Developer:	tlghks1009
* Date:			2022-08-17 14:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	public sealed class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		private string[] _listItems;
		private string _selected = "";
		private Action<string> _onSetIndexCallback;


		public void Set(string[] items, string selected, Action<string> callback)
		{
			_listItems = items;
			_selected = selected;
			_onSetIndexCallback = callback;
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			var searchList = new List<SearchTreeEntry>();
			searchList.Add(new SearchTreeGroupEntry(new GUIContent("Root"), 0));

			List<string> sortedListItems = _listItems?.ToList();
			sortedListItems.Sort((a, b) =>
			{
				string[] splits1 = a.Split('/');
				string[] splits2 = b.Split('/');

				for (int i = 0; i < splits1.Length; i++)
				{
					if (1 >= splits2.Length)
						return 1;
					int value = String.Compare(splits1[i], splits2[i], StringComparison.Ordinal);
					if (value != 0)
					{
						if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
							return splits1.Length < splits2.Length ? 1 : -1;
						return value;
					}
				}

				return 0;
			});


			var keyValueOfSelected = _selected.Split('/');
			string key = "";
			if (keyValueOfSelected.Length > 0)
				key = keyValueOfSelected[0];

			if (string.IsNullOrEmpty(key))
				key = "-";

			var normalIcon = EditorGUIUtility.IconContent("transparent").image;
			var checkIcon = EditorGUIUtility.IconContent("P4_CheckOutLocal").image;

			List<string> groups = new List<string>();
			foreach (string item in sortedListItems)
			{
				string[] entryTitle = item.Split('/');
				string groupName = "";
				for (int i = 0; i < entryTitle.Length - 1; i++)
				{
					groupName += entryTitle[i];
					if (!groups.Contains(groupName))
					{
						var entryTitleContent = groupName.Contains(key) ? new GUIContent(entryTitle[i], checkIcon) : new GUIContent(entryTitle[i], normalIcon);
						searchList.Add(new SearchTreeGroupEntry(entryTitleContent, i + 1));
						groups.Add(groupName);
					}

					groupName += "/";
				}

				if (string.IsNullOrEmpty(_selected))
					_selected = "-";

				var entryContent = item.Contains(_selected) ? new GUIContent(entryTitle.Last(), checkIcon) : new GUIContent(entryTitle.Last(), normalIcon);
				var entry = new SearchTreeEntry(entryContent)
				{
					level = entryTitle.Length,
					userData = GetEntryTitle(entryTitle)
				};
				searchList.Add(entry);
			}

			return searchList;
		}

		public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			_onSetIndexCallback?.Invoke((string) searchTreeEntry.userData);
			return true;
		}


		private string GetEntryTitle(string[] entryTitle)
		{
			if (entryTitle.Length == 3)
				return $"{entryTitle.First()}/{entryTitle[1]}/{entryTitle.Last()}";

			return entryTitle.First() == entryTitle.Last() ? entryTitle.First() : $"{entryTitle.First()}/{entryTitle.Last()}";
		}
	}
}
