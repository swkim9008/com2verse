/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGroupBuilderServicePack.cs
* Developer:	tlghks1009
* Date:			2023-03-31 14:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;

namespace Com2VerseEditor.AssetSystem
{
    public interface IServicePack
    {
        public void AddService(C2VAddressablesGroupBuilderServiceBase service);

        public void RemoveService(C2VAddressablesGroupBuilderServiceBase service);

        public T GetService<T>() where T : C2VAddressablesGroupBuilderServiceBase;

        public IReadOnlyList<C2VAddressablesGroupBuilderServiceBase> GetServices();

        public void ClearService();

        public C2VAddressablesGroupBuilderAdapterPack GetAdapterPack();

        public void InitializeServices();
    }

    public interface IPreProcessor
    {
        void Execute(eEnvironment environment);
    }

    public interface IPostProcessor
    {
        void Execute(eEnvironment environment);
    }

    public interface IEnvironmentSetting
    {
        void ApplyTo(eEnvironment environment);
    }


    public class C2VAddressablesGroupBuilderServicePack : IServicePack
    {
        private readonly List<C2VAddressablesGroupBuilderServiceBase> _services;

        private C2VAddressablesGroupBuilderAdapterPack _adapterPack;


        public C2VAddressablesGroupBuilderServicePack()
        {
            _services = new List<C2VAddressablesGroupBuilderServiceBase>();

            _adapterPack = new C2VAddressablesGroupBuilderAdapterPack();

            AddService(new C2VAddressablesEntryOperationInfoService(this));
            AddService(new C2VAddressablesEntryGroupBuildService(this));
            AddService(new C2VAssetBundleCacheBuildService(this));
            AddService(new C2VAddressablesGroupBuilderStorageService(this));
            AddService(new C2VAddressablesSettingService(this));
            AddService(new C2VAddressablesGroupSettingService(this));
            AddService(new C2VAddressablesBuildService(this));
            AddService(new C2VAddressablesEditorHostingService(this));

            InitializeServices();
        }


        public void Release()
        {
            _adapterPack.Release();

            _adapterPack = null;
        }

        public void AddService(C2VAddressablesGroupBuilderServiceBase service)
        {
            _services.Add(service);
        }

        public void RemoveService(C2VAddressablesGroupBuilderServiceBase service)
        {
            if (_services.Contains(service))
            {
                _services.Remove(service);
            }
        }

        public T GetService<T>() where T : C2VAddressablesGroupBuilderServiceBase
        {
            foreach (var service in _services)
            {
                if (service is T instance)
                {
                    return instance;
                }
            }

            return null;
        }

        public C2VAddressablesGroupBuilderAdapterPack GetAdapterPack() => _adapterPack;

        public IReadOnlyList<C2VAddressablesGroupBuilderServiceBase> GetServices() => _services;

        public void ClearService()
        {
            foreach (var service in _services)
            {
                service.Release();
            }

            _services.Clear();
        }

        public void InitializeServices()
        {
            foreach (var service in _services)
            {
                service.Initialize();
            }
        }
    }
}
