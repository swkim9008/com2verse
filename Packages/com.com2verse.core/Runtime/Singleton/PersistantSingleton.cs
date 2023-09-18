/*===============================================================
* Product:		Com2Verse
* File Name:	PersistantSingleton.cs
* Developer:	urun4m0r1
* Date:			2022-10-20 11:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse
{
	/// <inheritdoc />
	public class PersistantSingleton<T> : ISingleton where T : class
	{
		private static readonly Lazy<T> LazyInstance = SingletonHelper<T>.GenerateThreadSafeLazyInstance();

#region InstanceAccess
		public static bool InstanceExists => LazyInstance.IsValueCreated;

		public static T? InstanceOrNull => InstanceExists ? LazyInstance.Value : null;

		public static T Instance => GetOrCreateInstance();

		public static void CreateInstance()
		{
			if (InstanceExists)
			{
				throw SingletonHelper<T>.InstanceAlreadyCreatedException;
			}

			GetOrCreateInstance();
		}

		public static bool TryCreateInstance()
		{
			if (InstanceExists)
			{
				return false;
			}

			GetOrCreateInstance();
			return true;
		}

		public bool IsSingleton => ReferenceEquals(this, InstanceOrNull!);
#endregion // InstanceAccess

#region InstanceGeneration
		private static T GetOrCreateInstance() => LazyInstance.Value ?? throw SingletonHelper<T>.InstanceNullException;
#endregion // InstanceGeneration
	}
}
