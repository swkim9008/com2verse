/*===============================================================
* Product:		Com2Verse
* File Name:	RecyclableScrollRectBase.cs
* Developer:	eugene9721
* Date:			2022-10-06 16:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.EnhancedScroller;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 원본코드
// https://meta-svn.com2us.com/svn/Com2VERSE/AssetStoreAssets/echo17/Editor%20ExtensionsGUI/EnhancedScroller.unitypackage
namespace Com2Verse.RecyclableScroll
{
#region Delegate
	/// <summary>
	/// This delegate handles the visibility changes of cell views
	/// </summary>
	/// <param name="cellView">The cell view that changed visibility</param>
	public delegate void CellViewVisibilityChangedDelegate(EnhancedScrollerCellView cellView);

	/// <summary>
	/// This delegate will be fired just before the cell view is recycled
	/// </summary>
	/// <param name="cellView"></param>
	public delegate void CellViewWillRecycleDelegate(EnhancedScrollerCellView cellView);

	/// <summary>
	/// This delegate handles the scrolling callback of the ScrollRect.
	/// </summary>
	/// <param name="scroller">The scroller that called the delegate</param>
	/// <param name="val">The scroll value of the scroll rect</param>
	/// <param name="scrollPosition">The scroll position in pixels from the start of the scroller</param>
	public delegate void ScrollerScrolledDelegate(RecyclableScrollRectBase scroller, Vector2 val, float scrollPosition);

	/// <summary>
	/// This delegate handles the snapping of the scroller.
	/// </summary>
	/// <param name="scroller">The scroller that called the delegate</param>
	/// <param name="cellIndex">The index of the cell view snapped on (this may be different than the data index in case of looping)</param>
	/// <param name="dataIndex">The index of the data the view snapped on</param>
	public delegate void ScrollerSnappedDelegate(RecyclableScrollRectBase scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView);

	/// <summary>
	/// This delegate handles the change in state of the scroller (scrolling or not scrolling)
	/// </summary>
	/// <param name="scroller">The scroller that changed state</param>
	/// <param name="scrolling">Whether or not the scroller is scrolling</param>
	public delegate void ScrollerScrollingChangedDelegate(RecyclableScrollRectBase scroller, bool scrolling);

	/// <summary>
	/// This delegate handles the change in state of the scroller (jumping or not jumping)
	/// </summary>
	/// <param name="scroller">The scroller that changed state</param>
	/// <param name="tweening">Whether or not the scroller is tweening</param>
	public delegate void ScrollerTweeningChangedDelegate(RecyclableScrollRectBase scroller, bool tweening);

	/// <summary>
	/// This delegate is called when a cell view is created for the first time (not reused)
	/// </summary>
	/// <param name="scroller">The scroller that created the cell view</param>
	/// <param name="cellView">The cell view that was created</param>
	public delegate void CellViewInstantiated(RecyclableScrollRectBase scroller, EnhancedScrollerCellView cellView);

	/// <summary>
	/// This delegate is called when a cell view is reused from the recycled cell view list
	/// </summary>
	/// <param name="scroller">The scroller that reused the cell view</param>
	/// <param name="cellView">The cell view that was resused</param>
	public delegate void CellViewReused(RecyclableScrollRectBase scroller, EnhancedScrollerCellView cellView);
#endregion Delegate

#region Enum
	/// <summary>
	/// The direction this scroller is handling
	/// </summary>
	public enum ScrollDirectionEnum
	{
		VERTICAL,
		HORIZONTAL
	}

	/// <summary>
	/// Which side of a cell to reference.
	/// For vertical scrollers, before means above, after means below.
	/// For horizontal scrollers, before means to left of, after means to the right of.
	/// </summary>
	public enum CellViewPositionEnum
	{
		BEFORE,
		AFTER
	}

	/// <summary>
	/// This will set how the scroll bar should be shown based on the data. If no scrollbar
	/// is attached, then this is ignored. OnlyIfNeeded will hide the scrollbar based on whether
	/// the scroller is looping or there aren't enough items to scroll.
	/// </summary>
	public enum ScrollbarVisibilityEnum
	{
		ONLY_IF_NEEDED,
		ALWAYS,
		NEVER
	}
#endregion Enum

	/// <summary>
	/// The EnhancedScroller allows you to easily set up a dynamic scroller that will recycle views for you. This means
	/// that using only a handful of views, you can display thousands of rows. This will save memory and processing
	/// power in your application.
	/// </summary>
	public abstract class RecyclableScrollRectBase : ScrollRect, IBinding
	{
		/// <summary>
		/// 프로젝트에 따라 어떤 스크롤 타입을 반환할지 구현
		/// </summary>
		/// <returns>반한될 스크롤 타입</returns>
		protected abstract RecyclableScrollTypeBase GetRecyclableScrollController();

#region Class Enum
		public enum LoopJumpDirectionEnum
		{
			CLOSEST,
			UP,
			DOWN,
		}

		/// <summary>
		/// Where in the list we are
		/// </summary>
		private enum ListPositionEnum
		{
			FIRST,
			LAST
		}
#endregion Class Enum

#region SerializeField
		[SerializeField] protected eRecyclableScrollType _scrollType;

		[SerializeField] protected AssetReference _prefab;

		/// <summary>
		/// The direction the scroller is handling
		/// </summary>
		[SerializeField] protected ScrollDirectionEnum _scrollDirection;

		/// <summary>
		/// The number of pixels between cell views, starting after the first cell view
		/// </summary>
		[SerializeField] protected float _spacing;

		/// <summary>
		/// The padding inside of the scroller: top, bottom, left, right.
		/// </summary>
		[SerializeField] protected RectOffset _padding;

		/// <summary>
		/// Whether the scroller should loop the cell views
		/// </summary>
		[SerializeField] protected bool _loop;

		/// <summary>
		/// Whether the scroller should process loop jumping while being dragged.
		/// Note: if this is turned off while using a small list size, you may
		/// see elements missing while dragging near the edges of the list. Turning
		/// this value off can sometimes help with Unity adding a lot of velocity
		/// while dragging near the end of a list that loops. If this value is turned
		/// off, you can mitigate the large inertial velocity by setting the maxVelocity
		/// value to a non-zero amount (see maxVelocity).
		/// </summary>
		[SerializeField] protected bool _loopWhileDragging = true;

		/// <summary>
		/// The maximum speed the scroller can go. This can be useful to eliminate
		/// aggressive scrolling by the user. It can also be used to mitigate the
		/// large inertial velocity that Unity adds in the ScrollRect when dragging
		/// and looping near the edge of the list (See loopWhileDragging).
		/// </summary>
		[SerializeField] protected float _maxVelocity;

		/// <summary>
		/// Whether the scollbar should be shown
		/// </summary>
		[SerializeField] protected ScrollbarVisibilityEnum _scrollbarVisibility;

		/// <summary>
		/// Whether snapping is turned on
		/// </summary>
		[SerializeField] protected bool _snapping;

		/// <summary>
		/// This is the speed that will initiate the snap. When the
		/// scroller slows down to this speed it will snap to the location
		/// specified.
		/// </summary>
		[SerializeField] protected float _snapVelocityThreshold;

		/// <summary>
		/// The snap offset to watch for. When the snap occurs, this
		/// location in the scroller will be how which cell to snap to
		/// is determined.
		/// Typically, the offset is in the range 0..1, with 0 being
		/// the top / left of the scroller and 1 being the bottom / right.
		/// In most situations the watch offset and the jump offset
		/// will be the same, they are just separated in case you need
		/// that added functionality.
		/// </summary>
		[SerializeField] protected float _snapWatchOffset;

		/// <summary>
		/// The snap location to move the cell to. When the snap occurs,
		/// this location in the scroller will be where the snapped cell
		/// is moved to.
		/// Typically, the offset is in the range 0..1, with 0 being
		/// the top / left of the scroller and 1 being the bottom / right.
		/// In most situations the watch offset and the jump offset
		/// will be the same, they are just separated in case you need
		/// that added functionality.
		/// </summary>
		[SerializeField] protected float _snapJumpToOffset;

		/// <summary>
		/// Once the cell has been snapped to the scroller location, this
		/// value will determine how the cell is centered on that scroller
		/// location.
		/// Typically, the offset is in the range 0..1, with 0 being
		/// the top / left of the cell and 1 being the bottom / right.
		/// </summary>
		[SerializeField] protected float _snapCellCenterOffset;

		/// <summary>
		/// Whether to include the spacing between cells when determining the
		/// cell offset centering.
		/// </summary>
		[SerializeField] protected bool _snapUseCellSpacing;

		/// <summary>
		/// What function to use when interpolating between the current
		/// scroll position and the snap location. This is also known as easing.
		/// If you want to go immediately to the snap location you can either
		/// set the snapTweenType to immediate or set the snapTweenTime to zero.
		/// </summary>
		[SerializeField] protected Ease _snapTweenType;

		/// <summary>
		/// The time it takes to interpolate between the current scroll
		/// position and the snap location.
		/// If you want to go immediately to the snap location you can either
		/// set the snapTweenType to immediate or set the snapTweenTime to zero.
		/// </summary>
		[SerializeField] protected float _snapTweenTime;

		/// <summary>
		/// While true keeps snapping while the scroller is dragged.
		/// While false, this will disable snapping until the dragging stops.
		/// </summary>
		[SerializeField] protected bool _snapWhileDragging;

		/// <summary>
		/// The scrollers position
		/// </summary>
		[ReadOnly] [SerializeField] protected float _scrollPosition;
#endregion SerializeField

#region Delegate
		/// <summary>
		/// This delegate is called when a cell view is hidden or shown
		/// </summary>
		public CellViewVisibilityChangedDelegate CellViewVisibilityChanged { get; set; }

		/// <summary>
		/// This delegate is called just before a cell view is hidden by recycling
		/// </summary>
		public CellViewWillRecycleDelegate CellViewWillRecycle { get; set; }

		/// <summary>
		/// This delegate is called when the scroll rect scrolls
		/// </summary>
		public ScrollerScrolledDelegate ScrollerScrolled { get; set; }

		/// <summary>
		/// This delegate is called when the scroller has snapped to a position
		/// </summary>
		private ScrollerSnappedDelegate ScrollerSnapped { get; set; }

		/// <summary>
		/// This delegate is called when the scroller has started or stopped scrolling
		/// </summary>
		public ScrollerScrollingChangedDelegate ScrollerScrollingChanged { get; set; }

		/// <summary>
		/// This delegate is called when the scroller has started or stopped tweening
		/// </summary>
		public ScrollerTweeningChangedDelegate ScrollerTweeningChanged { get; set; }

		/// <summary>
		/// This delegate is called when the scroller creates a new cell view from scratch
		/// </summary>
		public CellViewInstantiated CellViewInstantiated { get; set; }

		/// <summary>
		/// This delegate is called when the scroller reuses a recycled cell view
		/// </summary>
		public CellViewReused CellViewReused { get; set; }
#endregion Delegate

#region DataBinder
		private bool _isPrefabInitialized;

		private CancellationTokenSource  _cancellationTokenSource;
		private EnhancedScrollerCellView _cellAsset;
		public  EnhancedScrollerCellView CellViewPrefab => _cellAsset;

		private INotifyCollectionChanged _collection;

		[UsedImplicitly]
		public INotifyCollectionChanged Collection
		{
			get => _collection;
			set
			{
				if (value == null)
				{
					C2VDebug.LogError($"[{nameof(RecyclableScrollRectBase)}] Can't find collection.");
					return;
				}

				_isPrefabInitialized = false;
				_cancellationTokenSource?.Cancel();

				_collection = value;
				_collection.AddEvent(OnCollectionItemChanged);

				OnInitializeAfterAssetLoad().Forget();
			}
		}

		private async UniTask OnInitializeAfterAssetLoad()
		{
			_cancellationTokenSource = new CancellationTokenSource();

			var loadedAsset = await C2VAddressables.LoadAssetAsync<GameObject>(_prefab).ToUniTask();
			if (_cancellationTokenSource.IsCancellationRequested) return;

			if (!loadedAsset.TryGetComponent(out _cellAsset))
			{
				C2VDebug.LogError($"[{nameof(RecyclableScrollRectBase)}] Can't find cell asset.");
			}

			_isPrefabInitialized = true;

			Initialize();

			RefreshCollection();
		}

		public void Bind() { }

		public void Unbind()
		{
			ResetCollection();
		}

		private void RefreshCollection()
		{
			if (_collection == null) return;
			RecyclableScrollType.Data.Clear();
			foreach (RecyclableCellViewModel viewModel in _collection.ItemsSource)
				RecyclableScrollType.Data.Add(viewModel);
			_reloadData = true;
		}

		private void ResetCollection()
		{
			_collection?.RemoveEvent(OnCollectionItemChanged);
			_collection = null;
			_prefab     = null;

			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = null;
		}

		private void OnCollectionItemChanged(eNotifyCollectionChangedAction action, IList items, int index)
		{
			if (!_isPrefabInitialized) return;

			switch (action)
			{
				case eNotifyCollectionChangedAction.ADD:
					if (items[index] is RecyclableCellViewModel)
						RecyclableScrollType.Data.Add(items[index] as RecyclableCellViewModel);
					else
					{
						C2VDebug.LogWarning($"[{nameof(RecyclableScrollRectBase)}] Invalid type");
						items.RemoveAt(index);
					}
					break;
				case eNotifyCollectionChangedAction.MOVE:
					break;
				case eNotifyCollectionChangedAction.REMOVE:
					if (RecyclableScrollType.Data.Count == 0) break;
					RecyclableScrollType.Data.Remove(RecyclableScrollType.Data[index]);
					break;
				case eNotifyCollectionChangedAction.REPLACE:
					break;
				case eNotifyCollectionChangedAction.RESET:
					if (RecyclableScrollType.Data.Count == 0) break;
					RecyclableScrollType.Data.Clear();
					break;
				case eNotifyCollectionChangedAction.DESTROY_ALL:
					DestroyAll();
					break;
			}

			_reloadData = true;
		}

		private void DestroyAll()
		{
			ResetCollection();
		}
#endregion DataBinder

#region Public Propeties
		/// <summary>
		/// The Delegate is what the scroller will call when it needs to know information about
		/// the underlying data or views. This allows a true MVC process.
		/// </summary>
		public RecyclableScrollTypeBase RecyclableScrollType
		{
			get => _recyclableScrollType ??= GetRecyclableScrollController();
			set
			{
				_recyclableScrollType = value;
				_reloadData           = true;
			}
		}

		public ScrollDirectionEnum ScrollDirection => _scrollDirection;

		/// <summary>
		/// The absolute position in pixels from the start of the scroller
		/// </summary>
		public float ScrollPosition
		{
			get => _scrollPosition;
			set
			{
				if (_loop)
				{
					// if we are looping, we need to make sure the new position isn't past the jump trigger.
					// if it is we need to reset back to the jump position on the other side of the area.

					//if (value > _loopLastJumpTrigger)
					//{
					//	value = _loopFirstScrollPosition + (value - _loopLastJumpTrigger);
					//}
					//else if (value < _loopFirstJumpTrigger)
					//{
					//	value = _loopLastScrollPosition - (_loopFirstJumpTrigger - value);
					//}
				}
				else
				{
					// make sure the position is in the bounds of the current set of views
					value = Mathf.Clamp(value, 0, ScrollSize);
				}

				// only if the value has changed
				if (Mathf.Approximately(_scrollPosition, value)) return;
				_scrollPosition = value;
				if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
				{
					// set the vertical position
					base.verticalNormalizedPosition = 1f - (_scrollPosition / ScrollSize);
				}
				else
				{
					// set the horizontal position
					base.horizontalNormalizedPosition = (_scrollPosition / ScrollSize);
				}

				// flag that we need to refresh
				//_refreshActive = true;
			}
		}

		/// <summary>
		/// The size of the active cell view container minus the visible portion
		/// of the scroller
		/// </summary>
		public float ScrollSize => _scrollDirection == ScrollDirectionEnum.VERTICAL
			? Mathf.Max(_container.rect.height - _scrollRectTransform.rect.height, 0)
			: Mathf.Max(_container.rect.width - _scrollRectTransform.rect.width, 0);

		/// <summary>
		/// The normalized position of the scroller between 0 and 1
		/// </summary>
		public float NormalizedScrollPosition
		{
			get
			{
				var scrollPosition = ScrollPosition;
				return (scrollPosition <= 0 ? 0 : _scrollPosition / ScrollSize);
			}
		}

		/// <summary>
		/// Whether the scroller should loop the resulting cell views.
		/// Looping creates three sets of internal size data, attempting
		/// to keep the scroller in the middle set. If the scroller goes
		/// outside of this set, it will jump back into the middle set,
		/// giving the illusion of an infinite set of data.
		/// </summary>
		public bool Loop
		{
			get => _loop;
			set
			{
				// only if the value has changed
				if (_loop == value) return;
				// get the original position so that when we turn looping on
				// we can jump back to this position
				var originalScrollPosition = _scrollPosition;

				_loop = value;

				// call resize to generate more internal elements if loop is on,
				// remove the elements if loop is off
				Resize(false);

				if (_loop)
				{
					// set the new scroll position based on the middle set of data + the original position
					ScrollPosition = _loopFirstScrollPosition + originalScrollPosition;
				}
				else
				{
					// set the new scroll position based on the original position and the first loop position
					ScrollPosition = originalScrollPosition - _loopFirstScrollPosition;
				}

				// update the scrollbars
				ScrollbarVisibility = _scrollbarVisibility;
			}
		}

		/// <summary>
		/// Sets how the visibility of the scrollbars should be handled
		/// </summary>
		public new ScrollbarVisibilityEnum ScrollbarVisibility
		{
			get => _scrollbarVisibility;
			set
			{
				_scrollbarVisibility = value;

				// only if the scrollbar exists
				if (_scrollbar == null) return;
				// make sure we actually have some cell views
				if (_cellViewOffsetArray is not { Count: > 0 }) return;
				if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
				{
					base.verticalScrollbar = _scrollbar;
				}
				else
				{
					base.horizontalScrollbar = _scrollbar;
				}

				if (_cellViewOffsetArray.Last() < ScrollRectSize || _loop)
				{
					// if the size of the scrollable area is smaller than the scroller
					// or if we have looping on, hide the scrollbar unless the visibility
					// is set to Always.
					_scrollbar.gameObject.SetActive(_scrollbarVisibility == ScrollbarVisibilityEnum.ALWAYS);
				}
				else
				{
					// if the size of the scrollable areas is larger than the scroller
					// or looping is off, then show the scrollbars unless visibility
					// is set to Never.
					_scrollbar.gameObject.SetActive(_scrollbarVisibility != ScrollbarVisibilityEnum.NEVER);
				}

				if (!_scrollbar.gameObject.activeSelf)
				{
					base.verticalScrollbar   = null;
					base.horizontalScrollbar = null;
				}
			}
		}

		/// <summary>
		/// This is the velocity of the scroller.
		/// </summary>
		public Vector2 Velocity
		{
			get => base.velocity;
			set => base.velocity = value;
		}

		/// <summary>
		/// The linear velocity is the velocity on one axis.
		/// The scroller should only be moving one one axix.
		/// </summary>
		public float LinearVelocity
		{
			get =>
				// return the velocity component depending on which direction this is scrolling
				(_scrollDirection == ScrollDirectionEnum.VERTICAL ? base.velocity.y : base.velocity.x);
			set =>
				// set the appropriate component of the velocity
				base.velocity = _scrollDirection == ScrollDirectionEnum.VERTICAL ? new Vector2(0, value) : new Vector2(value, 0);
		}

		/// <summary>
		/// Whether the scroller is scrolling or not
		/// </summary>
		public bool IsScrolling { get; private set; }

		/// <summary>
		/// Whether the scroller is tweening or not
		/// </summary>
		public bool IsTweening { get; private set; }

		/// <summary>
		/// This is the first cell view index showing in the scroller's visible area
		/// </summary>
		public int StartCellViewIndex => _activeCellViewsStartIndex;

		/// <summary>
		/// This is the last cell view index showing in the scroller's visible area
		/// </summary>
		public int EndCellViewIndex => _activeCellViewsEndIndex;

		/// <summary>
		/// This is the first data index showing in the scroller's visible area
		/// </summary>
		public int StartDataIndex => _activeCellViewsStartIndex % NumberOfCells;

		/// <summary>
		/// This is the last data index showing in the scroller's visible area
		/// </summary>
		public int EndDataIndex => _activeCellViewsEndIndex % NumberOfCells;

		/// <summary>
		/// This is the number of cells in the scroller
		/// </summary>
		public int NumberOfCells => _recyclableScrollType?.GetNumberOfCells() ?? 0;

		/// <summary>
		/// The size of the visible portion of the scroller
		/// </summary>
		public float ScrollRectSize => _scrollDirection == ScrollDirectionEnum.VERTICAL ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width;

		/// <summary>
		/// The first padder before the visible cells
		/// </summary>
		public LayoutElement FirstPadder => _firstPadder;

		/// <summary>
		/// The last padder after the visible cells
		/// </summary>
		public LayoutElement LastPadder => _lastPadder;

		/// <summary>
		/// Access to the scroll rect container
		/// </summary>
		public RectTransform Container => _container;

		/// <summary>
		/// The amount of space to look ahead before the scroller position.
		/// This allows cells to be loaded before the first visible cell even if they
		/// are not displayed yet. Good for tweening and loading external resources.
		/// </summary>
		private float _lookAheadBefore;

		public float LookAheadBefore
		{
			get => _lookAheadBefore;
			set => _lookAheadBefore = Mathf.Abs(value);
		}

		/// <summary>
		/// The amount of space to look ahead after the last visible cell.
		/// This allows cells to be loaded before the last visible cell even if they
		/// are not displayed yet. Good for tweening and loading external resources.
		/// </summary>
		private float _lookAheadAfter;

		public float LookAheadAfter
		{
			get => _lookAheadAfter;
			set => _lookAheadAfter = Mathf.Abs(value);
		}
#endregion Public Propeties

#region Public Methos
		/// <summary>
		/// Create a cell view, or recycle one if it already exists
		/// </summary>
		/// <param name="cellPrefab">The prefab to use to create the cell view</param>
		/// <returns></returns>
		public EnhancedScrollerCellView GetCellView(EnhancedScrollerCellView cellPrefab)
		{
			// see if there is a view to recycle
			var cellView = GetRecycledCellView(cellPrefab);
			if (cellView == null)
			{
				// no recyleable cell found, so we create a new view
				// and attach it to our container
				var go = Instantiate(cellPrefab.gameObject);
				cellView = go.GetComponent<EnhancedScrollerCellView>();
				cellView.transform.SetParent(_container);
				cellView.transform.localPosition = Vector3.zero;
				cellView.transform.localRotation = Quaternion.identity;

				// call the instantiated callback
				if (CellViewInstantiated != null)
				{
					CellViewInstantiated(this, cellView);
				}
			}
			else
			{
				// reactivate the cell view from one that was recycled
				cellView.gameObject.SetActive(true);

				// call the reused callback
				CellViewReused?.Invoke(this, cellView);
			}

			return cellView;
		}

		/// <summary>
		/// This resets the internal size list and refreshes the cell views
		/// </summary>
		/// <param name="scrollPositionFactor">The percentage of the scroller to start at between 0 and 1, 0 being the start of the scroller</param>
		/// <param name="reloadRequest">if the user wants to update the data, Scrolls may also need to be notified of the request.</param>
		public void ReloadData(float scrollPositionFactor = 0, bool reloadRequest = false)
		{
			if (reloadRequest)
				RecyclableScrollType?.OnReloadRequest();
			
			_reloadData = false;

			// recycle all the active cells so
			// that we are sure to get fresh views
			RecycleAllCells();

			// if we have a delegate handling our data, then
			// call the resize
			if (_recyclableScrollType != null)
				Resize(false);

			if (_scrollRectTransform == null || _container == null)
			{
				_scrollPosition = 0f;
				return;
			}

			_scrollPosition = Mathf.Clamp(scrollPositionFactor * ScrollSize, 0, ScrollSize);
			if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
			{
				// set the vertical position
				base.verticalNormalizedPosition = 1f - scrollPositionFactor;
			}
			else
			{
				// set the horizontal position
				base.horizontalNormalizedPosition = scrollPositionFactor;
			}

			RefreshActive();
		}

		/// <summary>
		/// This calls the RefreshCellView method on each active cell.
		/// If you override the RefreshCellView method in your cells
		/// then you can update the UI without having to reload the data.
		/// Note: this will not change the cell sizes, you will need
		/// to call ReloadData for that to work.
		/// </summary>
		public void RefreshActiveCellViews()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				_activeCellViews[i].RefreshCellView();
			}
		}

		/// <summary>
		/// Removes all cells, both active and recycled from the scroller.
		/// This will call garbage collection.
		/// </summary>
		public void ClearAll()
		{
			ClearActive();
			ClearRecycled();
		}

		/// <summary>
		/// Removes all the active cell views. This should only be used if you want
		/// to get rid of cells because of settings set by Unity that cannot be
		/// changed at runtime. This will call garbage collection.
		/// </summary>
		public void ClearActive()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				DestroyImmediate(_activeCellViews[i].gameObject);
			}

			_activeCellViews.Clear();
		}

		/// <summary>
		/// Removes all the recycled cell views. This should only be used after you
		/// load in a completely different set of cell views that will not use the
		/// recycled views. This will call garbage collection.
		/// </summary>
		public void ClearRecycled()
		{
			for (var i = 0; i < _recycledCellViews.Count; i++)
			{
				DestroyImmediate(_recycledCellViews[i].gameObject);
			}

			_recycledCellViews.Clear();
		}

		/// <summary>
		/// Turn looping on or off. This is just a helper function so
		/// you don't have to keep track of the state of the looping
		/// in your own scripts.
		/// </summary>
		public void ToggleLoop()
		{
			Loop = !_loop;
		}

		/// <summary>
		/// Toggle whether the loop jump calculation is used. Loop jumps
		/// give the appearance of a continuous stream of cells, when in
		/// reality it is just a set of three groups of cells.
		/// Loop jumps can cause issues if you are trying to change the size of
		/// a cell manually (like for expanding / collapsing) around the
		/// borders of the cell groups where the jump occurs.
		/// </summary>
		/// <param name="ignore"></param>
		public void IgnoreLoopJump(bool ignore)
		{
			_ignoreLoopJump = ignore;
		}

		/// <summary>
		/// Sets the scroll position and refresh the active cells.
		/// Normally the refreshing would occur the next frame as Unity
		/// picks up the change in the ScrollRect's position.
		/// If you need to handle active cells immediately after setting
		/// the scroll position, use this method instead of setting
		/// the ScrollPosition property directly.
		/// </summary>
		/// <param name="scrollPosition"></param>
		public void SetScrollPositionImmediately(float scrollPosition)
		{
			ScrollPosition = scrollPosition;
			RefreshActive();
		}

		/// <summary>
		/// Jump to a position in the scroller based on a dataIndex. This overload allows you
		/// to specify a specific offset within a cell as well.
		/// </summary>
		/// <param name="dataIndex">he data index to jump to</param>
		/// <param name="scrollerOffset">The offset from the start (top / left) of the scroller in the range 0..1.
		/// Outside this range will jump to the location before or after the scroller's viewable area</param>
		/// <param name="cellOffset">The offset from the start (top / left) of the cell in the range 0..1</param>
		/// <param name="useSpacing">Whether to calculate in the spacing of the scroller in the jump</param>
		/// <param name="tweenType">What easing to use for the jump</param>
		/// <param name="tweenTime">How long to interpolate to the jump point</param>
		/// <param name="jumpComplete">This delegate is fired when the jump completes</param>
		/// <param name="loopJumpDirection"></param>
		/// <param name="forceCalculateRange"></param>
		public void JumpToDataIndex(int dataIndex,
		                            float scrollerOffset = 0,
		                            float cellOffset = 0,
		                            bool useSpacing = true,
		                            Ease tweenType = Ease.Linear,
		                            float tweenTime = 0f,
		                            Action jumpComplete = null,
		                            LoopJumpDirectionEnum loopJumpDirection = LoopJumpDirectionEnum.CLOSEST,
		                            bool forceCalculateRange = false
		)
		{
			var cellOffsetPosition = 0f;

			if (cellOffset != 0)
			{
				// calculate the cell offset position

				// get the cell's size
				var cellSize = (_recyclableScrollType != null ? _recyclableScrollType.GetCellViewSize(dataIndex) : 0);

				if (useSpacing)
				{
					// if using spacing add spacing from one side
					cellSize += _spacing;

					// if this is not a bounday cell, then add spacing from the other side
					if (dataIndex > 0 && dataIndex < (NumberOfCells - 1)) cellSize += _spacing;
				}

				// calculate the position based on the size of the cell and the offset within that cell
				cellOffsetPosition = cellSize * cellOffset;
			}

			if (Mathf.Approximately(scrollerOffset, 1f))
			{
				cellOffsetPosition += _padding.bottom;
			}

			// cache the offset for quicker calculation
			var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;

			var newScrollPosition = 0f;

			if (_loop)
			{
				// if looping, then we need to determine the closest jump position.
				// we do that by checking all three sets of data locations, and returning the closest one

				var numberOfCells = NumberOfCells;

				// get the scroll positions for each data set.
				// Note: we are calculating the position based on the cell view index, not the data index here

				var set1CellViewIndex = _loopFirstCellIndex - (numberOfCells - dataIndex);
				var set2CellViewIndex = _loopFirstCellIndex + dataIndex;
				var set3CellViewIndex = _loopFirstCellIndex + numberOfCells + dataIndex;

				var set1Position = GetScrollPositionForCellViewIndex(set1CellViewIndex, CellViewPositionEnum.BEFORE) + offset;
				var set2Position = GetScrollPositionForCellViewIndex(set2CellViewIndex, CellViewPositionEnum.BEFORE) + offset;
				var set3Position = GetScrollPositionForCellViewIndex(set3CellViewIndex, CellViewPositionEnum.BEFORE) + offset;

				// get the offsets of each scroll position from the current scroll position
				var set1Diff = (Mathf.Abs(_scrollPosition - set1Position));
				var set2Diff = (Mathf.Abs(_scrollPosition - set2Position));
				var set3Diff = (Mathf.Abs(_scrollPosition - set3Position));

				var setOffset = -(scrollerOffset * ScrollRectSize);

				var currentSet           = 0;
				var currentCellViewIndex = 0;
				var nextCellViewIndex    = 0;

				if (loopJumpDirection is LoopJumpDirectionEnum.UP or LoopJumpDirectionEnum.DOWN)
				{
					currentCellViewIndex = GetCellViewIndexAtPosition(_scrollPosition - setOffset + 0.0001f);

					if (currentCellViewIndex < numberOfCells)
					{
						currentSet        = 1;
						nextCellViewIndex = dataIndex;
					}
					else if (currentCellViewIndex >= numberOfCells && currentCellViewIndex < (numberOfCells * 2))
					{
						currentSet        = 2;
						nextCellViewIndex = dataIndex + numberOfCells;
					}
					else
					{
						currentSet        = 3;
						nextCellViewIndex = dataIndex + (numberOfCells * 2);
					}
				}

				switch (loopJumpDirection)
				{
					case LoopJumpDirectionEnum.CLOSEST:
						// choose the smallest offset from the current position (the closest position)
						if (set1Diff < set2Diff)
						{
							newScrollPosition = set1Diff < set3Diff ? set1Position : set3Position;
						}
						else
						{
							newScrollPosition = set2Diff < set3Diff ? set2Position : set3Position;
						}

						break;
					case LoopJumpDirectionEnum.UP:
						if (nextCellViewIndex < currentCellViewIndex)
						{
							newScrollPosition = (currentSet == 1 ? set1Position : (currentSet == 2 ? set2Position : set3Position));
						}
						else
						{
							if (currentSet == 1 && (currentCellViewIndex == dataIndex))
							{
								newScrollPosition = set1Position - _singleLoopGroupSize;
							}
							else
							{
								newScrollPosition = (currentSet == 1 ? set3Position : (currentSet == 2 ? set1Position : set2Position));
							}
						}

						break;
					case LoopJumpDirectionEnum.DOWN:

						if (nextCellViewIndex > currentCellViewIndex)
						{
							newScrollPosition = (currentSet == 1 ? set1Position : (currentSet == 2 ? set2Position : set3Position));
						}
						else
						{
							if (currentSet == 3 && (currentCellViewIndex == nextCellViewIndex))
							{
								newScrollPosition = set3Position + _singleLoopGroupSize;
							}
							else
							{
								newScrollPosition = (currentSet == 1 ? set2Position : (currentSet == 2 ? set3Position : set1Position));
							}
						}

						break;
				}

				if (useSpacing)
				{
					newScrollPosition -= _spacing;
				}
			}
			else
			{
				// not looping, so just get the scroll position from the dataIndex
				newScrollPosition = GetScrollPositionForDataIndex(dataIndex, CellViewPositionEnum.BEFORE) + offset;

				// clamp the scroll position to a valid location
				newScrollPosition = Mathf.Clamp(newScrollPosition - (useSpacing ? _spacing : 0), 0, ScrollSize);
			}

			// ignore the jump if the scroll position hasn't changed
			if (Mathf.Approximately(newScrollPosition, _scrollPosition))
			{
				jumpComplete?.Invoke();
				return;
			}

			// start tweening
			TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete, forceCalculateRange).Forget();
		}

		/// <summary>
		/// Snaps the scroller on command. This is called internally when snapping is set to true and the velocity
		/// has dropped below the threshold. You can use this to manually snap whenever you like.
		/// </summary>
		public void Snap()
		{
			if (NumberOfCells == 0) return;

			// set snap jumping to true so other events won't process while tweening
			_snapJumping = true;

			// stop the scroller
			LinearVelocity = 0;

			// cache the current inertia state and turn off inertia
			_snapInertia = base.inertia;
			base.inertia = false;

			// calculate the snap position
			var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(_snapWatchOffset));

			// get the cell view index of cell at the watch location
			_snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);

			// get the data index of the cell at the watch location
			_snapDataIndex = _snapCellViewIndex % NumberOfCells;

			// jump the snapped cell to the jump offset location and center it on the cell offset
			JumpToDataIndex(_snapDataIndex, _snapJumpToOffset, _snapCellCenterOffset, _snapUseCellSpacing, _snapTweenType, _snapTweenTime, SnapJumpComplete);
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
		/// </summary>
		/// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForCellViewIndex(int cellViewIndex, CellViewPositionEnum insertPosition)
		{
			if (NumberOfCells == 0) return 0;
			if (cellViewIndex < 0) cellViewIndex = 0;

			if (cellViewIndex == 0 && insertPosition == CellViewPositionEnum.BEFORE)
			{
				return 0;
			}

			if (cellViewIndex >= _cellViewOffsetArray.Count)
				return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];
			// the index is in the range of cell view offsets

			if (insertPosition == CellViewPositionEnum.BEFORE)
			{
				// return the previous cell view's offset + the spacing between cell views
				return _cellViewOffsetArray[cellViewIndex - 1] + _spacing + (_scrollDirection == ScrollDirectionEnum.VERTICAL ? _padding.top : _padding.left);
			}

			// return the offset of the cell view (offset is after the cell)
			return _cellViewOffsetArray[cellViewIndex] + (_scrollDirection == ScrollDirectionEnum.VERTICAL ? _padding.top : _padding.left);

			// get the start position of the last cell (the offset of the second to last cell)
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the dataIndex
		/// </summary>
		/// <param name="dataIndex">The data index to look for</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForDataIndex(int dataIndex, CellViewPositionEnum insertPosition)
		{
			return GetScrollPositionForCellViewIndex(_loop ? _recyclableScrollType.GetNumberOfCells() + dataIndex : dataIndex, insertPosition);
		}

		/// <summary>
		/// Gets the index of a cell view at a given position
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <returns></returns>
		public int GetCellViewIndexAtPosition(float position)
		{
			// call the overrloaded method on the entire range of the list
			return GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
		}

		/// <summary>
		/// Get a cell view for a particular data index. If the cell view is not currently
		/// in the visible range, then this method will return null.
		/// Note: this is against MVC principles and will couple your controller to the view
		/// more than this paradigm would suggest. Generally speaking, the view can have knowledge
		/// about the controller, but the controller should not know anything about the view.
		/// Use this method sparingly if you are trying to adhere to strict MVC design.
		/// </summary>
		/// <param name="dataIndex">The data index of the cell view to return</param>
		/// <returns></returns>
		public EnhancedScrollerCellView GetCellViewAtDataIndex(int dataIndex)
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				if (_activeCellViews[i].dataIndex == dataIndex)
				{
					return _activeCellViews[i];
				}
			}

			return null;
		}
#endregion Public Methos

#region Private Fields
		/// <summary>
		/// Set after the scroller is first created. This allwos
		/// us to ignore OnValidate changes at the start
		/// </summary>
		private bool _initialized = false;

		/// <summary>
		/// Set when the spacing is changed in the inspector. Since we cannot
		/// make changes during the OnValidate, we have to use this flag to
		/// later call the UpdateSpacing method from Update()
		/// </summary>
		private bool _updateSpacing = false;

		/// <summary>
		/// Cached reference to the scrollRect's transform
		/// </summary>
		private RectTransform _scrollRectTransform;

		/// <summary>
		/// Cached reference to the scrollbar if it exists
		/// </summary>
		private Scrollbar _scrollbar;

		/// <summary>
		/// Cached reference to the active cell view container
		/// </summary>
		private RectTransform _container;

		/// <summary>
		/// Cached reference to the layout group that handles view positioning
		/// </summary>
		private HorizontalOrVerticalLayoutGroup _layoutGroup;

		/// <summary>
		/// Reference to the delegate that will tell this scroller information
		/// about the underlying data
		/// </summary>
		private RecyclableScrollTypeBase _recyclableScrollType;

		/// <summary>
		/// Flag to tell the scroller to reload the data
		/// </summary>
		private bool _reloadData;

		/// <summary>
		/// Flag to tell the scroller to refresh the active list of cell views
		/// </summary>
		private bool _refreshActive;

		/// <summary>
		/// List of views that have been recycled
		/// </summary>
		private SmallList<EnhancedScrollerCellView> _recycledCellViews = new SmallList<EnhancedScrollerCellView>();

		/// <summary>
		/// Cached reference to the element used to offset the first visible cell view
		/// </summary>
		private LayoutElement _firstPadder;

		/// <summary>
		/// Cached reference to the element used to keep the cell views at the correct size
		/// </summary>
		private LayoutElement _lastPadder;

		/// <summary>
		/// Cached reference to the container that holds the recycled cell views
		/// </summary>
		private RectTransform _recycledCellViewContainer;

		/// <summary>
		/// Internal list of cell view sizes. This is created when the data is reloaded
		/// to speed up processing.
		/// </summary>
		private SmallList<float> _cellViewSizeArray = new SmallList<float>();

		/// <summary>
		/// Internal list of cell view offsets. Each cell view offset is an accumulation
		/// of the offsets previous to it.
		/// This is created when the data is reloaded to speed up processing.
		/// </summary>
		private SmallList<float> _cellViewOffsetArray = new SmallList<float>();

		/// <summary>
		/// The list of cell views that are currently being displayed
		/// </summary>
		private SmallList<EnhancedScrollerCellView> _activeCellViews = new SmallList<EnhancedScrollerCellView>();

		/// <summary>
		/// The index of the first cell view that is being displayed
		/// </summary>
		private int _activeCellViewsStartIndex;

		/// <summary>
		/// The index of the last cell view that is being displayed
		/// </summary>
		private int _activeCellViewsEndIndex;

		/// <summary>
		/// The index of the first element of the middle section of cell view sizes.
		/// Used only when looping
		/// </summary>
		private int _loopFirstCellIndex;

		/// <summary>
		/// The index of the last element of the middle seciton of cell view sizes.
		/// used only when looping
		/// </summary>
		private int _loopLastCellIndex;

		/// <summary>
		/// The scroll position of the first element of the middle seciotn of cell views.
		/// Used only when looping
		/// </summary>
		private float _loopFirstScrollPosition;

		/// <summary>
		/// The scroll position of the last element of the middle section of cell views.
		/// Used only when looping
		/// </summary>
		private float _loopLastScrollPosition;

		/// <summary>
		/// The position that triggers the scroller to jump to the end of the middle section
		/// of cell views. This keeps the scroller in the middle section as much as possible.
		/// </summary>
		private float _loopFirstJumpTrigger;

		/// <summary>
		/// The position that triggers the scroller to jump to the start of the middle section
		/// of cell views. This keeps the scroller in the middle section as much as possible.
		/// </summary>
		private float _loopLastJumpTrigger;

		/// <summary>
		/// The cached value of the last scroll rect size. This is checked every frame to see
		/// if the scroll rect has resized. If so, it will refresh.
		/// </summary>
		private float _lastScrollRectSize;

		/// <summary>
		/// The cached value of the last loop setting. This is checked every frame to see
		/// if looping was toggled. If so, it will refresh.
		/// </summary>
		private bool _lastLoop;

		/// <summary>
		/// The cell view index we are snapping to
		/// </summary>
		private int _snapCellViewIndex;

		/// <summary>
		/// The data index we are snapping to
		/// </summary>
		private int _snapDataIndex;

		/// <summary>
		/// Whether we are currently jumping due to a snap
		/// </summary>
		private bool _snapJumping;

		/// <summary>
		/// What the previous inertia setting was before the snap jump.
		/// We cache it here because we need to turn off inertia while
		/// manually tweeing.
		/// </summary>
		private bool _snapInertia;

		/// <summary>
		/// The cached value of the last scrollbar visibility setting. This is checked every
		/// frame to see if the scrollbar visibility needs to be changed.
		/// </summary>
		private ScrollbarVisibilityEnum _lastScrollbarVisibility;

		/// <summary>
		/// The number of cells in one third of the allocated scroller space
		/// </summary>
		private float _singleLoopGroupSize;

		/// <summary>
		/// The snap value to store before the user begins dragging
		/// </summary>
		private bool _snapBeforeDrag;

		/// <summary>
		/// The loop value to store before the user begins dragging.
		/// </summary>
		private bool _loopBeforeDrag;

		/// <summary>
		/// Flag to ignore the jump loop that gives the illusion
		/// of a continuous stream of cells
		/// </summary>
		private bool _ignoreLoopJump;

		/// <summary>
		/// The number of fingers that are dragging the ScrollRect.
		/// Used in OnBeginDrag and OnEndDrag
		/// </summary>
		private int _dragFingerCount;
#endregion PrivateFields

#region Mono
		/// <summary>
		/// Caches and initializes the scroller
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			Initialize();
		}

		protected virtual void Initialize()
		{
			if (_initialized) return;

			// cache some components
			_scrollRectTransform = base.GetComponent<RectTransform>();
			_container           = GetContainer();
			base.content         = _container;

#if UNITY_EDITOR
			bool isPrefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
			if (isPrefabStage) return;
#endif

			ContainerSetting();

			// cache the scrollbar if it exists
			_scrollbar = _scrollDirection == ScrollDirectionEnum.VERTICAL
				? base.verticalScrollbar
				: base.horizontalScrollbar;

			LayoutGroupSetting();

			// force the scroller to scroll in the direction we want
			base.horizontal = _scrollDirection == ScrollDirectionEnum.HORIZONTAL;
			base.vertical   = _scrollDirection == ScrollDirectionEnum.VERTICAL;

			CreatePadder();

			// set up the last values for updates
			_lastScrollRectSize      = ScrollRectSize;
			_lastLoop                = _loop;
			_lastScrollbarVisibility = _scrollbarVisibility;

			_initialized = true;
			RefreshCollection();
		}

		protected virtual RectTransform GetContainer()
		{
			GameObject go;
			if (base.content.IsUnityNull())
			{
				// Create a new active cell view container with a layout group
				go = new GameObject("Container", typeof(RectTransform));
				go.transform.SetParent(_scrollRectTransform);
			}
			else
			{
				go = base.content.gameObject;
			}

			if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
				go.GetOrAddComponent<VerticalLayoutGroup>();
			else
				go.GetOrAddComponent<HorizontalLayoutGroup>();

			return go.GetComponent<RectTransform>();
		}

		protected virtual void ContainerSetting()
		{
			if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
			{
				_container.anchorMin = new Vector2(0, 1);
				_container.anchorMax = Vector2.one;
				_container.pivot     = new Vector2(0.5f, 1f);
			}
			else
			{
				_container.anchorMin = Vector2.zero;
				_container.anchorMax = new Vector2(0, 1f);
				_container.pivot     = new Vector2(0, 0.5f);
			}

			_container.localPosition = Vector3.zero;
			_container.localRotation = Quaternion.identity;
			_container.localScale    = Vector3.one;
			_container.offsetMax     = Vector2.zero;
			_container.offsetMin     = Vector2.zero;

			base.content = _container;
		}

		protected virtual void LayoutGroupSetting()
		{
			_layoutGroup                        = _container.GetComponent<HorizontalOrVerticalLayoutGroup>();
			_layoutGroup.spacing                = _spacing;
			_layoutGroup.padding                = _padding;
			_layoutGroup.childAlignment         = TextAnchor.UpperLeft;
			_layoutGroup.childForceExpandHeight = true;
			_layoutGroup.childForceExpandWidth  = true;
		}

		protected virtual void CreatePadder()
		{
			var go = new GameObject("@First Padder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(_container, false);
			_firstPadder = go.GetComponent<LayoutElement>();

			go = new GameObject("@Last Padder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(_container, false);
			_lastPadder = go.GetComponent<LayoutElement>();

			// create the recycled cell view container
			go = new GameObject("@Recycled Cells", typeof(RectTransform));
			go.transform.SetParent(base.transform, false);
			_recycledCellViewContainer = go.GetComponent<RectTransform>();
			_recycledCellViewContainer.gameObject.SetActive(false);
		}

		protected override void Start()
		{
			RecyclableScrollType ??= GetRecyclableScrollController();
		}

		/// <summary>
		/// This event is fired when the user begins dragging on the scroller.
		/// We can disable looping or snapping while dragging if desired.
		/// <param name="data">The event data for the drag</param>
		/// </summary>
		public override void OnBeginDrag(PointerEventData data)
		{
			base.OnBeginDrag(data);
			_dragFingerCount++;
			if (_dragFingerCount > 1) return;

			// capture the snapping and set it to false if desired
			_snapBeforeDrag = _snapping;
			if (!_snapWhileDragging)
			{
				_snapping = false;
			}

			// capture the looping and set it to false if desired
			_loopBeforeDrag = _loop;
			if (!_loopWhileDragging)
			{
				_loop = false;
			}
		}

		/// <summary>
		/// This event is fired when the user ends dragging on the scroller.
		/// We can re-enable looping or snapping while dragging if desired.
		/// <param name="data">The event data for the drag</param>
		/// </summary>
		public override void OnEndDrag(PointerEventData data)
		{
			base.OnEndDrag(data);
			_dragFingerCount--;
			if (_dragFingerCount < 0) _dragFingerCount = 0;

			// reset the snapping and looping to what it was before the drag
			_snapping = _snapBeforeDrag;
			_loop     = _loopBeforeDrag;
		}

		protected virtual void Update()
		{
			if (_updateSpacing)
			{
				UpdateSpacing(_spacing, true);
				_reloadData = false;
			}

			if (_reloadData)
			{
				// if the reload flag is true, then reload the data
				ReloadData(reloadRequest: true);
			}

			// if the scroll rect size has changed and looping is on,
			// or the loop setting has changed, then we need to resize
			if ((_loop && !Mathf.Approximately(_lastScrollRectSize, ScrollRectSize)) || (_loop != _lastLoop))
			{
				Resize(true);
				_lastScrollRectSize = ScrollRectSize;

				_lastLoop = _loop;
			}

			// update the scroll bar visibility if it has changed
			if (_lastScrollbarVisibility != _scrollbarVisibility)
			{
				ScrollbarVisibility      = _scrollbarVisibility;
				_lastScrollbarVisibility = _scrollbarVisibility;
			}

			// determine if the scroller has started or stopped scrolling
			// and call the delegate if so.
			if (LinearVelocity != 0 && !IsScrolling)
			{
				IsScrolling = true;
				ScrollerScrollingChanged?.Invoke(this, true);
			}
			else if (LinearVelocity == 0 && IsScrolling)
			{
				IsScrolling = false;
				ScrollerScrollingChanged?.Invoke(this, false);
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Reacts to changes in the inspector
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();
			// if spacing changed, update it
			if (_initialized && !Mathf.Approximately(_spacing, _layoutGroup.spacing))
			{
				_updateSpacing = true;
			}
		}
#endif

		/// <summary>
		/// Fired at the end of the frame.
		/// </summary>
		protected override void LateUpdate()
		{
			base.LateUpdate();
			// if maxVelocity is not zero, we can set the speed cap based on the scroll direction
			if (!(_maxVelocity > 0)) return;
			Velocity = _scrollDirection == ScrollDirectionEnum.HORIZONTAL
				? new Vector2(Mathf.Clamp(Mathf.Abs(Velocity.x), 0, _maxVelocity) * Mathf.Sign(Velocity.x), Velocity.y)
				: new Vector2(Velocity.x, Mathf.Clamp(Mathf.Abs(Velocity.y), 0, _maxVelocity) * Mathf.Sign(Velocity.y));
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			// when the scroller is enabled, add a listener to the onValueChanged handler
			onValueChanged.AddListener(ScrollRectOnValueChanged);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			// when the scroller is disabled, remove the listener
			onValueChanged.RemoveListener(ScrollRectOnValueChanged);
		}
#endregion Mono

#region Private Methods
		/// <summary>
		/// This function will create an internal list of sizes and offsets to be used in all calculations.
		/// It also sets up the loop triggers and positions and initializes the cell views.
		/// </summary>
		/// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
		private void Resize(bool keepPosition)
		{
			// cache the original position
			var originalScrollPosition = _scrollPosition;

			// clear out the list of cell view sizes and create a new list
			_cellViewSizeArray.Clear();
			var offset = AddCellViewSizes();

			// if looping, we need to create three sets of size data
			if (_loop)
			{
				var cellCount = _cellViewSizeArray.Count;

				// if the cells don't entirely fill up the scroll area,
				// make some more size entries to fill it up
				if (offset < ScrollRectSize)
				{
					int additionalRounds = Mathf.CeilToInt(Mathf.CeilToInt(ScrollRectSize / offset) / 2.0f) * 2;
					DuplicateCellViewSizes(additionalRounds, cellCount);
					_loopFirstCellIndex = cellCount * (1 + (additionalRounds / 2));
				}
				else
				{
					_loopFirstCellIndex = cellCount;
				}

				_loopLastCellIndex = _loopFirstCellIndex + cellCount - 1;

				// create two more copies of the cell sizes
				DuplicateCellViewSizes(2, cellCount);
			}

			// calculate the offsets of each cell view
			CalculateCellViewOffsets();

			// set the size of the active cell view container based on the number of cell views there are and each of their sizes
			_container.sizeDelta = _scrollDirection == ScrollDirectionEnum.VERTICAL
				? new Vector2(_container.sizeDelta.x, _cellViewOffsetArray.Last() + _padding.top + _padding.bottom)
				: new Vector2(_cellViewOffsetArray.Last() + _padding.left + _padding.right, _container.sizeDelta.y);

			// if looping, set up the loop positions and triggers
			if (_loop)
			{
				_loopFirstScrollPosition = GetScrollPositionForCellViewIndex(_loopFirstCellIndex, CellViewPositionEnum.BEFORE) + (_spacing * 0.5f);
				_loopLastScrollPosition  = GetScrollPositionForCellViewIndex(_loopLastCellIndex, CellViewPositionEnum.AFTER) - ScrollRectSize + (_spacing * 0.5f);

				_loopFirstJumpTrigger = _loopFirstScrollPosition - ScrollRectSize;
				_loopLastJumpTrigger  = _loopLastScrollPosition + ScrollRectSize;
			}

			// create the visible cells
			ResetVisibleCellViews();

			// if we need to maintain our original position
			if (keepPosition)
			{
				ScrollPosition = originalScrollPosition;
			}
			else
			{
				if (_loop) ScrollPosition = _loopFirstScrollPosition;
				else ScrollPosition       = 0;
			}

			// set up the visibility of the scrollbar
			ScrollbarVisibility = _scrollbarVisibility;
		}

		/// <summary>
		/// Updates the spacing on the scroller
		/// </summary>
		/// <param name="spacing">new spacing value</param>
		/// <param name="reloadRequest">if the user wants to update the data, Scrolls may also need to be notified of the request.</param>
		private void UpdateSpacing(float spacing, bool reloadRequest)
		{
			if (reloadRequest)
				RecyclableScrollType?.OnReloadRequest();
			_updateSpacing       = false;
			_layoutGroup.spacing = spacing;
			ReloadData(NormalizedScrollPosition);
		}

		/// <summary>
		/// Creates a list of cell view sizes for faster access
		/// </summary>
		/// <returns></returns>
		private float AddCellViewSizes()
		{
			var offset = 0f;
			_singleLoopGroupSize = 0;
			// add a size for each row in our data based on how many the delegate tells us to create
			for (var i = 0; i < NumberOfCells; i++)
			{
				// add the size of this cell based on what the delegate tells us to use. Also add spacing if this cell isn't the first one
				_cellViewSizeArray.Add(_recyclableScrollType.GetCellViewSize(i) + (i == 0 ? 0 : _layoutGroup.spacing));
				_singleLoopGroupSize += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
				offset               += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
			}

			return offset;
		}

		/// <summary>
		/// Create a copy of the cell view sizes. This is only used in looping
		/// </summary>
		/// <param name="numberOfTimes">How many times the copy should be made</param>
		/// <param name="cellCount">How many cells to copy</param>
		private void DuplicateCellViewSizes(int numberOfTimes, int cellCount)
		{
			for (var i = 0; i < numberOfTimes; i++)
			for (var j = 0; j < cellCount; j++)
				_cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? _layoutGroup.spacing : 0));
		}

		/// <summary>
		/// Calculates the offset of each cell, accumulating the values from previous cells
		/// </summary>
		private void CalculateCellViewOffsets()
		{
			_cellViewOffsetArray.Clear();
			var offset = 0f;
			for (var i = 0; i < _cellViewSizeArray.Count; i++)
			{
				offset += _cellViewSizeArray[i];
				_cellViewOffsetArray.Add(offset);
			}
		}

		/// <summary>
		/// Get a recycled cell with a given identifier if available
		/// </summary>
		/// <param name="cellPrefab">The prefab to check for</param>
		/// <returns></returns>
		private EnhancedScrollerCellView GetRecycledCellView(EnhancedScrollerCellView cellPrefab)
		{
			for (var i = 0; i < _recycledCellViews.Count; i++)
			{
				if (_recycledCellViews[i].cellIdentifier != cellPrefab.cellIdentifier) continue;
				// the cell view was found, so we use this recycled one.
				// we also remove it from the recycled list
				var cellView = _recycledCellViews.RemoveAt(i);
				return cellView;
			}

			return null;
		}

		/// <summary>
		/// This sets up the visible cells, adding and recycling as necessary
		/// </summary>
		private void ResetVisibleCellViews()
		{
			// calculate the range of the visible cells
			CalculateCurrentActiveCellRange(out var startIndex, out var endIndex);

			// go through each previous active cell and recycle it if it no longer falls in the range
			var i                    = 0;
			var remainingCellIndices = new SmallList<int>();
			while (i < _activeCellViews.Count)
			{
				if (_activeCellViews[i].cellIndex < startIndex || _activeCellViews[i].cellIndex > endIndex)
				{
					RecycleCell(_activeCellViews[i]);
				}
				else
				{
					// this cell index falls in the new range, so we add its
					// index to the reusable list
					remainingCellIndices.Add(_activeCellViews[i].cellIndex);
					i++;
				}
			}

			if (remainingCellIndices.Count == 0)
			{
				// there were no previous active cells remaining,
				// this list is either brand new, or we jumped to
				// an entirely different part of the list.
				// just add all the new cell views
				for (i = startIndex; i <= endIndex; i++)
					AddCellView(i, ListPositionEnum.LAST);
			}
			else
			{
				// we are able to reuse some of the previous
				// cell views

				// first add the views that come before the
				// previous list, going backward so that the
				// new views get added to the front
				for (i = endIndex; i >= startIndex; i--)
					if (i < remainingCellIndices.First())
						AddCellView(i, ListPositionEnum.FIRST);

				// next add teh views that come after the
				// previous list, going forward and adding
				// at the end of the list
				for (i = startIndex; i <= endIndex; i++)
					if (i > remainingCellIndices.Last())
						AddCellView(i, ListPositionEnum.LAST);
			}

			// update the start and end indices
			_activeCellViewsStartIndex = startIndex;
			_activeCellViewsEndIndex   = endIndex;

			// adjust the padding elements to offset the cell views correctly
			SetPadders();
		}

		/// <summary>
		/// Recycles all the active cells
		/// </summary>
		private void RecycleAllCells()
		{
			while (_activeCellViews.Count > 0) RecycleCell(_activeCellViews[0]);
			_activeCellViewsStartIndex = 0;
			_activeCellViewsEndIndex   = 0;
		}

		/// <summary>
		/// Recycles one cell view
		/// </summary>
		/// <param name="cellView"></param>
		private void RecycleCell(EnhancedScrollerCellView cellView)
		{
			if (CellViewWillRecycle != null) CellViewWillRecycle(cellView);

			// remove the cell view from the active list
			_activeCellViews.Remove(cellView);

			// add the cell view to the recycled list
			_recycledCellViews.Add(cellView);

			// move the GameObject to the recycled container
			//cellView.transform.SetParent(_recycledCellViewContainer);

			// deactivate the cellview (this is more efficient than moving the to a new parent like the above commented lines)
			cellView.transform.gameObject.SetActive(false);

			// reset the cellView's properties
			cellView.dataIndex = 0;
			cellView.cellIndex = 0;
			cellView.active    = false;

			if (CellViewVisibilityChanged != null) CellViewVisibilityChanged(cellView);
		}

		/// <summary>
		/// Creates a cell view, or recycles if it can
		/// </summary>
		/// <param name="cellIndex">The index of the cell view</param>
		/// <param name="listPosition">Whether to add the cell to the beginning or the end</param>
		private void AddCellView(int cellIndex, ListPositionEnum listPosition)
		{
			if (NumberOfCells == 0) return;

			// get the dataIndex. Modulus is used in case of looping so that the first set of cells are ignored
			var dataIndex = cellIndex % NumberOfCells;
			// request a cell view from the delegate
			var cellView = _recyclableScrollType.GetCellView(dataIndex, cellIndex);

			// set the cell's properties
			cellView.cellIndex = cellIndex;
			cellView.dataIndex = dataIndex;
			cellView.active    = true;

			// add the cell view to the active container
			cellView.transform.SetParent(_container, false);
			cellView.transform.localScale = Vector3.one;

			// add a layout element to the cellView
			LayoutElement layoutElement = cellView.gameObject.GetOrAddComponent<LayoutElement>();

			// set the size of the layout element
			if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
				layoutElement.minHeight = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);
			else
				layoutElement.minWidth = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);

			// add the cell to the active list
			if (listPosition == ListPositionEnum.FIRST)
				_activeCellViews.AddStart(cellView);
			else
				_activeCellViews.Add(cellView);

			// set the hierarchy position of the cell view in the container
			if (listPosition == ListPositionEnum.LAST)
				cellView.transform.SetSiblingIndex(_container.childCount - 2);
			else if (listPosition == ListPositionEnum.FIRST)
				cellView.transform.SetSiblingIndex(1);

			// call the visibility change delegate if available
			if (CellViewVisibilityChanged != null) CellViewVisibilityChanged(cellView);
		}

		/// <summary>
		/// This function adjusts the two padders that control the first cell view's
		/// offset and the overall size of each cell.
		/// </summary>
		private void SetPadders()
		{
			if (NumberOfCells == 0) return;

			// calculate the size of each padder
			var firstSize = _cellViewOffsetArray[_activeCellViewsStartIndex] - _cellViewSizeArray[_activeCellViewsStartIndex];
			var lastSize  = _cellViewOffsetArray.Last() - _cellViewOffsetArray[_activeCellViewsEndIndex];

			if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
			{
				// set the first padder and toggle its visibility
				_firstPadder.minHeight = firstSize;
				_firstPadder.gameObject.SetActive(_firstPadder.minHeight > 0);

				// set the last padder and toggle its visibility
				_lastPadder.minHeight = lastSize;
				_lastPadder.gameObject.SetActive(_lastPadder.minHeight > 0);
			}
			else
			{
				// set the first padder and toggle its visibility
				_firstPadder.minWidth = firstSize;
				_firstPadder.gameObject.SetActive(_firstPadder.minWidth > 0);

				// set the last padder and toggle its visibility
				_lastPadder.minWidth = lastSize;
				_lastPadder.gameObject.SetActive(_lastPadder.minWidth > 0);
			}
		}

		/// <summary>
		/// This function is called if the scroller is scrolled, updating the active list of cells
		/// </summary>
		private void RefreshActive()
		{
			//_refreshActive = false;
			// if looping, check to see if we scrolled past a trigger
			if (_loop && !_ignoreLoopJump)
			{
				Vector2 velocity;
				if (_scrollPosition < _loopFirstJumpTrigger)
				{
					velocity       = base.velocity;
					ScrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - _scrollPosition) + _spacing;
					base.velocity  = velocity;
				}
				else if (_scrollPosition > _loopLastJumpTrigger)
				{
					velocity       = base.velocity;
					ScrollPosition = _loopFirstScrollPosition + (_scrollPosition - _loopLastJumpTrigger) - _spacing;
					base.velocity  = velocity;
				}
			}

			// get the range of visible cells
			CalculateCurrentActiveCellRange(out var startIndex, out var endIndex);

			// if the index hasn't changed, ignore and return
			if (startIndex == _activeCellViewsStartIndex && endIndex == _activeCellViewsEndIndex) return;

			// recreate the visible cells
			ResetVisibleCellViews();
		}

		/// <summary>
		/// Determines which cells can be seen
		/// </summary>
		/// <param name="startIndex">The index of the first cell visible</param>
		/// <param name="endIndex">The index of the last cell visible</param>
		private void CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			startIndex = 0;
			endIndex   = 0;

			// get the positions of the scroller
			var startPosition = _scrollPosition - LookAheadBefore;
			var endPosition   = _scrollPosition + (_scrollDirection == ScrollDirectionEnum.VERTICAL ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width) + LookAheadAfter;

			// calculate each index based on the positions
			startIndex = GetCellViewIndexAtPosition(startPosition);
			endIndex   = GetCellViewIndexAtPosition(endPosition);
		}

		/// <summary>
		/// Gets the index of a cell at a given position based on a subset range.
		/// This function uses a recursive binary sort to find the index faster.
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <param name="startIndex">The first index of the range</param>
		/// <param name="endIndex">The last index of the rnage</param>
		/// <returns></returns>
		private int GetCellIndexAtPosition(float position, int startIndex, int endIndex)
		{
			// if the range is invalid, then we found our index, return the start index
			if (startIndex >= endIndex) return startIndex;

			// determine the middle point of our binary search
			var middleIndex = (startIndex + endIndex) / 2;

			// if the middle index is greater than the position, then search the last
			// half of the binary tree, else search the first half
			var pad = _scrollDirection == ScrollDirectionEnum.VERTICAL ? _padding.top : _padding.left;
			return (_cellViewOffsetArray[middleIndex] + pad) >= (position + (pad == 0 ? 0 : 1.00001f))
				? GetCellIndexAtPosition(position, startIndex, middleIndex)
				: GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
		}

		/// <summary>
		/// Handler for when the scroller changes value
		/// </summary>
		/// <param name="val">The scroll rect's value</param>
		private void ScrollRectOnValueChanged(Vector2 val)
		{
			// set the internal scroll position
			if (_scrollDirection == ScrollDirectionEnum.VERTICAL)
				_scrollPosition = (1f - val.y) * ScrollSize;
			else
				_scrollPosition = val.x * ScrollSize;
			//_refreshActive = true;
			_scrollPosition = Mathf.Clamp(_scrollPosition, 0, ScrollSize);

			// call the handler if it exists
			ScrollerScrolled?.Invoke(this, val, _scrollPosition);

			// if the snapping is turned on, handle it
			if (_snapping && !_snapJumping)
			{
				// if the speed has dropped below the threshhold velocity
				if (Mathf.Abs(LinearVelocity) <= _snapVelocityThreshold && LinearVelocity != 0)
				{
					// Make sure the scroller is not on the boundary if not looping
					var normalized = NormalizedScrollPosition;
					if (_loop || (!_loop && normalized > 0 && normalized < 1.0f))
					{
						// Call the snap function
						Snap();
					}
				}
			}

			RefreshActive();
		}

		/// <summary>
		/// This is fired by the tweener when the snap tween is completed
		/// </summary>
		private void SnapJumpComplete()
		{
			// reset the snap jump to false and restore the inertia state
			_snapJumping = false;
			base.inertia = _snapInertia;

			EnhancedScrollerCellView cellView = null;
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				if (_activeCellViews[i].dataIndex != _snapDataIndex) continue;
				cellView = _activeCellViews[i];
				break;
			}

			// fire the scroller snapped delegate
			ScrollerSnapped?.Invoke(this, _snapCellViewIndex, _snapDataIndex, cellView);
		}
#endregion Private Methods

#region Tweening
		private float _tweenTimeLeft;

		/// <summary>
		/// Moves the scroll position over time between two points given an easing function. When the
		/// tween is complete it will fire the jumpComplete delegate.
		/// </summary>
		/// <param name="easeType">The type of easing to use</param>
		/// <param name="time">The amount of time to interpolate</param>
		/// <param name="start">The starting scroll position</param>
		/// <param name="end">The ending scroll position</param>
		/// <param name="tweenComplete">The action to fire when the tween is complete</param>
		/// <param name="forceCalculateRange">RefreshActive</param>
		/// <returns></returns>
		private async UniTask TweenPosition(Ease easeType, float time, float start, float end, Action tweenComplete, bool forceCalculateRange)
		{
			if (time != 0)
			{
				// zero out the velocity
				base.velocity = Vector2.zero;

				// fire the delegate for the tween start
				IsTweening = true;
				if (ScrollerTweeningChanged != null) ScrollerTweeningChanged(this, true);

				_tweenTimeLeft = 0;
				// while the tween has time left, use an easing function
				while (_tweenTimeLeft < time)
				{
					// set the scroll position to the tweened position
					ScrollPosition = DOVirtual.EasedValue(start, end, _tweenTimeLeft / time, easeType);
					;

					// increase the time elapsed
					_tweenTimeLeft += Time.unscaledDeltaTime;

					await UniTask.Yield();
				}
			}

			// the time has expired, so we make sure the final scroll position
			// is the actual end position.
			ScrollPosition = end;

			if (forceCalculateRange)
			{
				RefreshActive();
			}

			// the tween jump is complete, so we fire the delegate
			tweenComplete?.Invoke();

			// fire the delegate for the tween ending
			IsTweening = false;
			ScrollerTweeningChanged?.Invoke(this, false);
		}
#endregion Tweening
	}
}
