/*===============================================================
* Product:		Com2Verse
* File Name:	UnityButtonEditor.cs
* Developer:	tlghks1009
* Date:			2022-05-10 12:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace Com2VerseEditor.UI
{
	/// <summary>
	/// "Do not use 'Button' component directly. please use 'MetaverseButton' Instead."
	/// </summary>
	[CustomEditor(typeof(Button))]
	public class UnityButtonEditor : ButtonEditor
	{
		protected override void OnEnable()
		{
			base.OnEnable();

			var unityButtonComponent = target as Button;
			var buttonObject = unityButtonComponent.gameObject;

			var propertyElements = unityButtonComponent.CopySerializedProperties();

			DestroyImmediate(unityButtonComponent);

			var metaverseButton = buttonObject.AddComponent<MetaverseButton>();
			metaverseButton.PasteSerializedProperties(propertyElements);
		}
	}


	[CustomEditor(typeof(MetaverseButton))]
	public class MetaverseButtonEditor : ButtonEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			// EditorGUILayout.PropertyField(serializedObject.FindProperty("_clickIntervalTime"));

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}
