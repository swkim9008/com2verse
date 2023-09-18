/*===============================================================
* Product:		Com2Verse
* File Name:	EnumExtensions.cs
* Developer:	jhkim
* Date:			2022-09-27 22:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif // UNITY_EDITOR

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] EnumExtensions")]
	public sealed class EnumExtensions : MonoBehaviour
	{
		private enum eType
		{
			ACTIVE_IS_EQUAL,
			ACTIVE_NOT_EQUAL,
		}

		[HideInInspector] public string Selected;
		[HideInInspector] public int Index;

#region Properties
		public int ActiveIsEqual
		{
			get => 0;
			set => gameObject.SetActive(Index == value);
		}

		public int ActiveNotEqual
		{
			get => 0;
			set => gameObject.SetActive(Index != value);
		}

		public int TargetIndex
		{
			get => Index;
			set => Index = value;
		}
#endregion // Properties
		public void Save()
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssetIfDirty(this);
#endif // UNITY_EDITOR
		}
	}

#if UNITY_EDITOR
	public sealed class EnumSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		private Action<object> _onSelectedItem;
		private static readonly string EnumPrefix = "e";
		private static Dictionary<string, EnumTypeInfo> _enums = null;

		public void SetOnSelectedItem(Action<object> onSelectedItem) => _onSelectedItem = onSelectedItem;
		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			if (_enums == null)
			{
				_enums = new();
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var asm in assemblies)
				{
					var types = asm.GetTypes();
					foreach (var type in types)
					{
						if (type.Name.StartsWith(EnumPrefix) && type.IsEnum && !type.IsGenericType)
						{
							foreach (var value in Enum.GetValues(type))
								AddItem($"{type.Name}({type.Namespace})", value);
						}
					}
				}
			}

			var result = new List<SearchTreeEntry>();
			result.Add(new SearchTreeGroupEntry(new GUIContent("List")));

			var keys = _enums.Keys.ToList();
			keys.Sort();

			foreach (var key in keys)
			{
				var value = _enums[key];
				result.Add(new SearchTreeGroupEntry(new GUIContent(key), 1));
				foreach (var info in value.Values)
				{
					var newItem = new SearchTreeEntry(new GUIContent($"{info.ValueName} ({key})"));
					newItem.level = 2;
					newItem.userData = info.Value;
					result.Add(newItem);
				}
			}
			return result;

			void AddItem(string typeName, object value)
			{
				if (_enums.ContainsKey(typeName))
				{
					_enums[typeName].Values.Add(new EnumValueInfo
					{
						Value = value,
						ValueName = value.ToString()
					});
				}
				else
				{
					_enums.Add(typeName, new EnumTypeInfo
					{
						Values = new List<EnumValueInfo>
						{
							new EnumValueInfo
							{
								Value = value,
								ValueName = value.ToString(),
							},
						},
					});
				}
			}
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			_onSelectedItem?.Invoke(SearchTreeEntry.userData);
			return true;
		}

		struct EnumTypeInfo
		{
			public List<EnumValueInfo> Values;
		}

		struct EnumValueInfo
		{
			public string ValueName;
			public object Value;
		}
	}
#endif // UNITY_EDITOR

#if UNITY_EDITOR
	[CustomEditor(typeof(EnumExtensions))]
	public sealed class EnumPropertyExtensionsEditor : Editor
	{
		private EnumExtensions _script;
		private Dictionary<string, SerializedProperty> _props;
		private void Awake()
		{
			_script = target as EnumExtensions;
			_props = new();

			AddProperty(nameof(_script.Selected));
			AddProperty(nameof(_script.Index));

			void AddProperty(string propName)
			{
				_props.Add(propName, serializedObject.FindProperty(propName));
			}
		}

		public override void OnInspectorGUI()
		{
			using (var scope = new EditorGUILayout.HorizontalScope())
			{
				if (Event.current.type == EventType.MouseDown && scope.rect.Contains(Event.current.mousePosition))
				{
					var provider = CreateInstance<EnumSearchProvider>();
					provider.SetOnSelectedItem(selected =>
					{
						_script.Selected = $"{selected.GetType().Name}/{selected}";
						_script.Index = (int) selected;
						_script.Save();
					});
					SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
				}

				EditorGUILayout.PropertyField(_props[nameof(_script.Selected)]);
				EditorGUILayout.IntField(_script.Index, GUILayout.MaxWidth(50));
			}
		}
	}
#endif // UNITY_EDITOR
}
