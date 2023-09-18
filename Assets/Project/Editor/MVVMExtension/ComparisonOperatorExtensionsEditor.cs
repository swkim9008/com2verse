/*===============================================================
* Product:		Com2Verse
* File Name:	ComparisonOperatorExtensionsEditor.cs
* Developer:	tlghks1009
* Date:			2022-07-14 12:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(ComparisonOperatorExtensions))]
	public sealed class ComparisonOperatorExtensionsEditor : BinderEditor
	{
		private ComparisonOperatorExtensions _comparisonOperator;

		protected override void OnEnable()
		{
			base.OnEnable();

			_comparisonOperator = target as ComparisonOperatorExtensions;
		}

		protected override void OnDisable()
		{
			_comparisonOperator = null;
		}

		public override void OnInspectorGUI()
		{
			base.DrawDefaultInspectorWithoutScriptField();

			DrawType();
			DrawTarget();
		}

		private void DrawType()
		{
			serializedObject.Update();
			
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_operationType"));	
			
			var type = (ComparisonOperatorExtensions.eType) EditorGUILayout.EnumPopup((ComparisonOperatorExtensions.eType) serializedObject.FindProperty("_type").intValue, GUILayout.Width(100) );
			serializedObject.FindProperty("_type").enumValueIndex = (int) type;

			
			switch ( type )
			{
				case ComparisonOperatorExtensions.eType.INT:
					serializedObject.FindProperty("_int").intValue = EditorGUILayout.IntField(serializedObject.FindProperty("_int").intValue, GUILayout.Width(200));
					break;
				case ComparisonOperatorExtensions.eType.BOOL:
					serializedObject.FindProperty("_boolean").boolValue = EditorGUILayout.Toggle(serializedObject.FindProperty("_boolean").boolValue, GUILayout.Width(200));
					break;
				case ComparisonOperatorExtensions.eType.STRING:
					serializedObject.FindProperty("_string").stringValue = EditorGUILayout.TextField(serializedObject.FindProperty("_string").stringValue, GUILayout.Width(200));
					break;
			}
			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
		}


		private void DrawTarget()
		{
			serializedObject.Update();
			
			var properties = Finder.GetBindableProperties(
				_comparisonOperator.gameObject,
				(property) =>
					property.GetSetMethod(false) != null &&
					property.GetGetMethod(false) != null &&
					property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

			var viewModelList = Finder.GetBindableProperties(
				ViewModelTypes,
				(property) =>
					property.GetSetMethod(false) != null &&
					property.GetGetMethod(false) != null &&
					property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

			properties.AddRange(viewModelList!);

			ShowMemberTypeMenu(_comparisonOperator, "Target", properties, GetBindingFullPath(serializedObject, "_targetPath"),
			                   (updateOwner, updateProperty) =>
			                   {
				                   serializedObject.FindProperty("_targetPath.propertyOwner").stringValue = updateOwner;
				                   serializedObject.FindProperty("_targetPath.property").stringValue = updateProperty;
			                   });

			var propertyOwnerName = serializedObject.FindProperty("_targetPath.propertyOwner").stringValue;
			UpdateTargetComponent(_comparisonOperator, propertyOwnerName, (component) => 
			                      { serializedObject.FindProperty("_targetPath.component").objectReferenceValue = component; });

			serializedObject.ApplyModifiedProperties();
		}
	}
}
