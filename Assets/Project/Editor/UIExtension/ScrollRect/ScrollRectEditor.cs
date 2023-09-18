/*===============================================================
* Product:		Com2Verse
* File Name:	ScrollRectEditor.cs
* Developer:	tlghks1009
* Date:			2022-12-16 16:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEditor;
using UnityEngine.UI;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(ScrollRect))]
	public sealed class ScrollRectEditor : UnityEditor.UI.ScrollRectEditor
	{
		private SerializedProperty m_ScrollSensitivity;

		protected override void OnEnable()
		{
			base.OnEnable();

			serializedObject.FindProperty("m_ScrollSensitivity").floatValue = 10.0f;

			serializedObject.ApplyModifiedProperties();
		}
	}
}
