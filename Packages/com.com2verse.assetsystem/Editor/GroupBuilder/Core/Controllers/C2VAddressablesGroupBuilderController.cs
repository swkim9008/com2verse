/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEditorController.cs
* Developer:	tlghks1009
* Date:			2023-03-03 15:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2VerseEditor.AssetSystem
{
    public sealed class C2VAddressablesGroupBuilderController
    {
        private static string _forceCleanGroupString = "cleanGroup_50";

        private IServicePack _servicePack;

        private readonly C2VAddressablesEditorAdapter _addressableEditorAdapter;

        private C2VAssetDataBaseAdapter      _assetDataBaseAdapter;
        private C2VAddressablesPathGenerator _addressablePathGenerator;
        private C2VAddressablesEntryGroupBuildService _entryGroupBuildService;
        private C2VAddressablesEntryOperationInfoService _entryOperationInfoService;
        private C2VAddressablesGroupBuilderStorageService _groupBuilderStorageService;


        public C2VAddressablesGroupBuilderController(IServicePack servicePack)
        {
            _servicePack = servicePack;

            _groupBuilderStorageService = servicePack.GetService<C2VAddressablesGroupBuilderStorageService>();
            _entryGroupBuildService = servicePack.GetService<C2VAddressablesEntryGroupBuildService>();
            _entryOperationInfoService = servicePack.GetService<C2VAddressablesEntryOperationInfoService>();

            _addressablePathGenerator = _entryGroupBuildService.AddressablePathGenerator;
            _assetDataBaseAdapter     = _entryGroupBuildService.AssetDataBaseAdapter;
            _addressableEditorAdapter = _entryGroupBuildService.AddressableEditorAdapter;
        }


        public void ProcessInitialize()
        {
            ForceRemoveAllGroupIfSettingChanged();

            _addressableEditorAdapter.MakeDefaultGroupIfNotExist();

            _addressableEditorAdapter.RemoveMissingReferences();
        }


        private void ForceRemoveAllGroupIfSettingChanged()
        {
            if (!PlayerPrefs.HasKey(_forceCleanGroupString))
            {
                RemoveAllGroup();
                PlayerPrefs.SetString(_forceCleanGroupString, "true");
            }
        }


        public void RemoveAllGroup() => _addressableEditorAdapter.RemoveAllGroup();


        public void CreateGroup(eEnvironment environment)
        {
            var entryOperationDictionary = new C2VEntryOperationDictionary();

            ProcessInitialize();

            ExecutePreProcess(environment);

            foreach (var assetPath in _assetDataBaseAdapter.GetAllAssetPaths())
            {
                ProcessStart(assetPath, entryOperationDictionary);
            }

            ProcessApply(entryOperationDictionary);

            ExecutePostProcess(environment);
        }


        private void ProcessStart(string assetPath, C2VEntryOperationDictionary entryOperationDictionary)
        {
            C2VAddressablesEntryOperationInfo entryOperationInfo = null;

            foreach (var ruleEntry in _groupBuilderStorageService.GroupRuleEntries)
            {
                var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(ruleEntry.GetProjectRootPath(), ruleEntry.RootPath, assetPath);

                if (!string.IsNullOrEmpty(addressablePath))
                {
                    var createEntryOperationInfo = _entryGroupBuildService.BuildCreateEntryOperationInfoByRule(assetPath, ruleEntry);

                    if (createEntryOperationInfo == null)
                    {
                        continue;
                    }

                    if (entryOperationDictionary.ContainsKey(createEntryOperationInfo.Address))
                    {
                        Debug.LogError($"[Duplicate Address] {createEntryOperationInfo.Address} 가 중복입니다. assetPath : {assetPath}");

                        continue;
                    }

                    entryOperationInfo = new C2VAddressablesEntryOperationInfo(createEntryOperationInfo, null, null);

                    entryOperationDictionary.Add(createEntryOperationInfo.Address, entryOperationInfo);
                }
            }

            if (entryOperationInfo == null)
            {
                var removeEntryOperationInfo = _entryGroupBuildService.BuildRemoveEntryOperationInfo(assetPath, null);

                entryOperationInfo = new C2VAddressablesEntryOperationInfo(null, removeEntryOperationInfo, null);

                entryOperationDictionary.Add(assetPath, entryOperationInfo);
            }
        }


        public void ProcessImportedAsset(string assetPath, C2VEntryOperationDictionary entryOperationDictionary)
        {
            foreach (var rule in _groupBuilderStorageService.GroupRuleEntries)
            {
                var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, assetPath);

                if (string.IsNullOrEmpty(addressablePath))
                {
                    continue;
                }

                var createEntryOperationInfo = _entryGroupBuildService.BuildCreateEntryOperationInfoByRule(assetPath, rule);

                if (createEntryOperationInfo == null)
                {
                    continue;
                }

                if (entryOperationDictionary.ContainsKey(createEntryOperationInfo.Address))
                {
                    continue;
                }

                var entryOperationInfo = new C2VAddressablesEntryOperationInfo(createEntryOperationInfo, null, null);

                entryOperationDictionary.Add(createEntryOperationInfo.Address, entryOperationInfo);
            }
        }


        public void ProcessDeletedAsset(string assetPath, C2VEntryOperationDictionary entryOperationDictionary)
        {
            foreach (var rule in _groupBuilderStorageService.GroupRuleEntries)
            {
                var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, assetPath);

                if (string.IsNullOrEmpty(addressablePath))
                {
                    continue;
                }

                var removeEntryOperationInfo = _entryGroupBuildService.BuildRemoveEntryOperationInfoByRule(assetPath, rule);

                if (removeEntryOperationInfo == null)
                {
                    continue;
                }

                if (entryOperationDictionary.ContainsKey(assetPath))
                {
                    continue;
                }

                var entryOperationInfo = new C2VAddressablesEntryOperationInfo(null, removeEntryOperationInfo, null);

                entryOperationDictionary.Add(assetPath, entryOperationInfo);
            }
        }


        public void ProcessMovedAsset(string toAssetPath, string fromAssetPath, C2VEntryOperationDictionary entryOperationDictionary)
        {
            foreach (var rule in _groupBuilderStorageService.GroupRuleEntries)
            {
                var toMovedAssetPath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, toAssetPath);
                var fromMovedAssetPath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, fromAssetPath);

                if (!string.IsNullOrEmpty(toMovedAssetPath) && !string.IsNullOrEmpty(fromMovedAssetPath))
                {
                    // Move
                    if (entryOperationDictionary.ContainsKey(toAssetPath))
                    {
                        continue;
                    }

                    var removeEntryOperationInfo = _entryGroupBuildService.BuildRemoveEntryOperationInfo(toAssetPath, null);
                    var createEntryOperationInfo = _entryGroupBuildService.BuildCreateEntryOperationInfoByRule(toAssetPath, rule);

                    var entryOperationInfo = new C2VAddressablesEntryOperationInfo(createEntryOperationInfo, removeEntryOperationInfo, null);

                    entryOperationDictionary.Add(toAssetPath, entryOperationInfo);
                }

                if (!string.IsNullOrEmpty(toMovedAssetPath) && string.IsNullOrEmpty(fromMovedAssetPath))
                {
                    var createEntryOperationInfo = _entryGroupBuildService.BuildCreateEntryOperationInfoByRule(toAssetPath, rule);

                    // Create
                    if (entryOperationDictionary.TryGetValue(toAssetPath, out var entryOperationInfo))
                    {
                        entryOperationInfo.EntryCreateInfo = createEntryOperationInfo;
                    }
                    else
                    {
                        entryOperationInfo = new C2VAddressablesEntryOperationInfo(createEntryOperationInfo, null, null);

                        entryOperationDictionary.Add(toAssetPath, entryOperationInfo);
                    }
                }

                if (string.IsNullOrEmpty(toMovedAssetPath) && !string.IsNullOrEmpty(fromMovedAssetPath))
                {
                    var removeEntryOperationInfo = _entryGroupBuildService.BuildRemoveEntryOperationInfo(toAssetPath, null);

                    // Remove
                    if (entryOperationDictionary.TryGetValue(toAssetPath, out var entryOperationInfo))
                    {
                        entryOperationInfo.EntryRemoveInfo = removeEntryOperationInfo;
                    }
                    else
                    {
                        entryOperationInfo = new C2VAddressablesEntryOperationInfo(null, removeEntryOperationInfo, null);

                        entryOperationDictionary.Add(toAssetPath, entryOperationInfo);
                    }
                }
            }
        }


        private void ProcessRemoveGroup(string assetPath, C2VEntryOperationDictionary entryOperationDictionary = null)
        {
            entryOperationDictionary ??= new C2VEntryOperationDictionary();

            foreach (var rule in _groupBuilderStorageService.GroupRuleEntries)
            {
                var addressablePath = _addressablePathGenerator.GenerateFromAssetPath(rule.GetProjectRootPath(), rule.RootPath, assetPath);

                if (string.IsNullOrEmpty(addressablePath))
                {
                    continue;
                }

                var removeGroupOperationInfo = _entryGroupBuildService.BuildRemoveGroupOperationInfoByRule(assetPath, rule);

                if (removeGroupOperationInfo == null)
                {
                    continue;
                }

                if (entryOperationDictionary.ContainsKey(assetPath))
                {
                    continue;
                }

                var entryOperationInfo = new C2VAddressablesEntryOperationInfo(null, null, removeGroupOperationInfo);

                entryOperationDictionary.Add(assetPath, entryOperationInfo);
            }
        }


        public void ProcessApply(C2VEntryOperationDictionary entryOperationDictionary)
        {
            if (entryOperationDictionary == null || entryOperationDictionary.Count == 0)
            {
                return;
            }

            if (!AreEntryOperationInfosValid(entryOperationDictionary))
            {
                entryOperationDictionary.Clear();
                return;
            }

            var progressStatus = new C2VAddressablesEditorDisplayProgress();

            foreach (var kvp in entryOperationDictionary)
            {
                var path = kvp.Key;
                var entryOperationInfo = kvp.Value;

                _entryOperationInfoService.Apply(entryOperationInfo);

                progressStatus.TotalCount = entryOperationDictionary.Count;
                progressStatus.Name = path;
                progressStatus.Current++;

                progressStatus.DisplayProgressBar();
            }

            _assetDataBaseAdapter.SaveAssets();

            entryOperationDictionary.Clear();
        }


        private bool AreEntryOperationInfosValid(C2VEntryOperationDictionary entryOperationDictionary)
        {
            bool processed = false;

            foreach (var kvp in entryOperationDictionary)
            {
                var entryOperationInfo = kvp.Value;

                processed |= _entryOperationInfoService.IsValid(entryOperationInfo);
            }

            return processed;
        }


        private void ExecutePreProcess(eEnvironment environment)
        {
            foreach (var service in _servicePack.GetServices())
            {
                if (service is IPreProcessor preProcessor)
                    preProcessor.Execute(environment);
            }
        }


        private void ExecutePostProcess(eEnvironment environment)
        {
            foreach (var service in _servicePack.GetServices())
            {
                if (service is IPostProcessor postProcessor)
                    postProcessor.Execute(environment);
            }
        }


        public void Release()
        {
            _servicePack = null;
            _assetDataBaseAdapter = null;
            _addressablePathGenerator = null;
            _entryGroupBuildService = null;
            _entryOperationInfoService = null;
            _groupBuilderStorageService = null;
        }
    }
}
