/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressableEditorCompositionRoot.cs
* Developer:	tlghks1009
* Date:			2023-03-03 15:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
    public sealed class C2VAddressablesEditorCompositionRoot : IDisposable
    {
        private static int _referenceCount;

        private static string _currentVersion = "70";

        private static string _versionInfoFileName              = "AddressableAssetSettingsVersionInfo.asset";
        private static string _addressableAssetSettingsFileName = "AddressableAssetSettings.asset";

        private static C2VAddressablesEditorCompositionRoot _instance;

        public C2VAddressablesGroupBuilderWindowController GroupBuilderWindowController { get; private set; }

        public C2VAddressablesGroupBuilderController GroupBuilderController { get; private set; }

        public C2VAddressablesGroupBuilderServicePack ServicePack { get; private set; }


        private C2VAddressablesEditorCompositionRoot(C2VAddressablesGroupBuilderWindow window = null)
        {
            if (CheckForAddressableAssetSettingsUpdate())
            {
                RemoveOldAddressableAssetSettingsFile();

                MakeAddressableAssetSettingsFileIfNotExist();

                Debug.Log("AddressableAssetSettings를 새로운 버전으로 업데이트 합니다.");
            }

            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                var addressableAssetSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>($"Assets/AddressableAssetsData/{_addressableAssetSettingsFileName}");
                if (addressableAssetSettings != null)
                {
                    AddressableAssetSettingsDefaultObject.Settings = addressableAssetSettings;
                }
                else
                {
                    Debug.LogWarning("AddressableAssetSettings이 null 입니다. 다시 시도 해주세요.");
                    return;
                }
            }

            ServicePack = new C2VAddressablesGroupBuilderServicePack();

            GroupBuilderController = new C2VAddressablesGroupBuilderController(ServicePack);

            GroupBuilderWindowController = new C2VAddressablesGroupBuilderWindowController(window, ServicePack, GroupBuilderController);
        }


        public static C2VAddressablesEditorCompositionRoot RequestInstance(C2VAddressablesGroupBuilderWindow window = null)
        {
            if (_referenceCount++ == 0)
            {
                _instance = new C2VAddressablesEditorCompositionRoot(window);
            }
            else
            {
                if (CheckForAddressableAssetSettingsUpdate())
                {
                    RemoveOldAddressableAssetSettingsFile();

                    MakeAddressableAssetSettingsFileIfNotExist();

                    Debug.Log("AddressableAssetSettings를 새로운 버전으로 업데이트 합니다. 'R' 버튼을 클릭해주세요.");
                }

                if (AddressableAssetSettingsDefaultObject.Settings == null)
                {
                    var addressableAssetSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>($"Assets/AddressableAssetsData/{_addressableAssetSettingsFileName}");
                    if (addressableAssetSettings != null)
                    {
                        AddressableAssetSettingsDefaultObject.Settings = addressableAssetSettings;
                    }
                    else
                    {
                        Debug.LogWarning("AddressableAssetSettings이 null 입니다. 다시 시도 해주세요.");
                    }
                }
            }
            return _instance;
        }


        public void Dispose()
        {
            if (--_referenceCount == 0)
            {
                _instance = null;

                AssetDatabase.SaveAssets();

                ServicePack?.Release();
                GroupBuilderWindowController?.Release();
                GroupBuilderController?.Release();

                ServicePack = null;
                GroupBuilderWindowController = null;
                GroupBuilderController = null;
            }
        }


        private static bool CheckForAddressableAssetSettingsUpdate()
        {
            if (!File.Exists($"{Application.dataPath}/AddressableAssetsData/AddressableAssetSettings.asset"))
            {
                return true;
            }

            if (File.Exists($"{Application.dataPath}/AddressableAssetsData/{_versionInfoFileName}"))
            {
                AddressableAssetSettingsVersionInfo versionInfo;
                try
                {
                    versionInfo = AssetDatabase.LoadAssetAtPath<AddressableAssetSettingsVersionInfo>("Assets/AddressableAssetsData/AddressableAssetSettingsVersionInfo.asset");

                    if (versionInfo.Version == _currentVersion)
                    {
                        return false;
                    }

                    Debug.Log("AddressableAssetSettings Version이 다릅니다.");
                    return true;
                }
                catch (Exception e)
                {
                    return true;
                }
            }
            Debug.Log("AddressableAssetSettings Version 정보가 없습니다.");

            return true;
        }

        private static void MakeAddressableAssetSettingsFileIfNotExist()
        {
            var settingsFilePath = $"{Application.dataPath}/AddressableAssetsData/AddressableAssetSettings.asset";

            if (!File.Exists(settingsFilePath))
            {
                var settingsFile = Path.GetFullPath(Path.Combine(Application.dataPath!, $@"../AddressableAssetSettings/{_addressableAssetSettingsFileName}"));
                File.Copy(settingsFile, settingsFilePath, true);
                Debug.Log("AddressableSettings 파일을 외부에서 Copy 합니다.");

                AssetDatabase.ImportAsset($"Assets/AddressableAssetsData/{_addressableAssetSettingsFileName}", ImportAssetOptions.ForceSynchronousImport);
                Debug.Log("AddressableSettings 파일을 Import 합니다.");
                AssetDatabase.SaveAssets();
                Debug.Log("AddressableSettings 파일을 Save 합니다.");
                AssetDatabase.Refresh();

                var addressableAssetSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>($"Assets/AddressableAssetsData/{_addressableAssetSettingsFileName}");
                if (addressableAssetSettings == null)
                {
                    Debug.LogError("AddressableAssetSettings 파일을 불러오지 못했습니다.");
                    return;
                }
                EditorUtility.SetDirty(addressableAssetSettings);
                AddressableAssetSettingsDefaultObject.Settings = addressableAssetSettings;
            }

            var settingsVersionFilePath = $"{Application.dataPath}/AddressableAssetsData/{_versionInfoFileName}";

            if (!File.Exists(settingsVersionFilePath))
            {
                var so = ScriptableObject.CreateInstance<AddressableAssetSettingsVersionInfo>();
                so.Version = _currentVersion;

                AssetDatabase.CreateAsset(so, $"Assets/AddressableAssetsData/{_versionInfoFileName}");
                AssetDatabase.ImportAsset($"Assets/AddressableAssetsData/{_versionInfoFileName}");
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }


        private static void RemoveOldAddressableAssetSettingsFile()
        {
            AssetDatabase.DeleteAsset($"Assets/AddressableAssetsData/AssetGroups");
            AssetDatabase.DeleteAsset($"Assets/AddressableAssetsData/{_addressableAssetSettingsFileName}");
            AssetDatabase.DeleteAsset($"Assets/AddressableAssetsData/DefaultObject.asset");
            AssetDatabase.DeleteAsset($"Assets/AddressableAssetsData/{_versionInfoFileName}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetBundle] Addressable Settings가 모두 제거 되었습니다.");
        }
    }
}
