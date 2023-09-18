/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEditorAdapter.cs
* Developer:	tlghks1009
* Date:			2023-03-03 13:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
    public sealed class C2VAddressablesEditorAdapter
    {
        private AddressableAssetSettings _settings;

        public C2VAddressablesEditorAdapter() => _settings = AddressableAssetSettingsDefaultObject.Settings;


        public AddressableAssetGroup CreateOrGetGroup(string groupName)
        {
            MakeDefaultGroupIfNotExist();

            var addressableAssetGroup = GetGroup(groupName);
            if (addressableAssetGroup != null)
            {
                return addressableAssetGroup;
            }

            var settingsGroupTemplateObject = _settings.GroupTemplateObjects[0] as AddressableAssetGroupTemplate;

            addressableAssetGroup = _settings.CreateGroup(groupName, false, false, true, null, settingsGroupTemplateObject.GetTypes());

            return addressableAssetGroup;
        }


        public void CreateOrMoveEntry(C2VAddressableEntryCreateInfo createInfo)
        {
            var group = CreateOrGetGroup(createInfo.GroupName);
            var entry = createInfo.AddressableAssetEntry;

            if (entry != null && entry.parentGroup == group)
            {
                SetupAddress();
                SetupLabels();
                return;
            }

            SetupEntry();

            void SetupEntry()
            {
                entry = _settings.CreateOrMoveEntry(createInfo.Guid, group);

                SetupAddress();
                SetupLabels();
            }

            void SetupAddress()
            {
                entry.address = createInfo.Address;
            }

            void SetupLabels()
            {
                entry.labels.Clear();
                entry.SetLabelsByLabelEnumValue(createInfo.LabelEnumValue);
            }
        }


        public void RemoveAssetEntry(C2VAddressableEntryRemoveInfo removeInfo)
        {
            var entry = removeInfo.AddressableAssetEntry;
            if (entry == null)
            {
                return;
            }

            var group = entry.parentGroup;
            if (group != null)
            {
                group.RemoveAssetEntry(entry);

                if (group.entries.Count == 0)
                {
                    RemoveGroup(group);
                }
            }
        }

        public AddressableAssetEntry FindAssetEntry(string guid)
        {
            return _settings.FindAssetEntry(guid);
        }


        public AddressableAssetGroup FindGroup(string guid)
        {
            var entry = FindAssetEntry(guid);

            return entry?.parentGroup;
        }


        public AddressableAssetGroup FindGroupByName(string groupName)
        {
            var group = _settings.FindGroup(groupName);

            return group;
        }


        public string FindGroupName(string guid)
        {
            var group = FindGroup(guid);

            return group == null ? string.Empty : group.name;
        }


        public void RemoveGroup(AddressableAssetGroup assetGroup)
        {
            if (assetGroup == _settings.DefaultGroup)
            {
                return;
            }

            _settings.RemoveGroup(assetGroup);
        }


        public void RemoveGroup(string groupName)
        {
            var group = _settings.FindGroup(groupName);

            if (group == null)
            {
                return;
            }

            RemoveGroup(group);
        }


        public AddressableAssetGroup GetGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return null;
            }

            var group = _settings.FindGroup(groupName);

            return group;
        }


        public bool ExistsAddress(string address)
        {
            foreach (var group in _settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.address == address)
                    {
                        if (string.IsNullOrEmpty(entry.AssetPath))
                        {
                            group.RemoveAssetEntry(entry);
                            return false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        public bool CanRemoveAssetEntry(string guid)
        {
            var entry = FindAssetEntry(guid);
            if (entry == null)
            {
                return false;
            }

            var group = entry.parentGroup;
            return group != null;
        }


        public bool CanCreateOrMoveEntry(C2VAddressablesEntryOperationInfo operationInfo)
        {
            if ((operationInfo.EntryRemoveInfo is {IsValid: true}) || (operationInfo.GroupRemoveInfo is {IsValid: true}))
            {
                return true;
            }

            var createInfo = operationInfo.EntryCreateInfo;
            var entry = createInfo.AddressableAssetEntry;

            if (entry != null && entry.Equals(createInfo.Address, createInfo.GroupName, createInfo.LabelEnumValue))
            {
                return false;
            }

            if (entry == null && ExistsAddress(createInfo.Address))
            {
                Debug.LogError($"[Duplicate Address] {createInfo.Address} 가 중복입니다. assetPath : {createInfo.AssetPath}");
                return false;
            }

            return true;
        }


        public void RemoveMissingReferences()
        {
            RemoveMissingAssetEntries();
            RemoveMissingGroupsImpl();

#region MissingAssetEntries
            void RemoveMissingAssetEntries()
            {
                var missingAssetEntries = new List<AddressableAssetEntry>();

                foreach (var group in _settings.groups)
                {
                    if (group == null)
                    {
                        continue;
                    }

                    if (group.name != _defaultLocalGroupName && group.name != _builtInDataGroupName)
                    {
                        foreach (var assetEntry in group.entries)
                        {
                            if (!IsValidAddressableAsset(assetEntry))
                            {
                                missingAssetEntries.Add(assetEntry);
                            }
                        }
                    }
                }

                foreach (var missingAssetEntry in missingAssetEntries)
                {
                    var group = missingAssetEntry.parentGroup;

                    group.RemoveAssetEntry(missingAssetEntry);

                    Debug.Log($"Invalid addressable asset Removed. Address : {missingAssetEntry.address}");
                }

                missingAssetEntries.Clear();
            }


            bool IsValidAddressableAsset(AddressableAssetEntry assetEntry)
            {
                if (string.IsNullOrEmpty(assetEntry.AssetPath))
                {
                    return false;
                }

                if (assetEntry.TargetAsset == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(assetEntry.guid)))
                {
                    return false;
                }

                return assetEntry.address == Path.GetFileName(assetEntry.AssetPath);
            }
#endregion

#region MissingGroups
            void RemoveMissingGroupsImpl()
            {
                if (RemoveMissingGroupReferences())
                {
                    _settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true, true);
                }
            }

            bool RemoveMissingGroupReferences()
            {
                var missingGroupsIndices = new List<int>();
                for (int i = 0; i < _settings.groups.Count; i++)
                {
                    var group = _settings.groups[i];
                    if (group == null)
                    {
                        missingGroupsIndices.Add(i);
                    }
                    else
                    {
                        if (group.name != _defaultLocalGroupName && group.name != _builtInDataGroupName)
                        {
                            if (group.entries.Count == 0)
                            {
                                missingGroupsIndices.Add(i);
                            }
                        }
                    }
                }

                if (missingGroupsIndices.Count > 0)
                {
                    Debug.Log("Addressable settings contains " + missingGroupsIndices.Count + " group reference(s) that are no longer there. Removing reference(s).");
                    for (int i = missingGroupsIndices.Count - 1; i >= 0; i--)
                    {
                        _settings.groups.RemoveAt(missingGroupsIndices[i]);
                    }

                    return true;
                }

                return false;
            }
#endregion
        }


        public void RemoveAllGroup()
        {
            var totalGroupCount = _settings.groups.Count;

            for (int i = 0; i < totalGroupCount; i++)
            {
                var group = _settings.groups[0];
                _settings.RemoveGroup(group);
            }
        }


        public void Release() => _settings = null;


        private static readonly string _defaultLocalGroupName = "defaultLocalGroup";
        private static readonly string _builtInDataGroupName  = "Built In Data";
        private static readonly string _resourcesGuid         = "Resources";
        private static readonly string _editorSceneList       = "EditorSceneList";


        public void MakeDefaultGroupIfNotExist()
        {
            if (_settings.FindGroup(_defaultLocalGroupName) == null)
            {
                var settingsGroupTemplateObject = _settings.GroupTemplateObjects[0] as AddressableAssetGroupTemplate;

                _settings.DefaultGroup = _settings.CreateGroup(_defaultLocalGroupName, false, false, true, null, settingsGroupTemplateObject.GetTypes());

                EditorUtility.SetDirty(_settings.DefaultGroup);
            }

            if (_settings.FindGroup(_builtInDataGroupName) == null)
            {
                var builtInDataGroup = _settings.CreateGroup(_builtInDataGroupName, false, false, false, null, typeof(PlayerDataGroupSchema));
                var resourceEntry = _settings.CreateOrMoveEntry(_resourcesGuid, builtInDataGroup, false, false);
                resourceEntry.IsInResources = true;

                _settings.CreateOrMoveEntry(_editorSceneList, builtInDataGroup, false, false);
                EditorUtility.SetDirty(builtInDataGroup);
            }
        }
    }
}
