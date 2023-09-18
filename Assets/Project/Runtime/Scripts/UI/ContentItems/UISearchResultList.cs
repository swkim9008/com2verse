/*===============================================================
* Product:		Com2Verse
* File Name:	UISearchResultList.cs
* Developer:	jhkim
* Date:			2022-09-07 15:19
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	// TMP_Dropdown
	[RequireComponent(typeof(UISearchResultListExtensions))]
	public sealed class UISearchResultList : MonoBehaviour
	{
#region Variables
		[Header("Input Field")]
		[SerializeField] private MetaverseSearchInputField _inputField;
		[SerializeField] private InputFieldExtensions _inputFieldExtensions;

		[Header("Scroll View")]
		[SerializeField] private ScrollRect _scroll;
		[HideInInspector] public UnityEvent<int> _onValueChanged;
		[HideInInspector] public UnityEvent<int> _onValueSubmitted;
		[HideInInspector] public UnityEvent<int> _onValueHovered;

		[Header("ETC")]
		[SerializeField] private TMP_Text _resultEmpty;
		[SerializeField] private Button _btnCancel;

		[Header("Options")]
		[Tooltip("검색 결과가 없을 때 스크롤뷰 활성화")]
		[SerializeField] private bool _showResultWhenEmpty;
		[Tooltip("결과 아이템 커서 이동시 끝에 도달하면 반대편 인덱스 선택")]
		[SerializeField] private bool _enableCursorNavigationLoop = false;
		[Tooltip("Blocker 오브젝트 사용")]
		[SerializeField] private bool _useBlocker = true;
		[Tooltip("취소 버튼 표시")]
		[SerializeField] private bool _useCancelButton = false;

		[SerializeField] private CollectionBinder _collectionBinder;

		private RectTransform _inputRt;
		private RectTransform _scrollRt;
		private float _inputFieldHeight;

		private UIBlocker _blocker;
		private List<SearchResultItem> _items = new();
		private int _selectedIndex = -1;
		private int _itemsInSingleScreen = -1;
		private float _itemHeight;

		private GUIView _view;

		private static readonly float VerticalScrollMoveDuration = .3f;
		private DG.Tweening.Tweener _verticalScrollTweener = null;
		public event Action OnValueChanged;
		public event Action OnValueSubmitted = () => { };
#endregion // Variables

#region Properties
		public int SelectedIndex
		{
			get => _selectedIndex;
			set => _selectedIndex = value;
		}

		public bool IsShowScroll
		{
			get => IsShow;
			set
			{
				if (value) ShowScrollView();
				else CloseScrollView();
			}
		}
		public int ChildCount => Collection?.CollectionCount ?? 0;

		// public int ChildCount => _items.Count;
		public bool IsShow => _scroll.gameObject.activeSelf;
		public string Text => _inputField.text;
		public string TextWithIME => _inputFieldExtensions.Text;
		private INotifyCollectionChanged Collection => _collectionBinder.Collection;
#endregion // Properties

#region Initialize
		private void Start()
		{
			if (!Application.isPlaying) return;
			Initialize();
		}

		private void Initialize()
		{
			InitSearchField();
			InitView();
			InitScrollView();
			InitCollection().Forget();
			InitCancelButton();
		}

		private void InitSearchField()
		{
			_inputField.OnMoveCursor += OnMoveCursor;
		}
		private void InitView()
		{
			_view = GetComponentInParent<GUIView>();
			_view.OnClosedEvent += _ =>
			{
				_inputField.SetTextWithoutNotify(string.Empty);
			};
		}
		private void InitScrollView()
		{
			_itemsInSingleScreen = CalcScrollableItemCount();
			gameObject.GetOrAddComponent<UISearchResultListExtensions>().Initialize();
			// _templateItem.gameObject.SetActive(false);
			CloseScrollView();
		}

		private async UniTask InitCollection()
		{
			await UniTask.WaitUntil(() => Collection != null);
			Collection?.AddEvent(OnCollectionChanged);
		}

		private void InitCancelButton()
		{
			if (!_useCancelButton || _btnCancel.IsReferenceNull()) return;
			_btnCancel.onClick.AddListener(() =>
			{
				_inputField.text = string.Empty;
				_inputFieldExtensions.Text = string.Empty;
				Reset();
				// OnValueChanged?.Invoke();
				CloseScrollView();
			});
		}
		private int CalcScrollableItemCount()
		{
			var listHeight = transform.GetComponent<RectTransform>().sizeDelta.y;
			return (int) (listHeight / _itemHeight);
		}
#endregion // Initializa

#region Public Methods
		private Action<Transform> _onTransformReady = null;
		private void AddItem(BaseSearchResultViewModel viewModel)
		{
			_onTransformReady ??= OnTransformReady;

			RefreshUI();

			// Set GameObject Component
			var transform = viewModel.Transform;
			if (transform.IsReferenceNull())
			{
				viewModel.SetOnTransformReady(_onTransformReady);
				return;
			}

			_onTransformReady?.Invoke(transform);

			void OnTransformReady(Transform transform)
			{
				var gameObject = transform.gameObject;
				var item = gameObject.GetOrAddComponent<SearchResultItem>();
				var toggle = gameObject.GetOrAddComponent<Toggle>();

				SetEvents(item, toggle);

				item.Idx = _items.Count;
				item.Toggle = toggle;
				item.gameObject.SetActive(true);
				viewModel.SearchText = _inputFieldExtensions.Text;
				_items.Add(item);

				UpdateToggleNavigation();
			}
			void SetEvents(SearchResultItem item, Toggle toggle)
			{
				item.OnSelected = idx => OnSelectItem(idx);
				item.OnSubmitted = idx => OnSubmitItem(idx);
				item.OnHovered = idx => OnHoverItem(idx);
				item.OnMoved = idx => OnMoveItem(idx);
				toggle.onValueChanged.RemoveAllListeners();
				toggle.onValueChanged.AddListener(_ => OnSelectItem(toggle));
			}
		}

		private void RemoveItem(BaseSearchResultViewModel viewModel)
		{
		}

		private int GetIndex(Toggle toggle) => _items.FindIndex(item => item.Toggle == toggle);
		void UpdateToggleNavigation()
		{
			var selectOnUpFallback = _enableCursorNavigationLoop ? _items[_items.Count - 1].Toggle : _items[0].Toggle;
			var selectOnDownFallback = _enableCursorNavigationLoop ? _items[0].Toggle : _items[_items.Count - 1].Toggle;

			for (var i = 0; i < _items.Count; i++)
			{
				var toggle = _items[i].Toggle;
				var navigation = toggle.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnUp = i > 0 ? _items[i - 1].Toggle : selectOnUpFallback;
				navigation.selectOnDown = i < _items.Count - 1 ? _items[i + 1].Toggle : selectOnDownFallback;
				toggle.navigation = navigation;
				_items[i].Toggle = toggle;
			}
		}
		void RefreshUI()
		{
			if (_itemsInSingleScreen != -1)
				_scroll.vertical = ChildCount > _itemsInSingleScreen;
			_resultEmpty.gameObject.SetActive(ChildCount == 0);
		}

		public void Show() => gameObject.SetActive(true);

		public void Hide() => gameObject.SetActive(false);
		public void ShowScrollView() => SetEnableScrollView(true);
		public void HideScrollView() => SetEnableScrollView(false);
#endregion // Public Methods

#region Input Field
		private void OnMoveCursor(MoveDirection direction)
		{
			switch (direction)
			{
				case MoveDirection.Up:
					OnMovePreviousItem();
					break;
				case MoveDirection.Down:
					OnMoveNextItem();
					break;
			}
		}
#endregion // Input Field

#region Search Result Item
		private void OnSelectItem(Toggle toggle)
		{
			var selectedIndex = -1;
			Transform tr = toggle.transform;
			Transform parent = tr.parent;
			var idx = 0;
			for (int i = 0; i < parent.childCount; ++i)
			{
				var child = parent.GetChild(i);
				if (child == tr)
				{
					selectedIndex = idx;
					break;
				}

				if (child.gameObject.activeSelf)
					idx++;
			}

			if (selectedIndex < 0)
				return;
			SelectedIndex = selectedIndex;
			_onValueChanged?.Invoke(SelectedIndex);
		}

		private void OnSelectItem(int selectedIndex)
		{
			if (selectedIndex < 0)
				return;
			SelectedIndex = selectedIndex;
			_onValueChanged?.Invoke(SelectedIndex);
		}

		private void OnHoverItem(int selectedIndex)
		{
			if (selectedIndex < 0)
				return;
			SelectedIndex = selectedIndex;
			_onValueHovered?.Invoke(SelectedIndex);
		}

		private void OnMovePreviousItem()
		{
			// var baseIdx = SelectedIndex == -1 ? 0 : SelectedIndex;
			// var moveIdx = baseIdx - 1 > 0 ? baseIdx - 1 : 0;
			// var nextToggle = GetToggle(moveIdx);
			// if (nextToggle != null)
			// {
			// 	// nextToggle.Select();
			// 	// _inputField.Select();
			// }
			//
			// SelectedIndex = moveIdx;
			// OnMoveItem(moveIdx);
		}

		private void OnMoveNextItem()
		{
			// var baseIdx = SelectedIndex == -1 ? 0 : SelectedIndex;
			// var moveIdx = baseIdx + 1 < ChildCount ? baseIdx + 1 : SelectedIndex;
			// var nextToggle = GetToggle(moveIdx);
			// if (nextToggle != null)
			// {
			// 	// nextToggle.Select();
			// 	// _inputField.Select();
			// }
			//
			// SelectedIndex = moveIdx;
			// OnMoveItem(moveIdx);
		}
		private void OnMoveItem(int moveIndex)
		{
			if (moveIndex < 0 || moveIndex >= ChildCount)
				return;
			// SetToggleOn(moveIndex);
			OnScrollItem(moveIndex);
		}

		private void OnScrollItem(int idx)
		{
			var normalizedPos = 1f - idx / (float) (ChildCount - 1);
			PlayScroll(normalizedPos);

			void PlayScroll(float targetPos)
			{
				if (_verticalScrollTweener != null && _verticalScrollTweener.IsPlaying())
				{
					_verticalScrollTweener.Kill();
					_verticalScrollTweener = null;
				}
				_verticalScrollTweener = _scroll.DOVerticalNormalizedPos(normalizedPos, VerticalScrollMoveDuration).Play();
				_verticalScrollTweener.onComplete = () => _verticalScrollTweener = null;
			}
		}

		private void OnSubmitItem(int idx)
		{
			_onValueSubmitted?.Invoke(idx);
			OnValueSubmitted?.Invoke();
		}

		private Toggle GetToggle(int index) => index >= 0 && index < _items.Count ? _items[index].Toggle : null;

		private void SetToggleOn(int index)
		{
			for (var i = 0; i < _items.Count; i++)
			{
				var toggle = _items[i].Toggle;
				toggle.SetIsOnWithoutNotify(index == i);
			}
		}
#endregion // Search Result Item

#region Scroll View
		private void SetEnableScrollView(bool show)
		{
			_scroll.gameObject.SetActive(show || _showResultWhenEmpty);

			if (show)
			{
				if (_useBlocker)
					_blocker ??= UIBlocker.CreateBlocker(transform, CloseScrollView);
			}
			else
			{
				if (_showResultWhenEmpty) return;
				CloseScrollView();
			}
		}

		private void CloseScrollView()
		{
			_scroll.gameObject.SetActive(false);
			if (_blocker != null)
			{
				_blocker.DestroyBlocker();
				_blocker = null;
			}
		}
#endregion // Scroll View

#region Callbacks
		private void OnCollectionChanged(eNotifyCollectionChangedAction action, IList list, int idx)
		{
			switch (action)
			{
				case eNotifyCollectionChangedAction.ADD:
					AddItem(list[idx] as BaseSearchResultViewModel);
					break;
				case eNotifyCollectionChangedAction.REMOVE:
					// RemoveItem(list[idx] as BaseSearchResultViewModel);
					break;
				case eNotifyCollectionChangedAction.REPLACE:
					break;
				case eNotifyCollectionChangedAction.MOVE:
					break;
				case eNotifyCollectionChangedAction.RESET:
					Reset();
					break;
				case eNotifyCollectionChangedAction.DESTROY_ALL:
					break;
				case eNotifyCollectionChangedAction.LOAD_COMPLETE:
					break;
				default:
					break;
			}

			OnValueChanged?.Invoke();
		}

		private void Reset()
		{
			_items.Clear();
			RefreshUI();
		}
#endregion // Callbacks

#region Data
		private sealed class SearchResultItem : MonoBehaviour, ISelectHandler, ISubmitHandler, IPointerEnterHandler, IMoveHandler
		{
			public int Idx { get; set; }
			public Toggle Toggle { get; set; }

			public Action<int> OnSelected;
			public Action<int> OnSubmitted;
			public Action<int> OnHovered;
			public Action<int> OnMoved;
			public void OnSelect(BaseEventData eventData)
			{
				if (eventData is PointerEventData) return;
				OnSelected?.Invoke(Idx);
			}

			public void OnSubmit(BaseEventData eventData)
			{
				OnSubmitted?.Invoke(Idx);
			}

			public void OnPointerEnter(PointerEventData eventData)
			{
				OnHovered?.Invoke(Idx);
			}

			public void OnMove(AxisEventData eventData)
			{
				var idx = Idx;
				switch (eventData.moveDir)
				{
					case MoveDirection.Left:
					case MoveDirection.Up:
						idx--;
						break;
					case MoveDirection.Right:
					case MoveDirection.Down:
						idx++;
						break;
					default:
						break;
				}
				OnMoved?.Invoke(idx);
			}
		}
#endregion // Data
	}
}
