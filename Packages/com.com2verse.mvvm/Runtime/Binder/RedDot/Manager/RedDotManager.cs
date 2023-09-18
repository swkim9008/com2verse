/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotManager.cs
* Developer:	NGSG
* Date:			2023-04-18 12:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.UI;
using Binder = System.Reflection.Binder;

namespace Com2Verse
{
	public class RedDotData : IEquatable<RedDotData>
	{
		public string BadgeType { get; private set; }
		public int    Id        { get; private set; }

		public RedDotData(string badgeType, int id, bool isRedDot = false)
		{
			BadgeType = badgeType;
			Id        = id;
		}

		public bool Equals(RedDotData other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return BadgeType == other.BadgeType && Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((RedDotData)obj);
		}

		public override int GetHashCode() => HashCode.Combine(BadgeType, Id);
	}
	
	public sealed class RedDotManager : Singleton<RedDotManager>
	{
		private readonly int MAX_COUNT = 99;
		//private readonly Dictionary<ViewModel, bool> _viewModelChecker = new();

		// 최하단 리프만 데이타 처리하고 그 상위 부모는 모두 리프의 값을 가지고 결정한다
		private Dictionary<string, List<RedDotData>>     _dicRedDotData = new();
		private Dictionary<string, List<CollectionItem>> _dicCollection = new();
		
		private RedDotManager()
		{
		}

		private void RunBind(CollectionItem collectionItem, ViewModel viewModel)
		{
			//if (!_viewModelChecker.TryGetValue(viewModel, out var duplicate))
			{
				collectionItem.ViewModel = viewModel;
				collectionItem.Index = GetBadgeModelView(collectionItem).RedDot.Id;
				collectionItem.BindStaticRedDot(); //레드닷 전용 바인더
				//_viewModelChecker.Add(viewModel, true);
				//OnItemBinded(collectionItem);
			}
		}
		
		public void Clear(bool isClearData = false)
		{
			if(isClearData)
				ClearAllRedDotData();
			ClearAllCollectItem();
		}

		private void ClearCollectItem(string type)
		{
			List<CollectionItem> list = FindCollectionList(type);
			list?.Clear();
		}

		private void ClearAllCollectItem()
		{
			Dictionary<string, List<CollectionItem>>.Enumerator iter = _dicCollection.GetEnumerator();
			while (iter.MoveNext() == true)
				iter.Current.Value.Clear();

			_dicCollection.Clear();
		}
		
		public void CreateCollectionItem(GameObject go, ViewModel vm)
		{
			bool bAdd = true;
			CollectionItem collectionItem = FindCollectionItem(vm);
			if (collectionItem == null)
			{
				bAdd           = false;
				collectionItem = new CollectionItem(go);
				RunBind(collectionItem, vm);
			}
			else
			{
				RunBind(collectionItem, collectionItem.ViewModel);
			}
			
			if(bAdd == false)
				AddCollectionItem(collectionItem);
		}

		private void AddCollectionItem(CollectionItem item)
		{
			IRedDot redDotViewModel = GetBadgeModelView(item);
			BadgeTreeNode <string> node = BadgeHierarchyTable.Instance.GetNode(redDotViewModel.RedDot.BadgeType);
			string parentBadgeType = node.Parent.Data;
			
			if (_dicCollection.ContainsKey(parentBadgeType))
			{
				_dicCollection[parentBadgeType].Add(item);
			}
			else
			{
				List<CollectionItem> list = new List<CollectionItem>();
				list.Add(item);

				_dicCollection.Add(parentBadgeType, list);
			}
		}

		public void RemoveCollectionItem(CollectionItem item)
		{
			IRedDot redDotViewModel = GetBadgeModelView(item);
			BadgeTreeNode<string> node = BadgeHierarchyTable.Instance.GetNode(redDotViewModel.RedDot.BadgeType);
			string parentBadgeType = node.Parent.Data;

			if (_dicCollection.ContainsKey(parentBadgeType))
				_dicCollection[parentBadgeType].Remove(item);
		}

		private void SetRedDot(CollectionItem item, bool val)
		{
			IRedDot redDotViewModel = GetBadgeModelView(item);
			if (redDotViewModel != null)
				redDotViewModel.IsRedDot = val;
		}

		private CollectionItem FindCollectionItem(RedDotData data)
		{
			Dictionary<string, List<CollectionItem>>.Enumerator iter = _dicCollection.GetEnumerator();
			while (iter.MoveNext() == true)
			{
				// 키를 가지고 value 를 찾아야 한다.
				foreach (var item in iter.Current.Value)
				{
					IRedDot redDotViewModel = GetBadgeModelView(item);
					if(data.Equals(redDotViewModel.RedDot))
						return item;
				}
			}

			return null;
		}

