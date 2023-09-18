/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGroupRuleEditorController.cs
* Developer:	tlghks1009
* Date:			2023-03-03 11:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2VerseEditor.AssetSystem
{
    public sealed class C2VAddressablesGroupBuilderWindowController
    {
        private IServicePack _servicePack;

        private C2VAddressablesGroupBuilderController _groupBuilderController;

        private C2VAddressablesGroupRuleTreeView _groupRuleTreeView;

        private C2VAddressablesGroupBuilderStorageService _groupBuilderStorageService;

        private C2VAddressablesBuildService _buildService;


        public C2VAddressablesGroupBuilderWindowController(C2VAddressablesGroupBuilderWindow     window,
                                                           IServicePack                          servicePack,
                                                           C2VAddressablesGroupBuilderController groupBuilderController)
        {
            _servicePack = servicePack;

            _groupBuilderController = groupBuilderController;

            _groupBuilderStorageService = servicePack.GetService<C2VAddressablesGroupBuilderStorageService>();

            _buildService = _servicePack.GetService<C2VAddressablesBuildService>();

            if (window != null)
            {
                _groupRuleTreeView = window.GroupRuleTreeView;

                window.OnCreateMenuClick.AddListener(OnCreateButtonClicked);
                window.OnSaveButtonClick.AddListener(OnSaveButtonClicked);
                window.OnRemoveButtonClick.AddListener(OnRemoveButtonClicked);
                window.OnCreateGroupButtonClick.AddListener(OnCreateGroupButtonClicked);
                window.OnCleanCreateGroupButtonClick.AddListener(OnCleanCreateGroupButtonClicked);
                window.OnAutoPackagingButtonClick.AddListener(OnAutoPackagingButtonClicked);
                window.OnEnvironmentSettingButtonClick.AddListener(OnEnvironmentSettingButtonClicked);
                window.OnBuildButtonClick.AddListener(OnBuildButtonClicked);
                window.OnCleanBuildButtonClick.AddListener(OnCleanBuildButtonClicked);
                window.OnBuildAndUploadButtonClick.AddListener(OnBuildAndUploadButtonClicked);

                AddGroupRulesItemFromAssetDatabase();
            }
        }


        private void OnBuildAndUploadButtonClicked(eEnvironment environment) => _buildService.BuildAndUpload(environment);

        private void OnCleanBuildButtonClicked(eEnvironment environment) => _buildService.CleanBuild(environment);

        private void OnBuildButtonClicked(eEnvironment environment) => _buildService.Build(environment);


        private void OnEnvironmentSettingButtonClicked(eEnvironment environment)
        {
            _groupBuilderStorageService.AddressableGroupRuleData.Environment = environment;
            _groupBuilderStorageService.SaveOption();

            foreach (var service in _servicePack.GetServices())
            {
                if (service is IEnvironmentSetting environmentSetting)
                    environmentSetting.ApplyTo(environment);
            }
        }


        private void AddGroupRulesItemFromAssetDatabase()
        {
            foreach (var groupRuleItem in _groupBuilderStorageService.GroupRuleEntries)
            {
                AddGroupRuleItem(groupRuleItem);
            }
        }


        private void OnCreateButtonClicked() => AddGroupRuleItem(new C2VAddressableGroupRuleEntry());

        private void OnSaveButtonClicked() => _groupBuilderStorageService.Save(_groupRuleTreeView.Items());


        private void OnCreateGroupButtonClicked()
        {
            var storageService = _servicePack.GetService<C2VAddressablesGroupBuilderStorageService>();
            var environment    = storageService.AddressableGroupRuleData.Environment;

            _groupBuilderController.CreateGroup(environment);
        }


        private void OnCleanCreateGroupButtonClicked()
        {
            var storageService = _servicePack.GetService<C2VAddressablesGroupBuilderStorageService>();
            var environment    = storageService.AddressableGroupRuleData.Environment;

            _groupBuilderController.RemoveAllGroup();
            _groupBuilderController.CreateGroup(environment);
        }


        private void OnAutoPackagingButtonClicked()
        {
            var originalValue = _groupBuilderStorageService.AddressableGroupRuleData.AutoPackaging;
            _groupBuilderStorageService.AddressableGroupRuleData.AutoPackaging = !originalValue;

            _groupBuilderStorageService.SaveOption();
        }


        private void OnRemoveButtonClicked()
        {
            if (!_groupRuleTreeView.HasSelection())
            {
                return;
            }

            foreach (var id in _groupRuleTreeView.GetSelection())
            {
                _groupRuleTreeView.RemoveItem(id);
            }
        }


        private void AddGroupRuleItem(C2VAddressableGroupRuleEntry groupRule)
        {
            var item = new C2VAddressableGroupRuleTreeItem();
            item.id        = item.GetHashCode();
            item.GroupRule = groupRule;

            _groupRuleTreeView.AddItem(item);
        }


        public void Release()
        {
            _buildService               = null;
            _servicePack                = null;
            _groupBuilderController     = null;
            _groupBuilderStorageService = null;
            _groupRuleTreeView          = null;
        }
    }
}
