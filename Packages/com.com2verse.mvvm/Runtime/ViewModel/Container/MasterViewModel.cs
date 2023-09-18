/*===============================================================
* Product:    Com2Verse
* File Name:  ViewModelManager.cs
* Developer:  tlghks1009
* Date:       2022-04-14 17:35
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;

namespace Com2Verse.UI
{
    public static class MasterViewModel
    {
        private static readonly Dictionary<Type, ViewModel> _viewModelDictionary = new();

        public static IReadOnlyDictionary<Type, ViewModel> ViewModelDict => _viewModelDictionary;

        public static bool TryAdd(Type viewModelType, ViewModel viewModel) => _viewModelDictionary.TryAdd(viewModelType, viewModel);

        public static ViewModel Get(Type viewModelType) => !_viewModelDictionary.TryGetValue(viewModelType!, out var viewModel) ? null : viewModel;

        public static T Get<T>() where T : ViewModel => Get(typeof(T)) as T;


        public static T GetOrAdd<T>() where T : ViewModel, new()
        {
            var result = Get<T>();
            if (result != null)
            {
                return result;
            }

            var viewModel = new T();
            TryAdd(typeof(T), viewModel);

            return viewModel;
        }

        public static void Remove(ViewModel viewModel)
        {
            var viewModelType = viewModel.GetType();

            if (_viewModelDictionary.ContainsKey(viewModelType))
            {
                _viewModelDictionary.Remove(viewModelType);
            }
        }


        public static void ClearAll()
        {
            var viewModelDictionary = new Dictionary<Type, ViewModel>(_viewModelDictionary!);

            foreach (var viewModel in viewModelDictionary.Values)
            {
                if (viewModel.DontDestroyOnLoad)
                {
                    continue;
                }

                Remove(viewModel);
            }
        }

        public static void Dispose() => _viewModelDictionary.Clear();
    }
}
