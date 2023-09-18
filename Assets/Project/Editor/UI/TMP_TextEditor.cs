/*===============================================================
* Product:    Com2Verse
* File Name:  TMP_TextEditor.cs
* Developer:  haminjeong
* Date:       2022-11-25 11:38
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using TMPro;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Com2VerseEditor.UI
{
	[CustomEditor(typeof(TextMeshProUGUI), true), CanEditMultipleObjects]
	public class TMP_TextEditor : TMP_EditorPanelUI
	{
		private string _textKey = string.Empty;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var localization = target.GetComponent<LocalizationUI>();
			if (!localization.IsUnityNull())
				_textKey = localization.TextKey;
			
			EditorGUILayout.Space();

			var prevTextKey = _textKey;
			_textKey = EditorGUILayout.TextField("TextKey", _textKey);
			if (!prevTextKey.Equals(_textKey))
			{
				if (string.IsNullOrEmpty(_textKey))
				{
					if (!localization.IsUnityNull())
						DestroyImmediate(localization);
				}
				else
				{
					if (localization.IsUnityNull())
						localization = target.AddComponent<LocalizationUI>();
					localization.TextKey = _textKey;
				}
				if (!m_HavePropertiesChanged)
					m_HavePropertiesChanged = true;
			}
			
			if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
			{
				m_TextComponent.havePropertiesChanged = true;
				m_HavePropertiesChanged = false;
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

