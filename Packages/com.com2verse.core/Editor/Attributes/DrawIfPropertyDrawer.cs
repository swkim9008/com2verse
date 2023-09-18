/*===============================================================
 * Product:		Com2Verse
 * File Name:	DrawIfPropertyDrawer.cs
 * Developer:	eugene9721
 * Date:		2022-04-28 14:05
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Logger;
using Com2Verse.Utils;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.Utils
{
	[CustomPropertyDrawer(typeof(DrawIfAttribute))]
	public sealed class DrawIfPropertyDrawer : PropertyDrawer
	{
		// Reference to the attribute on the property.
		private DrawIfAttribute? _drawIf;

		// Field that is being compared.
		private SerializedProperty? _comparedField;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (DrawIfConditionMatch(property))
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
			else if (_drawIf?.DisablingType == DrawIfAttribute.eDisablingType.READ_ONLY)
			{
				GUI.enabled = false;
				EditorGUI.PropertyField(position, property, label, true);
				GUI.enabled = true;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			switch (DrawIfConditionMatch(property), _drawIf?.DisablingType)
			{
				case (true, _):
				case (false, DrawIfAttribute.eDisablingType.READ_ONLY):
					return EditorGUI.GetPropertyHeight(property, label, true);
				default:
					return -EditorGUIUtility.standardVerticalSpacing;
			}
		}

		/// <summary>
		/// Errors default to showing the property.
		/// </summary>
		private bool DrawIfConditionMatch(SerializedProperty property)
		{
			_drawIf = attribute as DrawIfAttribute;
			if (_drawIf == null)
			{
				C2VDebug.LogError("Error: property is null");
				return true;
			}

			_comparedField = FindPropertyInSamePath(property, _drawIf.ComparedPropertyName!);
			if (_comparedField == null)
			{
				C2VDebug.LogError($"Cannot find property name \"{_drawIf.ComparedPropertyName}\" in object {property.serializedObject?.targetObject}");
				return true;
			}

			// Possible extend cases to support your own type
			var result = _comparedField.type switch
			{
				"bool" => _comparedField.boolValue.Equals(_drawIf.ComparedValue),
				"Enum" => _comparedField.enumValueIndex.Equals((int)_drawIf.ComparedValue),
				_      => true,
			};

			return _drawIf.InvertCondition ? !result : result;
		}

		private static SerializedProperty? FindPropertyInSamePath(SerializedProperty property, string name)
		{
			var path   = ReplacePropertyPathName(property, name);
			var result = FindPropertyWithPath(property, path);

			if (result == null)
			{
				path   = ReplacePropertyPathName(property, name.GetBackingFieldName());
				result = FindPropertyWithPath(property, path);

				if (result == null)
				{
					return null;
				}
			}

			return result;
		}

		private static string? ReplacePropertyPathName(SerializedProperty property, string? name)
		{
			var src = property.name;
			if (string.IsNullOrWhiteSpace(src!)) return name;
			return property.propertyPath?.Replace(src!, name!);
		}

		private static SerializedProperty? FindPropertyWithPath(SerializedProperty property, string? path)
		{
			if (string.IsNullOrWhiteSpace(path!)) return null;
			return property.serializedObject?.FindProperty(path!);
		}
	}
}
