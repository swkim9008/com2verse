/*===============================================================
* Product:    Com2Verse
* File Name:  ViewModelContainer.cs
* Developer:  tlghks1009
* Date:       2022-04-14 17:35
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;

namespace Com2Verse.UI
{
    public class ViewModelContainer
    {
        private readonly Dictionary<Type, ViewModel> _viewModelDictionary;

        public ViewModelContainer() => _viewModelDictionary = new Dictionary<Type, ViewModel>();

        public void CreateInstanceOfViewModel(Binder binder, bool allowDuplicate = false)
        {
            var viewModelType = binder.SourceOwnerType;

            if (viewModelType == null)
            {
                return;
            }

            if (_viewModelDictionary.ContainsKey(viewModelType))
            {
                return;
            }

            var viewModel = MasterViewModel.Get(viewModelType);
            if (viewModel == null || allowDuplicate)
            {
                try
                {
                    viewModel = (ViewModel)Activator.CreateInstance(viewModelType);
                }
                catch (Exception e)
                {
                    var binderPath = binder.IsUnityNull() ? "unknown" : binder.transform.GetFullPathInHierachy();
                    C2VDebug.LogErrorMethod(nameof(ViewModelContainer), $"{e.GetType()?.Name} occur while creating {viewModelType.Name} instance. / Path: {binderPath}");
                    C2VDebug.LogErrorMethod(nameof(ViewModelContainer), $"{e.Message}\n{e.StackTrace}");
                    return;
                }

                if (!allowDuplicate)
                {
                    if (!MasterViewModel.TryAdd(viewModelType, viewModel))
                    {
                        C2VDebug.LogErrorMethod(nameof(ViewModelContainer), $"{viewModelType.Name} is already added to MasterViewModel.");
                    }
                }
            }

            viewModel.OnInitialize();

            AddViewModel(viewModel);
        }


        public void AddViewModel(ViewModel viewModel, bool isNesting = false)
        {
            if (_viewModelDictionary.ContainsKey(viewModel.GetType()))
            {
                // 중첩 ViewModel 재귀 순환 중에는 부모에 동일한 ViewModel이 존재해도 문제가 없으므로 로그를 출력하지 않습니다.
                if (!isNesting) C2VDebug.LogErrorMethod(nameof(ViewModelContainer), $"{viewModel.GetType().Name} is already added to ViewModelContainer.");
                return;
            }

            viewModel.SetContainer(this);

            _viewModelDictionary.Add(viewModel.GetType(), viewModel);

            AddNestedViewModelsToContainer(viewModel);
        }


        public bool TryGetViewModel(Type viewModelType, out ViewModel viewModel)
        {
            return _viewModelDictionary.TryGetValue(viewModelType!, out viewModel);
        }


        public bool TryGetViewModel<T>(out T viewModel) where T : ViewModel
        {
            if (!TryGetViewModel(typeof(T), out var result))
            {
                viewModel = null;
                return false;
            }
            viewModel = result as T;
            return true;
        }

        
        public T GetViewModel<T>() where T : ViewModel
        {
            if (!TryGetViewModel(typeof(T), out var viewModel))
            {
                return null;
            }

            return viewModel as T;
        }

        public T GetOrAddViewModel<T>() where T : ViewModel, new()
        {
            var result = GetViewModel<T>();
            if (result != null)
            {
                return result;
            }

            var viewModel = new T();
            AddViewModel(viewModel);

            return viewModel;
        }


        public void InitializeViewModel()
        {
            foreach (var viewModel in _viewModelDictionary.Values)
            {
                viewModel.OnInitialize();
            }
        }


        public void Remove(ViewModel viewModel, bool isDispose = true)
        {
            RemoveNestedViewModelsFromContainer(viewModel);

            if (viewModel.DontDestroyOnLoad)
                return;

            viewModel.OnRelease();

            if (isDispose)
                (viewModel as IDisposable)?.Dispose();

            _viewModelDictionary.Remove(viewModel.GetType());
        }

        public void ClearAll()
        {
            var viewModelDictionary = new Dictionary<Type, ViewModel>(_viewModelDictionary!);

            // 중첩 ViewModel 바인딩을 먼저 해제한다 (순서 상관 없이 해제 시 자식 ViewModel이 먼저 해제되는 것을 방지하기 위함)
            foreach (var viewModel in viewModelDictionary.Values)
                RemoveNestedViewModelsFromContainer(viewModel);

            foreach (var viewModel in viewModelDictionary.Values)
            {
                if (!_viewModelDictionary.ContainsKey(viewModel.GetType()))
                    continue;

                Remove(viewModel);
            }
        }

#region NestedViewModel
        /// <summary>
        /// ViewModel이 <see cref="INestedViewModel"/>을 구현한 경우, ViewModelContainer에 자식 ViewModel을 재귀적으로 추가합니다.
        /// </summary>
        /// <exception cref="StackOverflowException">
        /// ViewModel 중첩 구조에 순환이 존재하는 경우 발생합니다.
        /// </exception>
        private void AddNestedViewModelsToContainer(ViewModel viewModel)
        {
            if (viewModel is not INestedViewModel nestedViewModel)
                return;

            AddNestedViewModelsToContainerRecursively(nestedViewModel);
        }

        private void AddNestedViewModelsToContainerRecursively(INestedViewModel nestedViewModel)
        {
            foreach (var viewModel in nestedViewModel.NestedViewModels)
            {
                AddViewModel(viewModel, true);

                if (viewModel is INestedViewModel recursivelyNestedViewModel)
                    AddNestedViewModelsToContainerRecursively(recursivelyNestedViewModel);
            }
        }

        /// <summary>
        /// ViewModel이 <see cref="INestedViewModel"/>을 구현한 경우, ViewModelContainer에서 자식 ViewModel을 재귀적으로 제거합니다.
        /// </summary>
        /// <exception cref="StackOverflowException">
        /// ViewModel 중첩 구조에 순환이 존재하는 경우 발생합니다.
        /// </exception>
        private void RemoveNestedViewModelsFromContainer(ViewModel viewModel)
        {
            if (viewModel is not INestedViewModel nestedViewModel)
                return;

            RemoveNestedViewModelsFromContainerRecursively(nestedViewModel);
        }

        private void RemoveNestedViewModelsFromContainerRecursively(INestedViewModel nestedViewModel)
        {
            foreach (var viewModel in nestedViewModel.NestedViewModels)
            {
                // 중첩 ViewModel의 자식 ViewModel의 생명 주기 관리는 직접 하는 것이 타당하기 때문에, 여기서는 바인딩 제거만 한다.
                Remove(viewModel, false);

                if (viewModel is INestedViewModel recursivelyNestedViewModel)
                    RemoveNestedViewModelsFromContainerRecursively(recursivelyNestedViewModel);
            }
        }
#endregion // NestedViewModel
    }
}