		private CollectionItem FindCollectionItem(string ty)
		{
			if (ty == BadgeHierarchyTable.ROOT)
				return null;
			
			Dictionary<string, List<CollectionItem>>.Enumerator iter = _dicCollection.GetEnumerator();
			while (iter.MoveNext() == true)
			{
				// 키를 가지고 value 를 찾아야 한다.
				foreach (var item in iter.Current.Value)
				{
					IRedDot redDotViewModel = GetBadgeModelView(item);
					if(redDotViewModel != null && redDotViewModel.RedDot.BadgeType == ty)
						return item;
				}
			}

			return null;
		}

		public CollectionItem FindCollectionItem(ViewModel vm)
		{
			IRedDot argRedDotViewModel = vm as IRedDot;
			if (argRedDotViewModel == null)
				return null;

			Dictionary<string, List<CollectionItem>>.Enumerator iter = _dicCollection.GetEnumerator();
			while (iter.MoveNext() == true)
			{
				// 키를 가지고 value 를 찾아야 한다.
				foreach (var item in iter.Current.Value)
				{
					IRedDot redDotViewModel = GetBadgeModelView(item);
					if(argRedDotViewModel.RedDot.Equals(redDotViewModel.RedDot))
						return item;
				}
			}

			return null;
		}
		
		private List<CollectionItem> FindCollectionList(string type)
		{
			if (_dicCollection.ContainsKey(type))
				return _dicCollection[type];

			return null;
		}

		private IRedDot GetBadgeModelView(CollectionItem collectionItem)
		{
			IRedDot redDotViewModel = collectionItem.ViewModel as IRedDot;
			return redDotViewModel == null ? null : redDotViewModel;
		}

		private void AddRedDotData(RedDotData redDotData)
		{
			//redDotData.IsRedDot = true;
			string type = redDotData.BadgeType;
			if (_dicRedDotData.ContainsKey(type))
			{
				if (_dicRedDotData[type].Contains(redDotData))
					Debug.LogError($"redDotData id - {redDotData.Id} 가 이미 존재함");
				else
					_dicRedDotData[type].Add(redDotData);
			}
			else
			{
				List<RedDotData> list = new List<RedDotData>();
				list.Add(redDotData);

				_dicRedDotData.Add(type, list);
			}
		}

		private RedDotData FindRedDotData(RedDotData redDotData)
		{
			string type = redDotData.BadgeType;
			if (_dicRedDotData.ContainsKey(type))
				return _dicRedDotData[type].Find(item => item.Id == redDotData.Id);

			return null;
		}
		
		private List<RedDotData> FindRedDotData(string type)
		{
			if (_dicRedDotData.ContainsKey(type))
				return _dicRedDotData[type];

			return null;
		}

		private void RemoveRedDotData(RedDotData redDotData)
		{
			//redDotData.IsRedDot = false;
			string type = redDotData.BadgeType;
			if (_dicRedDotData.ContainsKey(type))
			{
				RedDotData rdd = FindRedDotData(redDotData);
				if(rdd != null)
					_dicRedDotData[type].Remove(rdd);
			}
		}

		private void ClearRedDotData(string type)
		{
			if (_dicRedDotData.ContainsKey(type))
				_dicRedDotData[type].Clear();
		}
		
		private void ClearAllRedDotData()
		{
			Dictionary<string, List<RedDotData>>.Enumerator iter = _dicRedDotData.GetEnumerator();
			while (iter.MoveNext() == true)
				iter.Current.Value.Clear();
			_dicRedDotData.Clear();
		}

		public int GetBadgeCount(string type)
		{
			// type 타입의 모든 자식들의 개수를 더해준다
			List<RedDotData> list = FindRedDataChild(type);
			if (list != null)
				return list.Count < MAX_COUNT ? list.Count : MAX_COUNT;

			return 0;
		}

		private void SetRedDotCount(string type)
		{
			CollectionItem item = FindCollectionItem(type);
			if (item != null)
				SetRedDotCount(item, GetBadgeCount(type));
		}

		private void SetRedDotCount(CollectionItem item, int count)
		{
			IRedDot redDotViewModel = GetBadgeModelView(item);
			if (redDotViewModel != null)
				redDotViewModel.BadgeCount = count > 0 ? count.ToString() : string.Empty;
		}

