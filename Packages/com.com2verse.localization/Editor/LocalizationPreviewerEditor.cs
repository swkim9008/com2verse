/*===============================================================
* Product:		Com2Verse
* File Name:	LocalizationEditor.cs
* Developer:	tlghks1009
* Date:			2022-12-20 11:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2Verse.UI
{
	public sealed class LocalizationPreviewerEditor : EditorWindow
	{
		private void OnGUI() => DrawLanguageButtonList();


		private void DrawLanguageButtonList()
		{
			using (new EditorGUILayout.VerticalScope())
			{
				foreach (Localization.eLanguage languageType in Enum.GetValues(typeof(Localization.eLanguage)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();
						{
							if (GUILayout.Button(languageType.ToString(), GUILayout.Width(300)))
							{
								ChangeLanguage(languageType).Forget();
							}
						}
						GUILayout.FlexibleSpace();
					}
				}
			}
		}


		private async UniTask<TableString> LoadStringTable()
		{
			var stringTextAsset = Addressables.LoadAssetAsync<TextAsset>("String.bytes");

			await stringTextAsset;

			var resultInfo = await Loader.LoadAsync(typeof(TableString), stringTextAsset.Result.bytes);

			if (resultInfo.Data is TableString tableString)
			{
				return tableString;
			}

			return null;
		}


		private async UniTask ChangeLanguage(Localization.eLanguage languageType)
		{
			var tableString = await LoadStringTable();

			if (tableString == null)
			{
				return;
			}

			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

			if (prefabStage == null)
			{
				return;
			}

			var gameObjectOfPrefabStageRoot = prefabStage.prefabContentsRoot;
			var localizationUIs = gameObjectOfPrefabStageRoot.GetComponentsInChildren<LocalizationUI>();

			foreach (var localizationUI in localizationUIs)
			{
				var textKey = localizationUI.TextKey;

				if (tableString.Datas.TryGetValue(textKey!, out var stringValue))
				{
					switch (languageType)
					{
						case Localization.eLanguage.KOR:
							localizationUI.SetText(stringValue.KO);
							break;
						case Localization.eLanguage.ENG:
							localizationUI.SetText(stringValue.EN);
							break;
						default:
							localizationUI.SetText(stringValue.KO);
							break;
					}
				}
			}
		}


		[MenuItem("Com2Verse/Localization Previewer", false)]
		private static void OpenLocalizationPreviewer()
		{
			var window = GetWindow<LocalizationPreviewerEditor>();
			window.titleContent = new GUIContent("Localization Previewer");
			window.minSize = new Vector2(300, 300);
			window.Show();
		}
	}
}
