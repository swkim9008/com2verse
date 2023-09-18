/*===============================================================
* Product:		Com2Verse
* File Name:	BinderAccessors.cs
* Developer:	tlghks1009
* Date:			2023-01-11 15:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

namespace Com2Verse.UI
{
	internal static class BindablePropertyDispatcher
	{
		private static readonly Dictionary<object, List<Binder>> SubscribedBinders = new();

		private static bool _isInitialized;

		public static void Subscribe(object source, Binder binder)
		{
			if (source is ViewModel)
			{
				if (!_isInitialized)
				{
					Initialize();
				}

				if (SubscribedBinders.TryGetValue(source, out var binderList))
				{
					binderList.Add(binder);
					return;
				}

				binderList = new List<Binder>() {binder};

				SubscribedBinders.Add(source, binderList);
			}
		}


		public static void Unsubscribe(object source, Binder binder)
		{
			if (source is ViewModel)
			{
				if (!_isInitialized)
				{
					return;
				}

				if (SubscribedBinders.TryGetValue(source, out var binderList))
				{
					binderList.Remove(binder);

					if (binderList.Count == 0)
						SubscribedBinders.Remove(source);
				}
			}
		}


		private static bool TryGetValue(object key, out List<Binder> binder) => SubscribedBinders.TryGetValue(key, out binder);


		public static void NotifyPropertyChanged<T>(object sender, string propertyName, T value) where T : unmanaged, IConvertible
		{
			if (TryGetValue(sender, out var binderList))
			{
				for (int binderIdx = 0; binderIdx < binderList.Count; binderIdx++)
				{
					binderList[binderIdx].NotifyPropertyChanged(propertyName, value);
				}
			}
		}


		public static void NotifyPropertyChanged(object sender, string propertyName, [CanBeNull] object value)
		{
			if (TryGetValue(sender, out var binderList))
			{
				for (int binderIdx = 0; binderIdx < binderList.Count; binderIdx++)
				{
					binderList[binderIdx].NotifyPropertyChanged(propertyName, value);
				}
			}
		}

		public static void NotifyPropertyChanged(object sender, string propertyName)
		{
			if (TryGetValue(sender, out var binderList))
			{
				for (int binderIdx = 0; binderIdx < binderList.Count; binderIdx++)
				{
					binderList[binderIdx].NotifyPropertyChanged(propertyName);
				}
			}
		}

		private static void Initialize()
		{
			SceneManager.sceneUnloaded += OnSceneUnloaded;

			_isInitialized = true;
		}

		private static void OnSceneUnloaded(Scene scene) => Cleanup();

		private static void Cleanup()
		{
			var destroyedBinders = new Queue<Binder>();

			foreach (var kvp in SubscribedBinders)
			{
				var key = kvp.Key;
				var value = SubscribedBinders[key];

				foreach (var binder in value)
				{
					if (!binder)
						destroyedBinders.Enqueue(binder);
				}
			}

			while (destroyedBinders.Count != 0)
				destroyedBinders.Dequeue()?.Unbind();
		}
	}
}
