/*===============================================================
* Product:		Com2Verse
* File Name:	ToggleGetterEditor.cs
* Developer:	tlghks1009
* Date:			2022-07-13 19:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Reflection;
using Com2Verse.UI;
using UnityEditor;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(ToggleGetter))]
	public sealed class ToggleGetterEditor : BinderEditor
	{
		private ToggleGetter _toggleGetter;
		private MetaverseToggle _metaverseToggle;

		protected override void OnEnable()
		{
			base.OnEnable();

			_toggleGetter = target as ToggleGetter;
			_metaverseToggle = _toggleGetter.gameObject.GetComponent<MetaverseToggle>();
		}

		protected override void OnDisable()
		{
			_toggleGetter = null;
		}


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			serializedObject.FindProperty("_targetPath.propertyOwner").stringValue      = "MetaverseToggle";
			serializedObject.FindProperty("_targetPath.property").stringValue           = "isOn";
			serializedObject.FindProperty("_targetPath.component").objectReferenceValue = _metaverseToggle;


			var properties = Finder.GetBindableProperties(
				_toggleGetter.gameObject,
				(property) =>
					property.GetSetMethod(false) != null &&
					property.GetGetMethod(false) != null &&
					property.GetGetMethod(false).ReturnType == typeof(Boolean) &&
					property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

			var viewModelList = Finder.GetBindableProperties(
				ViewModelTypes,
				(property) =>
					property.GetSetMethod(false) != null &&
					property.GetGetMethod(false) != null &&
					property.GetGetMethod(false).ReturnType == typeof(Boolean) &&
					property.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

			properties.AddRange(viewModelList!);

			ShowMemberTypeMenu(_toggleGetter, "Target", properties, GetBindingFullPath(serializedObject, "_sourcePath"),
			                   (updateOwner, updateProperty) =>
			                   {
				                   serializedObject.FindProperty("_sourcePath.propertyOwner").stringValue = updateOwner;
				                   serializedObject.FindProperty("_sourcePath.property").stringValue = updateProperty; 
			                   });

			serializedObject.ApplyModifiedProperties();
		}
	}
}
