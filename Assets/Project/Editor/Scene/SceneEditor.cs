/*===============================================================
* Product:		Com2Verse
* File Name:	SceneEditor.cs
* Developer:	eugene9721
* Date:			2022-06-17 18:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Com2VerseEditor
{
	public sealed class SceneEditor
	{
		[InitializeOnLoad]
		static class EditorSceneManagerSceneSaved {
			static EditorSceneManagerSceneSaved () {
				UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
			}

			static void OnSceneSaving (UnityEngine.SceneManagement.Scene scene, string path) {
				C2VDebug.Log($"Saving scene '{scene.name}' to {path}");
				var eventSystems = Object.FindObjectsOfType<EventSystem>();
				foreach (var eventSystem in eventSystems)
				{
					C2VDebug.LogWarning($"[Editor] '{eventSystem.gameObject.name}' EventSystem has been removed");
					if (eventSystem.TryGetComponent(out StandaloneInputModule standaloneInputModule))
					{
						Object.DestroyImmediate(standaloneInputModule);
					}
					if (eventSystem.TryGetComponent(out InputSystemUIInputModule inputSystemUIInput))
					{
						Object.DestroyImmediate(inputSystemUIInput);
					}
					Object.DestroyImmediate(eventSystem);
				}
			}
		}
	}
}