		public void AddBadge(RedDotData redDotData)
		{
			if (redDotData == null)
				return;

			// 데이타 추가
			AddRedDotData(redDotData);

			// 추가된 데이타 기준으로 연결된 부모 활성화 해준다
			ActiveRedDot(redDotData);
		}
		public void RemoveBadge(RedDotData redDotData)
		{
			if (redDotData == null)
				return;

			// 데이타 삭제
			RemoveRedDotData(redDotData);
			
			// 삭제된 데이타 기준으로 업데이트
			DeActiveRedDot(redDotData);

			SetRedDotCount(redDotData.BadgeType);
		}

		private void ActiveRedDot(RedDotData redDotData)
		{
			CollectionItem item = FindCollectionItem(redDotData);
			if (item != null)
				SetRedDot(item, true);

			// 연결된 부모들의 타입을 찾아서 모두 배찌 활성화한다
			List<string> list = new List<string>();
			BadgeHierarchyTable.Instance.FindTableParentList(redDotData.BadgeType, ref list);
			foreach (var parentType in list)
				ActiveParentRedDot(parentType, true);
		}
		private void ActiveParentRedDot(string type, bool val)
		{
			CollectionItem item = FindCollectionItem(type);
			if (item != null)
			{
				SetRedDot(item, val);
				SetRedDotCount(type);
			}
		}

		private void DeActiveRedDot(RedDotData redDotData)
		{
			// 차일드에서 부모순으로 전달해야 한다
			CollectionItem item = FindCollectionItem(redDotData);
			if (item != null)
				SetRedDot(item, false);

			SetParentsRedDot(redDotData.BadgeType);
		}

		public void RemoveBadgeType(string type)
		{
			List<RedDotData> list = FindRedDotData(type);
			if (list != null)
			{
				foreach (var rdd in list)
				{
					//rdd.IsRedDot = false;
					CollectionItem ci = FindCollectionItem(rdd);
					if (ci != null)
					{
						SetRedDot(ci, false);
					}
				}
				ClearRedDotData(type);
			}
			
			SetParentsRedDot(type);
		}
		
		public void Notify(string type)
		{
			// 리프에 전달
			List<RedDotData> list = FindRedDotData(type);
			if (list != null)
			{
				foreach (var rdd in list)
				{
					CollectionItem ci = FindCollectionItem(rdd);
					if (ci != null)
						SetRedDot(ci, true);
				}
			}

			// 부모에 전달
			SetParentsRedDot(type);
		}

		private void SetParentsRedDot(string type)
		{
			// 부모에 전달
			List<string> listParent = new List<string>();
			BadgeHierarchyTable.Instance.FindTableParentList(type, ref listParent);
			foreach (var parentType in listParent)
				UpdateParentRedDot(parentType);
		}
		private void UpdateParentRedDot(string type)
		{
			CollectionItem parentItem = FindCollectionItem(type);
			if (parentItem != null)
			{
				// 부모를 찾아서 설정할때 자식의 데이타 값을 참고해서 설정하도록 한다
				List<RedDotData> list = FindRedDataChild(type);
				if (list != null && list.Count > 0)
					SetRedDot(parentItem, true);
				else
					SetRedDot(parentItem, false);

				SetRedDotCount(type);
			}
		}
		private List<RedDotData> FindRedDataChild(string type)
		{
			List<RedDotData> resultRdd = new List<RedDotData>();
			List<string>     listChild = new List<string>();
			BadgeHierarchyTable.Instance.FindTableChildList(type, ref listChild);
			foreach (var child in listChild)
			{
				List<RedDotData> list = FindRedDotData(child);
				if (list != null && list.Count > 0)
				{
					resultRdd.AddRange(list);
				}
			}

			return resultRdd;
		}
		
		public void NotifyAll()
		{
			// 리프에 전달
			if (_dicRedDotData.Count > 0)
			{
				// 배찌 데이타 하단들만 값을 설정한다
				foreach (var lRdd in _dicRedDotData)
				{
					string type = lRdd.Key;
					foreach (var rdd in lRdd.Value)
					{
						CollectionItem ci = FindCollectionItem(rdd);
						if (ci != null)
							SetRedDot(ci, true);
					}
				}

				// 부모에 전달
				foreach (var lRdd in _dicRedDotData)
				{
					// 연결된 부모들의 타입을 찾는다
					foreach (var rdd in lRdd.Value)
						SetParentsRedDot(rdd.BadgeType);
				}
			}
		}

	}
}
