/*===============================================================
* Product:		Com2Verse
* File Name:	TogglePropertyEnumExtensions.cs
* Developer:	jhkim
* Date:			2022-12-02 14:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
#endif // UNITY_EDITOR

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] TogglePropertyEnumExtensions")]
	public sealed class TogglePropertyEnumExtensions : TogglePropertyExtensions
	{
		[HideInInspector] public string Selected;
		[HideInInspector] public int Value;
		private Action<bool> _onValueEnumEqualWithoutNotify;

		public int ValueEnumEqual
		{
			get
			{
				CheckToggle();
				return Value;
			}
			set
			{
				CheckToggle();
				_toggle.isOn = Value == value;
			}
		}

		public int ValueEnumEqualReverse
		{
			get
			{
				CheckToggle();
				return Value;
			}
			set
			{
				CheckToggle();
				_toggle.isOn = Value != value;
			}
		}
		public int ValueEnumEqualWithoutNotify
		{
			get
			{
				CheckToggle();
				return Value;
			}
			set
			{
				CheckToggle();
				_toggle.SetIsOnWithoutNotify(Value == value);
				_onValueEnumEqualWithoutNotify?.Invoke(Value == value);
			}
		}

		public int ValueEnumEqualWithoutNotifyReverse
		{
			get
			{
				CheckToggle();
				return Value;
			}
			set
			{
				CheckToggle();
				_toggle.SetIsOnWithoutNotify(Value != value);
				_onValueEnumEqualWithoutNotify?.Invoke(Value != value);
			}
		}

		public void SetOnValueEnumEqualWithoutNotify(Action<bool> onValueEnumEqualWithoutNotify) => _onValueEnumEqualWithoutNotify = onValueEnumEqualWithoutNotify;
#if UNITY_EDITOR
		public void SetValue(int value) => Value = value;
		public void Save()
		{
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssetIfDirty(this);
		}
#endif // UNITY_EDITOR
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(TogglePropertyEnumExtensions))]
	public sealed class TogglePropertyEnumExtensionsEditor : Editor
	{
		private TogglePropertyEnumExtensions _script;
		private Dictionary<string, SerializedProperty> _props;

		private void Awake()
		{
			_script = target as TogglePropertyEnumExtensions;
			_props = new();

			AddProperty(nameof(_script.Selected));
			AddProperty(nameof(_script.Value));

			void AddProperty(string propName)
			{
				_props.Add(propName, serializedObject.FindProperty(propName));
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			using (var scope = new EditorGUILayout.HorizontalScope())
			{
				if (Event.current.type == EventType.MouseDown && scope.rect.Contains(Event.current.mousePosition))
				{
					var provider = CreateInstance<EnumSearchProvider>();
					provider.SetOnSelectedItem(selected =>
					{
						_script.Selected = $"{selected.GetType().Name}/{selected}";
						_script.SetValue((int) selected);
						_script.Save();
					});
					SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
				}

				EditorGUILayout.PropertyField(_props[nameof(_script.Selected)]);
				EditorGUILayout.IntField(_script.Value, GUILayout.MaxWidth(50));
			}
		}
	}
#endif // UNITY_EDITOR
}
