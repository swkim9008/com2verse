/*===============================================================
* Product:		Com2Verse
* File Name:	ViewModelManager.cs
* Developer:	tlghks1009
* Date:			2023-01-04 12:54
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
    public sealed class ViewModelManager : Singleton<ViewModelManager>, IDisposable
    {
        [UsedImplicitly] private ViewModelManager() { }

        private Action _onUpdateHandler;
        public event Action OnUpdateHandler
        {
            add
            {
                _onUpdateHandler -= value;
                _onUpdateHandler += value;
            }
            remove => _onUpdateHandler -= value;
        }

        private Action _onClearedHandler;
        public event Action OnClearedHandler
        {
            add
            {
                _onClearedHandler -= value;
                _onClearedHandler += value;
            }
            remove => _onClearedHandler -= value;
        }

        public void Initialize()
        {
            ViewModelTypeHolder.Initialize();

            SceneManager.Instance.BeforeSceneChanged += OnBeforeSceneChanged;
        }

        public void OnUpdate()
        {
            _onUpdateHandler?.Invoke();
        }


        public bool TryAdd(Type viewModelType, ViewModel viewModel) => MasterViewModel.TryAdd(viewModelType, viewModel);

        public ViewModel Get(Type viewModelType) => MasterViewModel.Get(viewModelType);


        public T Get<T>() where T : ViewModel => Get(typeof(T)) as T;


        [NotNull]
        public T GetOrAdd<T>() where T : ViewModel, new() => MasterViewModel.GetOrAdd<T>();


        public void Remove(ViewModel viewModel) => MasterViewModel.Remove(viewModel);


        public void ClearAll()
        {
            MasterViewModel.ClearAll();
            _onClearedHandler?.Invoke();
        }


        public void Dispose() => MasterViewModel.Dispose();


        private void OnBeforeSceneChanged(SceneBase currentScene, SceneBase newScene)
        {
            ClearAll();
        }
    }
}
