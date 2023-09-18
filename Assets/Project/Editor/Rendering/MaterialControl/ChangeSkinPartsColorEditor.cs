/*===============================================================
* Product:		Com2Verse
* File Name:	ChangeSkinPartsColorEditor.cs
* Developer:	ljk
* Date:			2022-08-26 11:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Rendering.MaterialControl;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.Rendering.MaterialControl
{
	[CustomEditor(typeof(ChangeSkinPartsColor))]
	public sealed class ChangeSkinPartsColorEditor : Editor
	{
		private int _id;
		private Color _selected;
		private int _maskPrecision;
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			
			GUILayout.BeginHorizontal();
			_id = EditorGUILayout.IntField(_id,GUILayout.Width(100));
			_selected = EditorGUILayout.ColorField(_selected,GUILayout.Width(100));
			if (Application.isPlaying)
			{
				if (GUILayout.Button("set"))
				{
					(target as ChangeSkinPartsColor).SetColor(_id,_selected);
				}
			}
			else
			{
				GUILayout.Label("런타임에서만 가능 : Material 생성 방지");		
			}
			
			
			GUILayout.EndHorizontal();
		}
	}
}
