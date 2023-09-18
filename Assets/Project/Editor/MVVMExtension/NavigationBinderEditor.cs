/*===============================================================
* Product:    Com2Verse
* File Name:  NavigationBinderEditor.cs
* Developer:  tlghks1009
* Date:       2022-04-05 12:20
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/


#if UNITY_EDITOR

using Com2Verse.UI;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(NavigationBinder))]
	public sealed class NavigationBinderEditor : BinderEditor
	{
		private NavigationBinder _navigationBinder;

		protected override void OnEnable()
		{
			_navigationBinder = target as NavigationBinder;

			serializedObject.Update();
			serializedObject.FindProperty("_metaverseButton").objectReferenceValue = _navigationBinder.GetComponent<MetaverseButton>();
			serializedObject.ApplyModifiedProperties();
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspectorWithoutScriptField();

			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_comment"));

			Refresh();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				if (ButtonHelper.Button("Add Sequence", Color.blue, 200))
				{
					var sequenceList = serializedObject.FindProperty("_sequenceList");
					sequenceList.InsertArrayElementAtIndex(sequenceList.arraySize);
					sequenceList.GetArrayElementAtIndex(sequenceList.arraySize - 1).FindPropertyRelative("priority").intValue = sequenceList.arraySize - 1;
				}
			}
			EditorGUILayout.EndHorizontal();
			serializedObject.ApplyModifiedProperties();
		}


		private void Refresh()
		{
			for (int index = 0; index < serializedObject.FindProperty("_sequenceList").arraySize; ++index)
			{
				LineHelper.Draw(Color.gray);

				DrawSequence(index);
			}
		}


		private void DrawSequence(int index)
		{
			serializedObject.Update();

			var serializedObjectOfSequence = serializedObject.FindProperty("_sequenceList").GetArrayElementAtIndex(index);
			var serializedPropertyOfCommandType = serializedObjectOfSequence.FindPropertyRelative("commandType");
			var commandType = (NavigationBinder.eCommandType) serializedPropertyOfCommandType.intValue;

			EditorGUILayout.PropertyField(serializedPropertyOfCommandType);

			switch (commandType)
			{
				case NavigationBinder.eCommandType.VIEW_HIDE:
				case NavigationBinder.eCommandType.VIEW_SHOW:
				case NavigationBinder.eCommandType.VIEW_FIXED:
				case NavigationBinder.eCommandType.VIEW_DESTROY:
				{
					var serializedPropertyOfTargetType = serializedObjectOfSequence.FindPropertyRelative("targetType");
					var targetType = (NavigationBinder.eTargetType) serializedPropertyOfTargetType.intValue;
					EditorGUILayout.PropertyField(serializedPropertyOfTargetType);

					if (targetType == NavigationBinder.eTargetType.OTHER)
					{
						var assetReferenceProperty = serializedObjectOfSequence.FindPropertyRelative("assetReference");
						EditorGUILayout.PropertyField(assetReferenceProperty);
					}
				}
					break;
			}

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			{
				if (ButtonHelper.Button("up"))
				{
					var priority = serializedObjectOfSequence.FindPropertyRelative("priority").intValue;
					Swap(priority, priority - 1);
				}

				if (ButtonHelper.Button("down"))
				{
					var priority = serializedObjectOfSequence.FindPropertyRelative("priority").intValue;
					Swap(priority, priority + 1);
				}


				if (ButtonHelper.Button("X"))
					RemoveSequence(index);
			}
			EditorGUILayout.EndHorizontal();
			serializedObject.ApplyModifiedProperties();
		}


		private void Swap(int from, int to)
		{
			var serializedObjectOfSequence = serializedObject.FindProperty("_sequenceList");
			if (to >= serializedObjectOfSequence.arraySize || to < 0)
				return;

			serializedObjectOfSequence.MoveArrayElement(from, to);

			for (int index = 0; index < serializedObjectOfSequence.arraySize; index++)
				serializedObjectOfSequence.GetArrayElementAtIndex(index).FindPropertyRelative("priority").intValue = index;
		}


		private void RemoveSequence(int sequenceIndex)
		{
			var serializedObjectOfSequence = serializedObject.FindProperty("_sequenceList");
			var priority = serializedObjectOfSequence.GetArrayElementAtIndex(sequenceIndex).FindPropertyRelative("priority").intValue;

			for (int index = priority; index < serializedObjectOfSequence.arraySize; ++index)
				serializedObjectOfSequence.GetArrayElementAtIndex(index).FindPropertyRelative("priority").intValue -= 1;

			serializedObjectOfSequence.DeleteArrayElementAtIndex(sequenceIndex);
		}
	}
}
#endif
