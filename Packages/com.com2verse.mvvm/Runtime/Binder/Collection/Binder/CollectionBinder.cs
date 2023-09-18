/*===============================================================
* Product:    Com2Verse
* File Name:  CollectionBinder.cs
* Developer:  tlghks1009
* Date:       2022-03-04 14:12
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace Com2Verse.UI
{
    public class CollectionItem : IBindingContainer
    {
        public  int Index { get; set; }

        public  ViewModelContainer ViewModelContainer { get; } = new();
        public  Transform          GetTransform()     => null;

        public ViewModel ViewModel { get; set; }

        private GameObject _collectionItemObject;
        public GameObject CollectionItemObject => _collectionItemObject;


        private IBindingContainer _bindingContainer;
        private List<Binder> _binderList = new();

        public CollectionItem(GameObject itemObject)
        {
            _collectionItemObject = itemObject;
            Index = -1;
        }
        public void Bind()
        {
            if (!_collectionItemObject)
                return;

            _bindingContainer = _collectionItemObject.GetComponent<IBindingContainer>();

            if (_bindingContainer != null)
            {
                _bindingContainer.ViewModelContainer.ClearAll();
                _bindingContainer.ViewModelContainer.AddViewModel(ViewModel);
                _bindingContainer.Unbind();
                _bindingContainer.Bind();
            }
            else
            {
                ViewModelContainer.ClearAll();
                ViewModelContainer.AddViewModel(ViewModel);
                ViewModelContainer.InitializeViewModel();

                if (_binderList.Count == 0)
                    FindBinderList(true);

                foreach (var binder in _binderList)
                {
                    RedDotBinder redDotBinder = binder as RedDotBinder;
                    if (redDotBinder.IsReferenceNull() == false)
                    {
                        var redDotViewModel = new RedDotViewModel(new RedDotData(redDotBinder._badgeType, Index));
                        RedDotManager.Instance.CreateCollectionItem(redDotBinder.gameObject, redDotViewModel);
                    }
                    else
                    {
                        binder.SetViewModelContainer(ViewModelContainer);
                        binder.Bind();
                    }
                }
            }
        }

        public void BindStaticRedDot()
        {
            if (!_collectionItemObject)
                return;

            _bindingContainer = _collectionItemObject.GetComponent<IBindingContainer>();

            if (_bindingContainer != null)
            {
                _bindingContainer.ViewModelContainer.AddViewModel(ViewModel);
                _bindingContainer.Unbind();
                _bindingContainer.Bind();
            }
            else
            {
                ViewModelContainer.AddViewModel(ViewModel, true);
                ViewModelContainer.InitializeViewModel();

                if (_binderList.Count == 0)
                {
                    FindBinderList(false);
                }

                foreach (var binder in _binderList)
                {
                    binder.SetViewModelContainer(ViewModelContainer);
                    binder.Bind();
                }
            }
        }

        public void Unbind()
        {
            if (_bindingContainer != null)
            {
                _bindingContainer.ViewModelContainer.ClearAll();
                _bindingContainer.Unbind();
            }
            else
            {
                if (_binderList != null)
                {
                    foreach (var binder in _binderList)
                    {
                        binder.Unbind();
                    }
                }

                ViewModelContainer.ClearAll();
            }
        }

        public void Reset()
        {
            ViewModelContainer.ClearAll();
            ViewModel = null;

            _binderList.Clear();
            _binderList           = null;
            _collectionItemObject = null;
            _bindingContainer     = null;
            Index                 = -1;
        }


        private void FindBinderList(bool includeRedDotChild)
        {
            var bindingContainers = _collectionItemObject.GetComponentsInChildren<IBindingContainer>(true);
            var bindersOfChildren = _collectionItemObject.GetComponentsInChildren<Binder>(true);

            var skipBinderList = new List<Binder>();
            if (bindingContainers.Length != 0)
            {
                foreach (var bindingContainer in bindingContainers)
                {
                    if (bindingContainer == (IBindingContainer)this)
                        continue;

                    if (bindingContainer is IViewModelContainerBridge bridge)
                        bridge.SetViewModelContainer(ViewModelContainer);
                        

                    var childBinderListOfBindingContainer = bindingContainer.GetTransform().GetComponentsInChildren<Binder>(true);
                    foreach (var binder in childBinderListOfBindingContainer)
                    {
                        if (bindingContainer.GetTransform() == binder.transform)
                        {
                            continue;
                        }

                        skipBinderList.Add(binder);
                    }
                }
            }

            // 레드닷 차일드들은 제거한다
            if(includeRedDotChild)
                GetRedDotBinderChild(bindersOfChildren, ref skipBinderList);

            foreach (var binder in bindersOfChildren)
            {
                if (skipBinderList.Contains(binder))
                {
                    continue;
                }

                _binderList.Add(binder);
            }
        }

        private void GetRedDotBinderChild(Binder[] bindersOfChildren, ref List<Binder> skipBinderList)
        {
            // 레드닷 차일드들은 제거한다
            foreach (var binder in bindersOfChildren)
            {
                RedDotBinder redDotBinder = binder as RedDotBinder;
                if (redDotBinder.IsReferenceNull() == false)
                {
                    var childBinderListOfBindingContainer = redDotBinder.GetComponentsInChildren<Binder>(true);
                    foreach (var redDotbinderChild in childBinderListOfBindingContainer)
                    {
                        if (redDotbinderChild is RedDotBinder)
                            continue;

                        skipBinderList.Add(redDotbinderChild);
                    }
                }
            }
        }
    }


    [AddComponentMenu("[DB]/[DB] Collection Binder")]
    [RequireComponent(typeof(DataBinder))]
    public class CollectionBinder : Binder
    {
        public event Action<CollectionItem> ItemActivated;
        public event Action<CollectionItem> ItemDeactivated;

        public List<CollectionItem> ActivatedItems { get; } = new();

        private readonly List<CollectionItem> _clearingItemList = new();
        private readonly Dictionary<ViewModel, bool> _viewModelChecker = new();

        private ObjectPool<CollectionItem> _itemPool;

        [SerializeField] private int _poolCount;
        [SerializeField] private int _maxPoolCount = int.MaxValue;
        [SerializeField] private AssetReference _prefab;

        [Header("Optional")]
        [Tooltip("생성되는 프리팹의 순서 정렬이 필요할 경우 체크. (단, ItemRoot와 PoolRoot가 동일한 Transform인 경우에만 정렬.)")]
        [SerializeField] private bool _isItemOrdered = true;

        [Tooltip("IsItemOrdered 값이 True일 때만 동작.")] [DrawIf(nameof(_isItemOrdered), true)]
        [SerializeField] private bool _isAscendingOrder = true;

        [Tooltip("비활성화된 프리팹을 담아두는 Transform을 별도로 지정할 경우 설정.")]
        [SerializeField] private Transform _poolRoot;

        [Tooltip("프리팹이 생성되는 Transform을 별도로 지정할 경우 설정.")]
        [SerializeField] private Transform _itemRoot;

        [SerializeField] private UnityEvent<bool> _onResourceLoadComplete;

        private GameObject _loadedAsset;
        private bool _isInitialized;

        protected Transform ItemRoot => !_itemRoot ? this.transform : _itemRoot;
        protected Transform PoolRoot => !_poolRoot ? this.transform : _poolRoot;


        private CancellationTokenSource _cancellationTokenSource;


        private INotifyCollectionChanged _collection;

        public INotifyCollectionChanged Collection
        {
            get => _collection;
            set
            {
                if (value == null)
                {
                    C2VDebug.LogError($"[CollectionBinder] Can't find collection. ObjectName : {this.gameObject.name}");
                    return;
                }

                _isInitialized = false;
                _cancellationTokenSource?.Cancel();

                _collection = value;
                _collection.AddEvent(OnCollectionItemChanged);

                OnInitializeAfterAssetLoad().Forget();
            }
        }

        private async UniTask OnInitializeAfterAssetLoad()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            //_loadedAsset = await C2VAddressables.LoadAssetAsync<GameObject>(_prefab).ToUniTask();
            _loadedAsset = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(_prefab);
            
            if (_loadedAsset == null)
            {
                return;
            }

            if (_cancellationTokenSource == null)
            {
                _loadedAsset = null;
                return;
            }

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _loadedAsset = null;
                return;
            }

            _onResourceLoadComplete?.Invoke(true);

            Initialize();
        }


        public override void Bind() { }

        public override void Unbind()
        {
            base.Unbind();

            Reset();
        }

        private void Initialize()
        {
            SetupObjectPool();

            RunBindAll();

            _isInitialized = true;

            OnCompleteCreate?.Invoke();
        }

        private void SetupObjectPool()
        {
            _itemPool ??= new ObjectPool<CollectionItem>(
                createFunc: () =>
                {
                    if (_loadedAsset == null)
                        throw new ArgumentNullException("Asset Null.", $"BindingException : {MVVMUtil.GetFullPathInHierarchy(this.transform)}");

                    var itemObject = Instantiate(_loadedAsset, PoolRoot);
                    if (itemObject != null) itemObject.SetActive(false);
                    return new CollectionItem(itemObject);//, this);
                }
              , actionOnGet: item =>
                {
                    ActivatedItems?.Add(item);
                    ItemActivated?.Invoke(item);

                    var itemObject = item?.CollectionItemObject;
                    if (itemObject != null)
                    {
                        itemObject.SetActive(true);
                        itemObject.transform.SetParent(ItemRoot);

                        // 아이템 생성 위치가 SetParent로 변경되지 않는 경우, 리스트 순서 보장을 위해 맨 뒤로 이동
                        if (_isItemOrdered && ItemRoot == PoolRoot)
                        {
                            if (_isAscendingOrder)
                                itemObject.transform.SetAsLastSibling();
                            else
                                itemObject.transform.SetAsFirstSibling();
                        }
                    }
                }
              , actionOnRelease: item =>
                {
                    ActivatedItems?.Remove(item);
                    ItemDeactivated?.Invoke(item);

                    if (item.ViewModel != null)
                        _viewModelChecker.Remove(item.ViewModel);

                    var itemObject = item?.CollectionItemObject;
                    if (itemObject != null)
                    {
                        itemObject.SetActive(false);
                        itemObject.transform.SetParent(PoolRoot);
                    }
                }
              , actionOnDestroy: static item =>
                {
                    item?.Reset();
                    Destroy(item?.CollectionItemObject);
                }
              , collectionCheck: true
              , defaultCapacity: _poolCount
              , maxSize: _maxPoolCount
            );
        }

        private void RunBindAll()
        {
            int index = 0;
            foreach (var itemSource in _collection.ItemsSource)
            {
                if (itemSource is ViewModel viewModel)
                {
                    RunBind(viewModel, index++);
                }
            }
        }

        private void RunBind(ViewModel viewModel, int index = 0)
        {
            if (!_viewModelChecker.TryGetValue(viewModel, out var duplicate))
            {
                var collectionItem = _itemPool?.Get();

                collectionItem.ViewModel = viewModel;
                IRedDotCollection irddc = viewModel as IRedDotCollection;
                if (irddc != null && irddc.RedDot != null)
                {
                    collectionItem.Index = irddc.RedDot.Id;
                    irddc.IsCreate = true;
                }
                else
                {
                    collectionItem.Index = index;
                }

                collectionItem.Unbind();
                collectionItem.Bind();

                _viewModelChecker.Add(viewModel, true);
                OnItemBinded(collectionItem);
            }
        }

        private void RemoveItem(int index)
        {
            var collectionItem = ActivatedItems[index];
            _itemPool?.Release(collectionItem);

            OnItemUnbinded(collectionItem);
            collectionItem.Unbind();
        }

        protected virtual void OnItemBinded(CollectionItem   target) { }
        protected virtual void OnItemUnbinded(CollectionItem target) { }

        public Action OnCompleteCreate;

        private void OnCollectionItemChanged(eNotifyCollectionChangedAction action, IList items, int index)
        {
            if (!_isInitialized)
                return;

            switch (action)
            {
                case eNotifyCollectionChangedAction.ADD:
                {
                    if (items[index] is ViewModel viewModel)
                    {
                        RunBind(viewModel, index);
                    }
                }
                    break;
                case eNotifyCollectionChangedAction.ADD_RANGE:
                {
                    int i = 0;
                    foreach (var item in items)
                    {
                        if (item is ViewModel viewModel)
                            RunBind(viewModel, i++);
                    }
                }
                    break;
                case eNotifyCollectionChangedAction.MOVE:
                    break;
                case eNotifyCollectionChangedAction.REMOVE:
                    RemoveItem(index);
                    break;
                case eNotifyCollectionChangedAction.REPLACE:
                    break;
                case eNotifyCollectionChangedAction.RESET:
                    Clear();
                    break;
                case eNotifyCollectionChangedAction.DESTROY_ALL:
                    DestroyAll();
                    break;
            }
        }

        private void Clear()
        {
            _clearingItemList.AddRange(ActivatedItems);
            foreach (var target in _clearingItemList)
            {
                _itemPool?.Release(target);
                OnItemUnbinded(target);
                target?.Unbind();
            }

            //RedDotManager.Instance.ClearCollectItem(eBadgeType.MAIN_MENU_SUB1_TAB1);
            _clearingItemList.Clear();
            _viewModelChecker.Clear();
        }

        private void Reset()
        {
            Clear();

            _collection?.RemoveEvent(OnCollectionItemChanged);
            _collection = null;
            _loadedAsset = null;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }


        private void DestroyAll()
        {
            Reset();
            _itemPool?.Dispose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyAll();
        }
    }
}
