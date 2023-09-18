/*===============================================================
* Product:		Com2Verse
* File Name:	SceneStarter.cs
* Developer:	tlghks1009
* Date:			2022-05-17 10:24
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using Com2Verse;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.RenderFeatures.Data;
using Com2Verse.UI;
using Com2VerseEditor.ArtTools.Utility;
using Com2VerseEditor.AssetSystem;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Com2VerseEditor
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle COMMAND_BUTTON_STYLE;

		static ToolbarStyles()
		{
			COMMAND_BUTTON_STYLE = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold
			};
		}
	}

	[InitializeOnLoad]
	public class EditorToolbarGUI
	{
		private static string StatePath => Path.Join(Application.persistentDataPath, ".editor_toolbar_gui_state");
		private static EditorToolbarGUIState _state;
		private static readonly GUILayoutOption[] Width32 = { GUILayout.Width(32.0f) };
		private static readonly GUILayoutOption[] Width82 = { GUILayout.Width(82.0f) };
		private static readonly GUIContent ReloadDomainGUIContent = new("R", "Reload Domain");
		private static readonly GUIContent StartSceneSplashGUIContent = new("P", "Start Scene Splash");
		private static readonly GUIContent AutoLoginGUIContent = new(ArtToolsIcons.Instance.LoginIcon, "Toggle to auto-login");

		static EditorToolbarGUI()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
			ToolbarExtender.RightToolbarGUI.Insert(0, OnToolbarGUIRight);
		}

		static void OnToolbarGUILeft()
		{
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(ReloadDomainGUIContent, ToolbarStyles.COMMAND_BUTTON_STYLE))
			{
				if (UnitySceneManager.GetActiveScene().isDirty)
				{
					EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				}

				EditorUtility.RequestScriptReload();

				CreateAddressableGroup();
			}

			if (GUILayout.Button(StartSceneSplashGUIContent, ToolbarStyles.COMMAND_BUTTON_STYLE))
			{
				if (UnitySceneManager.GetActiveScene().isDirty)
				{
					EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				}

				SceneHelper.StartScene("SceneSplash");
			}
		}

		static void CreateAddressableGroup()
		{
			using var compositionRoot = C2VAddressablesEditorCompositionRoot.RequestInstance();
			if (compositionRoot == null)
			{
				C2VDebug.LogError("Please click the 'R' button again.");
				return;
			}

			var groupBuilderController = compositionRoot.GroupBuilderController;

			groupBuilderController.CreateGroup(eEnvironment.LOCAL);
		}

		private static async void OnToolbarGUIRight()
		{
			_state ??= await FileSerializable.Load<EditorToolbarGUIState>(StatePath);
			using var changed = new EditorGUI.ChangeCheckScope();

			_state._loginUserName = EditorGUILayout.TextField(_state._loginUserName, Width82);
			using (var changedAutoLoginEnabled = new EditorGUI.ChangeCheckScope())
			{
				_state._autoLoginEnabled = GUILayout.Toggle(_state._autoLoginEnabled, AutoLoginGUIContent, EditorStyles.toolbarButton, Width32);
				if (changedAutoLoginEnabled.changed) _state.CanSendLogIn |= _state._autoLoginEnabled;
			}

			EditorApplication.update -= AutoLoginPolls;
			if (_state._autoLoginEnabled) EditorApplication.update += AutoLoginPolls;
			if (changed.changed) FileSerializable.Save(_state, StatePath);
		}

		private static void AutoLoginPolls()
		{
			_state.CanSendLogIn |= !Application.isPlaying;

			if (!_state.CanSendLogIn) return;
			if (!Application.isPlaying) return;
			if (CurrentScene.Scene is not SceneLogin) return;
			if (CurrentScene.Scene.SceneState is not eSceneState.LOADED) return;

			foreach (var inputField in UnityEngine.Object.FindObjectsOfType<TMP_InputField>())
			{
				if (inputField.GetComponent<DataBinder>() is not { } binder || binder.SourceOwnerType != typeof(NewAuthLoginViewModel)) continue;
				inputField.text = _state._loginUserName;
				_state.CanSendLogIn = false;

				if (LoginManager.InstanceOrNull is { } loginManager)
				{
					loginManager.RequestCom2VerseLogin(LoginManager.eCom2VerseLoginType.HIVE_DEV, _state._loginUserName);
				} else Debug.LogWarning("failed to find login manager");

				EditorApplication.update -= AutoLoginPolls;
				break;
			}
		}
	}

	[Serializable]
	public class EditorToolbarGUIState
	{
		[SerializeField] public string _loginUserName;
		[SerializeField] public bool _autoLoginEnabled;
		public bool CanSendLogIn { get; set; }
	}

	static class SceneHelper
	{
		static string sceneToOpen;

		public static void StartScene(string sceneName)
		{
			if (EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
			}

			sceneToOpen = sceneName;
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (sceneToOpen == null ||
			    EditorApplication.isPlaying || EditorApplication.isPaused ||
			    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnUpdate;

			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				// need to get scene via search because the path to the scene
				// file contains the package version so it'll change over time
				string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
				if (guids.Length == 0)
				{
					Debug.LogWarning("Couldn't find scene file");
				}
				else
				{
					string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
					EditorSceneManager.OpenScene(scenePath);
					EditorApplication.isPlaying = true;
				}
			}
			sceneToOpen = null;
		}
	}
}
