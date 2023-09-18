/*===============================================================
* Product:    Com2Verse
* File Name:  INotifyCollectionChanged.cs
* Developer:  tlghks1009
* Date:       2022-04-01 16:25
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections;

namespace Com2Verse.UI
{
	using CollectionIndex = Int32;
	using CollectionList = IList;


	public enum eNotifyCollectionChangedAction
	{
		ADD,
		ADD_RANGE,
		REMOVE,
		REPLACE,
		MOVE,
		RESET,
		DESTROY_ALL,
		LOAD_COMPLETE,
	}


	public interface INotifyCollectionChanged
	{
		void AddEvent(Action<eNotifyCollectionChangedAction, CollectionList, CollectionIndex> eventHandler);

		void RemoveEvent(Action<eNotifyCollectionChangedAction, CollectionList, CollectionIndex> eventHandler);

		IEnumerable ItemsSource { get; }

		int CollectionCount { get; }
	}
}
