/*===============================================================
* Product:		Com2Verse
* File Name:	MiceMVMBase.cs
* Developer:	sprite
* Date:			2023-04-18 12:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Com2Verse.UI;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
    public partial class MiceViewModel : RecyclableCellViewModel
    {
        /// <summary>
        /// 같은 값이면 갱신하지 않는다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected new void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
            where T : unmanaged, IConvertible
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return;

            base.SetProperty(ref storage, value, propertyName);
        }

        /// <summary>
        /// 같은 값이면 갱신하지 않는다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected new void SetProperty<T>(ref T storage, object value, [CallerMemberName] string propertyName = "")
        {
            if ((object)storage == value) return;

            base.SetProperty(ref storage, value, propertyName);
        }

        protected ViewModelContainer GetViewModelContainer()
        {
            var field = typeof(ViewModel).GetField("_container", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field == null) return null;

            return (ViewModelContainer)field.GetValue(this);
        }

        public static async UniTask<GUIView> ShowView(string asset, Action<GUIView> prepareViewModel = null, Action<GUIView> onShow = null, Action<GUIView> onHide = null, Action<GUIView> onOpening = null)
        {
            GUIView view = await asset.AsGUIView();

            void OnOpeningEvent(GUIView view)
            {
                onOpening?.Invoke(view);
            }

            void OnOpenedEvent(GUIView view)
            {
                onShow?.Invoke(view);
            }

            void OnClosedEvent(GUIView view)
            {
                onHide?.Invoke(view);

                view.OnOpeningEvent -= OnOpeningEvent;
                view.OnOpenedEvent -= OnOpenedEvent;
                view.OnClosedEvent -= OnClosedEvent;
            }

            view.OnOpeningEvent += OnOpeningEvent;
            view.OnOpenedEvent += OnOpenedEvent;
            view.OnClosedEvent += OnClosedEvent;

            prepareViewModel?.Invoke(view);

            view.Show();

            return view;
        }

        public static UniTask<GUIView> ShowView<TEnum>(TEnum asset, Action<GUIView> prepareViewModel = null, Action<GUIView> onShow = null, Action<GUIView> onHide = null, Action<GUIView> onOpening = null)
            where TEnum : unmanaged, Enum
            => MiceViewModel.ShowView(asset.ToString(), prepareViewModel, onShow, onHide, onOpening);
    }

    /// <summary>
    /// MiceViewModelInfo용 ViewModel
    /// </summary>
    /// <typeparam name="TMiceViewModel"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public partial class MiceViewModelForInfo<TMiceViewModel, TModel> : MiceViewModel, INestedViewModel
    {
        IList<ViewModel> INestedViewModel.NestedViewModels => _nestedViewModels;

        protected TModel _data;
        private List<ViewModel> _nestedViewModels;

        public MiceViewModelForInfo(TModel data, bool isUnique = false, params ViewModel[] nestedViewModels)
        {
            _data = data;
            _nestedViewModels = nestedViewModels?.ToList() ?? new List<ViewModel>(1);

            if (isUnique)
            {
                MasterViewModel.TryAdd(typeof(TMiceViewModel), this);
            }
        }

        public ViewModel RegisterNestedViewModel(ViewModel nestedViewModel)
        {
            _nestedViewModels.Add(nestedViewModel);

            return nestedViewModel;
        }

        public void RemoveNestedViewModel(ViewModel nestedViewModel)
        {
            _nestedViewModels.Remove(nestedViewModel);
        }
    }

    public partial class MiceViewModelForInfo<TMiceViewModel, TModel>   // Nested ViewModel Generator
    {
        static readonly int DEFAULT_CAPACITY = 5;

        private Dictionary<Type, Func<MiceBaseInfo, ViewModel>[]> _nestedViewModelGeneratorMap;

        public virtual void RegisterNestedViewModels<TInfo>(params Func<TInfo, ViewModel>[] nestedViewModelGenerators)
            where TInfo : MiceBaseInfo
        {
            if (_nestedViewModelGeneratorMap == null) _nestedViewModelGeneratorMap = new Dictionary<Type, Func<MiceBaseInfo, ViewModel>[]>(DEFAULT_CAPACITY);

            var generators = nestedViewModelGenerators.Select(e => (Func<MiceBaseInfo, ViewModel>)(info => e.Invoke((TInfo)info))).ToArray();

            if (!_nestedViewModelGeneratorMap.ContainsKey(typeof(TInfo)))
            {
                _nestedViewModelGeneratorMap.Add(typeof(TInfo), generators);
            }
            else
            {
                _nestedViewModelGeneratorMap[typeof(TInfo)] = generators;
            }
        }

        public ViewModel[] GenerateNestedViewModels<TInfo>(TInfo info)
            where TInfo : MiceBaseInfo
        {
            if
            (
                _nestedViewModelGeneratorMap == null ||
                !_nestedViewModelGeneratorMap.TryGetValue(typeof(TInfo), out var generators) ||
                generators == null ||
                generators.Length == 0
            )
            {
                return null;
            }

            return generators.Where(e => e != null).Select(e => e(info)).ToArray();
        }

        public virtual void ClearNestedViewModels<TInfo>()
            where TInfo : MiceBaseInfo
        {
            _nestedViewModelGeneratorMap?.Remove(typeof(TInfo));
        }
    }
}
