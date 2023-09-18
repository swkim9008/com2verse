/*===============================================================
* Product:		Com2Verse
* File Name:	GuiVIewEditor.cs
* Developer:	tlghks1009
* Date:			2022-05-20 16:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(GUIView), true)]
	public sealed class GuiViewEditor : Editor
	{
		private GUIView _guiView;
		private bool    _addStackRegisterer = false;

		private void OnEnable()
		{
			_guiView             = target as GUIView;
			_addStackRegisterer = !target.GetComponent<GUIViewStackRegisterer>().IsUnityNull();
		}

		private void OnDisable()
		{
			_guiView = null;
		}


		public override void OnInspectorGUI()
		{
			DrawViewName();
			DrawTransitionType();
			DrawSound();
			DrawStackRegisterer();

			DrawDefaultInspectorWithoutScriptField();
		}


		private bool DrawDefaultInspectorWithoutScriptField()
		{
			EditorGUI.BeginChangeCheck();

			this.serializedObject.Update();
			SerializedProperty iterator = this.serializedObject.GetIterator();

			iterator.NextVisible(true);

			while (iterator.NextVisible(false))
			{
				EditorGUILayout.PropertyField(iterator, true);
			}

			this.serializedObject.ApplyModifiedProperties();

			return (EditorGUI.EndChangeCheck());
		}


		private void DrawViewName()
		{
			serializedObject.Update();

			serializedObject.FindProperty("_viewName").stringValue = _guiView.gameObject.name;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_viewName"));

			serializedObject.ApplyModifiedProperties();
		}


		private void DrawTransitionType()
		{
			serializedObject.Update();

			var serializedTransitionType = serializedObject.FindProperty("_transitionType");
			var transitionType = (GUIView.eActiveTransitionType) serializedTransitionType.intValue;
			EditorGUILayout.PropertyField(serializedTransitionType);

			serializedObject.ApplyModifiedProperties();

			serializedObject.Update();

			if (transitionType == GUIView.eActiveTransitionType.ANIMATION)
			{
				var transformOfRoot = _guiView.transform.Find("Root");
				if (!ReferenceEquals(transformOfRoot, null))
				{
					var animationPlayer = transformOfRoot.GetOrAddComponent<AnimationPlayer>();
					if (ReferenceEquals(animationPlayer, null))
					{
						Debug.LogError("Can't find AnimationPlayer Component.");
						return;
					}

					serializedObject.FindProperty("_animationPlayer").objectReferenceValue = animationPlayer;
				}
			}
			else if (transitionType == GUIView.eActiveTransitionType.FADE)
			{
				EditorGUI.indentLevel += 1;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_fadeSpeed"));
				EditorGUI.indentLevel -= 1;
			}

			var defaultActiveStateProperty = serializedObject.FindProperty("_defaultActiveState");
			EditorGUILayout.PropertyField(defaultActiveStateProperty);

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawSound()
		{
			serializedObject.Update();

			var isPlaySound = serializedObject.FindProperty("_isPlaySound").boolValue;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_isPlaySound"));

			if (isPlaySound)
			{
				EditorGUI.indentLevel += 1;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioFileWhenActivated"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioFileWhenInactivated"));
				EditorGUI.indentLevel -= 1;
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawStackRegisterer()
		{
			if (_guiView == null) return;

			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Add to Stack", EditorStyles.boldLabel);

			var stackManager       = target.GetComponent<GUIViewStackRegisterer>();
			var addStackRegisterer = !stackManager.IsUnityNull();
			_addStackRegisterer = EditorGUILayout.Toggle("Add Stack Registerer", addStackRegisterer);

			if (_addStackRegisterer)
			{
				if (stackManager.IsUnityNull())
				{
					target.AddComponent<GUIViewStackRegisterer>();
					Save();
				}
			}
			else
			{
				if (!stackManager.IsUnityNull())
				{
					DestroyImmediate(stackManager);
					Save();
				}
			}

			serializedObject.ApplyModifiedProperties();

			void Save()
			{
				EditorUtility.SetDirty(target);
				var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				if (!prefabStage.IsUnityNull())
				{
					EditorSceneManager.MarkSceneDirty(prefabStage.scene);
				}
			}
		}
	}
}
