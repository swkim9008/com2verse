/*===============================================================
* Product:		Com2Verse
* File Name:	MapObjectManagerViewModel.cs
* Developer:	eugene9721
* Date:			2022-08-18 15:46
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Com2Verse.Network;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Network")]
	public sealed class ActiveObjectManagerViewModel : CollectionManagerViewModel<long, ActiveObjectViewModel>
	{
		public enum eDisplayType
		{
			ALL,
			OWN,
			OTHERS,
			NONE,
			CUSTOM,
		}

#region Fields/Properties
		private bool _enable = true;

		public bool Enable
		{
			get => _enable;
			set
			{
				if (_enable == value)
					return;

				_enable = value;

				if (!value) Clear();
				else AddAllActiveObject();
			}
		}

		private eDisplayType _type = eDisplayType.ALL;

		public eDisplayType DisplayType
		{
			get => _type;
			set
			{
				if (_type == value)
					return;

				if (value != eDisplayType.CUSTOM)
					IsCustomDisplayFunc = null;

				_type = value;

				RefreshItemViewModels();
			}
		}

		public Func<ActiveObject, bool>? IsCustomDisplayFunc { get; set; }
#endregion Fields/Properties

#region Initialize
		public ActiveObjectManagerViewModel()
		{
			MapController.Instance.OnMapObjectCreate += OnMapObjectAdded;
			MapController.Instance.OnMapObjectRemove += OnMapObjectRemoved;

			if (MapController.Instance.ActiveObjects != null)
				foreach (var baseMapObject in MapController.Instance.ActiveObjects.Values)
					RegisterObserverMapObject(baseMapObject);
		}
#endregion Initialize

#region Event Handler
		private void OnMapObjectAdded(Protocols.ObjectState? objectState, BaseMapObject? baseMapObject)
		{
			RegisterObserverMapObject(baseMapObject);
		}

		private void OnMapObjectRemoved(BaseMapObject? baseMapObject)
		{
			DeleteObserverRemoveMapObject(baseMapObject);
		}

		private void RegisterObserverMapObject(BaseMapObject? baseMapObject)
		{
			if (baseMapObject is not ActiveObject activeObject)
				return;

			activeObject.BecomeOccluded   += OnBecomeOccluded;
			activeObject.BecomeUnoccluded += OnBecomeUnoccluded;

			AddActiveObject(activeObject);
		}

		private void DeleteObserverRemoveMapObject(BaseMapObject? baseMapObject)
		{
			if (baseMapObject is not ActiveObject activeObject)
				return;

			activeObject.BecomeOccluded   -= OnBecomeOccluded;
			activeObject.BecomeUnoccluded -= OnBecomeUnoccluded;

			RemoveActiveObject(activeObject);
		}
#endregion Event Handler

#region Add/Remove Object
		/// <summary>
		/// 뷰모델 컬렉션에 ActiveObject를 추가합니다.<br/>
		/// 4가지 조건 모두 만족시 추가<br/>
		/// 1) 구독한 셀 안에 있는 오브젝트<br/>
		/// 2) 메인 카메라에 렌더링중인 아바타(head bone 기준으로 설정한 bounding sphere가 카메라 프러스텀에 포함되어야 하며, 컬링되는 경우 보여지지 않음)<br/>
		/// 3) 유저 아바타와 해당 ActiveObject의 거리가 BoundingDistance보다 작은 경우<br/>
		/// 4) 현재 AvatarHud 설정값에 따라 추가 여부 최종 결정
		/// </summary>
		/// <param name="activeObject">추가할 activeObject</param>
		private void AddActiveObject(ActiveObject activeObject)
		{
			if (!_enable || Contains(activeObject.ObjectID) || activeObject.IsOccluded)
				return;

			if (!IsMatchingCondition(activeObject))
				return;

			Add(activeObject.ObjectID, new ActiveObjectViewModel(activeObject));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsMatchingCondition(ActiveObject activeObject)
		{
			return _type switch
			{
				eDisplayType.ALL    => true,
				eDisplayType.OWN    => activeObject.IsMine,
				eDisplayType.OTHERS => !activeObject.IsMine,
				eDisplayType.NONE   => false,
				eDisplayType.CUSTOM => IsCustomDisplayFunc?.Invoke(activeObject) == true,
				_                   => false,
			};
		}

		/// <summary>
		/// 뷰모델 컬렉션에서 ActiveObject를 제거합니다.<br/>
		/// 제거되는 케이스: 해당 오브젝트게 구독한 셀에서 없어진 경우
		/// </summary>
		/// <param name="activeObject">제거할 activeObject</param>
		private void RemoveActiveObject(ActiveObject activeObject)
		{
			Remove(activeObject.ObjectID);
		}

		private void AddAllActiveObject()
		{
			if (MapController.Instance.ActiveObjects == null) return;

			foreach (var baseMapObject in MapController.Instance.ActiveObjects.Values)
			{
				if (baseMapObject is not ActiveObject activeObject)
					continue;

				AddActiveObject(activeObject);
			}
		}

		public void RefreshItemViewModels()
		{
			Clear();
			AddAllActiveObject();
		}
#endregion Add/Remove Object

#region CullingGroup
		private void OnBecomeUnoccluded(MapObject? mapObject)
		{
			if (!User.InstanceExists) return;

			if (mapObject is not ActiveObject activeObject)
				return;

			AddActiveObject(activeObject);

			User.Instance.UserFunctionUI?.AddNearByObject(activeObject);
		}

		private void OnBecomeOccluded(MapObject? mapObject)
		{
			if (!User.InstanceExists) return;

			if (mapObject is not ActiveObject activeObject)
				return;

			User.Instance.UserFunctionUI?.RemoveNearByObject(activeObject);
		}
#endregion CullingGroup

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				MapController.Instance.OnMapObjectCreate -= OnMapObjectAdded;
				MapController.Instance.OnMapObjectRemove -= OnMapObjectRemoved;
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
