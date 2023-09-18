/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseToggleEditor.cs
* Developer:	tlghks1009
* Date:			2022-07-14 11:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(MetaverseToggle))]
	public sealed class MetaverseToggleEditor : ToggleEditor
	{
		private MetaverseToggle _toggle;
	
		protected override void OnEnable()
		{
			base.OnEnable();
			
			_toggle = target as MetaverseToggle;
		}


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();


			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			if (ButtonHelper.Button("Add ToggleGetter", Color.gray, 300))
			{
				_toggle.gameObject.AddComponent<ToggleGetter>();
			}

			if (ButtonHelper.Button("Add DataBinder", Color.gray, 300))
			{
				_toggle.gameObject.AddComponent<DataBinder>();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			
			if (ButtonHelper.Button("Add ComparisonOperatorExtension", Color.gray, 300))
			{
				_toggle.gameObject.AddComponent<ComparisonOperatorExtensions>();
			}

			if (ButtonHelper.Button("Add GameObjectPropertyExtension", Color.gray, 300))
			{
				_toggle.gameObject.AddComponent<GameObjectPropertyExtensions>();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
	}
}
