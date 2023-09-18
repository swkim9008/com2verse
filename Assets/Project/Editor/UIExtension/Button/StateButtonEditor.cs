/*===============================================================
* Product:    Com2Verse
* File Name:  StateButtonEditor.cs
* Developer:  haminjeong
* Date:       2022-07-29 18:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(StateButton), true)]
	public class StateButtonEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");

			EditorGUILayout.Space();
			SerializedProperty stateListObject = serializedObject.FindProperty("_stateList");
			List<string> stateArray = new List<string>();
			for (int i = 0; i < stateListObject.arraySize; ++i)
				stateArray.Add(stateListObject.GetArrayElementAtIndex(i).stringValue);
			List<string> transitionArray = new List<string>();
			transitionArray.Add("Color");
			transitionArray.Add("Sprite");
			transitionArray.Add("Sprite & Color");

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Current State");
			int prevIdx = serializedObject.FindProperty("_currentIndex").intValue;
			serializedObject.FindProperty("_currentIndex").intValue = EditorGUILayout.Popup(serializedObject.FindProperty("_currentIndex").intValue, stateArray.ToArray());
			EditorGUILayout.EndHorizontal();

			int arraySize = stateListObject.arraySize;
			SerializedProperty targetGraphicObject = serializedObject.FindProperty("_targetGraphic");
			if (targetGraphicObject.objectReferenceValue != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Transition");
				serializedObject.FindProperty("_transition").intValue = EditorGUILayout.Popup(serializedObject.FindProperty("_transition").intValue, transitionArray.ToArray());
				EditorGUILayout.EndHorizontal();
				switch (serializedObject.FindProperty("_transition").intValue)
				{
					case 0:
					{
						EditorGUILayout.LabelField("Color List");
						SerializedProperty colorListObject = serializedObject.FindProperty("_stateColorList");
						if (arraySize != colorListObject.arraySize)
						{
							while (arraySize > colorListObject.arraySize)
							{
								colorListObject.InsertArrayElementAtIndex(colorListObject.arraySize);
							}

							while (arraySize < colorListObject.arraySize)
							{
								colorListObject.DeleteArrayElementAtIndex(colorListObject.arraySize - 1);
							}
						}

						for (int i = 0; i < arraySize; ++i)
						{
							if (colorListObject == null) break;
							SerializedProperty colorProperty = colorListObject.GetArrayElementAtIndex(i);
							SerializedProperty stateProperty = stateListObject.GetArrayElementAtIndex(i);

							EditorGUILayout.BeginHorizontal();
							colorProperty.colorValue = EditorGUILayout.ColorField(stateProperty.stringValue, colorProperty.colorValue);
							EditorGUILayout.EndHorizontal();
						}
					}
						break;
					case 1:
					{
						EditorGUILayout.LabelField("Sprite List");
						SerializedProperty spriteListObject = serializedObject.FindProperty("_stateSpriteList");
						if (arraySize != spriteListObject.arraySize)
						{
							while (arraySize > spriteListObject.arraySize)
							{
								spriteListObject.InsertArrayElementAtIndex(spriteListObject.arraySize);
							}

							while (arraySize < spriteListObject.arraySize)
							{
								spriteListObject.DeleteArrayElementAtIndex(spriteListObject.arraySize - 1);
							}
						}

						for (int i = 0; i < arraySize; ++i)
						{
							if (spriteListObject == null) break;
							SerializedProperty spriteProperty = spriteListObject.GetArrayElementAtIndex(i);
							SerializedProperty stateProperty = stateListObject.GetArrayElementAtIndex(i);

							EditorGUILayout.BeginHorizontal();
							spriteProperty.objectReferenceValue = EditorGUILayout.ObjectField(stateProperty.stringValue, spriteProperty.objectReferenceValue, typeof(Sprite), true);
							EditorGUILayout.EndHorizontal();
						}
					}
						break;
					case 2:
					{
						EditorGUILayout.LabelField("Sprite & Color List");
						SerializedProperty spriteListObject = serializedObject.FindProperty("_stateSpriteList");
						SerializedProperty colorListObject = serializedObject.FindProperty("_stateSpriteList");
						if (arraySize != spriteListObject.arraySize)
						{
							while (arraySize > spriteListObject.arraySize)
							{
								spriteListObject.InsertArrayElementAtIndex(spriteListObject.arraySize);
							}

							while (arraySize < spriteListObject.arraySize)
							{
								spriteListObject.DeleteArrayElementAtIndex(spriteListObject.arraySize - 1);
							}

							while (arraySize > colorListObject.arraySize)
							{
								colorListObject.InsertArrayElementAtIndex(colorListObject.arraySize);
							}

							while (arraySize < colorListObject.arraySize)
							{
								colorListObject.DeleteArrayElementAtIndex(colorListObject.arraySize - 1);
							}
						}

						for (int i = 0; i < arraySize; ++i)
						{
							if (spriteListObject == null) break;
							if (colorListObject == null) break;
							SerializedProperty spriteProperty = spriteListObject.GetArrayElementAtIndex(i);
							SerializedProperty colorProperty = colorListObject.GetArrayElementAtIndex(i);
							SerializedProperty stateProperty = stateListObject.GetArrayElementAtIndex(i);

							EditorGUILayout.BeginHorizontal();
							spriteProperty.objectReferenceValue = EditorGUILayout.ObjectField(stateProperty.stringValue, spriteProperty.objectReferenceValue, typeof(Sprite), true);
							colorProperty.colorValue = EditorGUILayout.ColorField(colorProperty.colorValue);
							EditorGUILayout.EndHorizontal();
						}
					}
						break;
				}
			}

			SerializedProperty subGraphicObject = serializedObject.FindProperty("_subGraphic");
			if (subGraphicObject.objectReferenceValue != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Transition (Sub)");
				serializedObject.FindProperty("_subTransition").intValue = EditorGUILayout.Popup(serializedObject.FindProperty("_subTransition").intValue, transitionArray.ToArray());
				EditorGUILayout.EndHorizontal();
				switch (serializedObject.FindProperty("_subTransition").intValue)
				{
					case 0:
					{
						EditorGUILayout.LabelField("Color List (Sub)");
						SerializedProperty colorSubListObject = serializedObject.FindProperty("_stateSubColorList");
						if (arraySize != colorSubListObject.arraySize)
						{
							while (arraySize > colorSubListObject.arraySize)
							{
								colorSubListObject.InsertArrayElementAtIndex(colorSubListObject.arraySize);
							}

							while (arraySize < colorSubListObject.arraySize)
							{
								colorSubListObject.DeleteArrayElementAtIndex(colorSubListObject.arraySize - 1);
							}
						}

						for (int i = 0; i < arraySize; ++i)
						{
							if (colorSubListObject == null) break;
							SerializedProperty colorProperty = colorSubListObject.GetArrayElementAtIndex(i);
							SerializedProperty stateProperty = stateListObject.GetArrayElementAtIndex(i);

							EditorGUILayout.BeginHorizontal();
							colorProperty.colorValue = EditorGUILayout.ColorField(stateProperty.stringValue, colorProperty.colorValue);
							EditorGUILayout.EndHorizontal();
						}
					}
						break;
					case 1:
					{
						EditorGUILayout.LabelField("Sprite List (Sub)");
						SerializedProperty spriteSubListObject = serializedObject.FindProperty("_stateSubSpriteList");
						if (arraySize != spriteSubListObject.arraySize)
						{
							while (arraySize > spriteSubListObject.arraySize)
							{
								spriteSubListObject.InsertArrayElementAtIndex(spriteSubListObject.arraySize);
							}

							while (arraySize < spriteSubListObject.arraySize)
							{
								spriteSubListObject.DeleteArrayElementAtIndex(spriteSubListObject.arraySize - 1);
							}
						}

						for (int i = 0; i < arraySize; ++i)
						{
							if (spriteSubListObject == null) break;
							SerializedProperty spriteProperty = spriteSubListObject.GetArrayElementAtIndex(i);
							SerializedProperty stateProperty = stateListObject.GetArrayElementAtIndex(i);

							EditorGUILayout.BeginHorizontal();
							spriteProperty.objectReferenceValue = EditorGUILayout.ObjectField(stateProperty.stringValue, spriteProperty.objectReferenceValue, typeof(Sprite), true);
							EditorGUILayout.EndHorizontal();
						}
					}
						break;
					case 2:
					{
						EditorGUILayout.LabelField("Sprite & Color List (Sub)");
						SerializedProperty spriteSubListObject = serializedObject.FindProperty("_stateSubSpriteList");
						SerializedProperty colorSubListObject = serializedObject.FindProperty("_stateSubColorList");
						if (arraySize != spriteSubListObject.arraySize)
						{
							while (arraySize > spriteSubListObject.arraySize)
							{
								spriteSubListObject.InsertArrayElementAtIndex(spriteSubListObject.arraySize);
							}

							while (arraySize < spriteSubListObject.arraySize)
							{
								spriteSubListObject.DeleteArrayElementAtIndex(spriteSubListObject.arraySize - 1);
							}

							while (arraySize > colorSubListObject.arraySize)
							{
								colorSubListObject.InsertArrayElementAtIndex(colorSubListObject.arraySize);
							}

							while (arraySize < colorSubListObject.arraySize)
							{
								colorSubListObject.DeleteArrayElementAtIndex(colorSubListObject.arraySize - 1);
							}
						}

						for (int i = 0; i < arraySize; ++i)
						{
							if (spriteSubListObject == null) break;
							if (colorSubListObject == null) break;
							SerializedProperty spriteProperty = spriteSubListObject.GetArrayElementAtIndex(i);
							SerializedProperty colorProperty = colorSubListObject.GetArrayElementAtIndex(i);
							SerializedProperty stateProperty = stateListObject.GetArrayElementAtIndex(i);

							EditorGUILayout.BeginHorizontal();
							spriteProperty.objectReferenceValue = EditorGUILayout.ObjectField(stateProperty.stringValue, spriteProperty.objectReferenceValue, typeof(Sprite), true);
							colorProperty.colorValue = EditorGUILayout.ColorField(colorProperty.colorValue);
							EditorGUILayout.EndHorizontal();
						}
					}
						break;
				}
			}

			StateButton stateButton = target as StateButton;
			if (prevIdx != serializedObject.FindProperty("_currentIndex").intValue)
				stateButton.Index = serializedObject.FindProperty("_currentIndex").intValue;

			serializedObject.ApplyModifiedProperties();
		}
	}
}

