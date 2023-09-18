/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceInfo.cs
* Developer:	urun4m0r1
* Date:			2022-04-04 10:31
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;

namespace Com2Verse.Communication
{
	/// <summary>
	/// Record for physical device information.
	/// </summary>
	/// <param name="Id"><c>[NotNull]</c> Device`s unique identifier.</param>
	/// <param name="Name"><c>[NotNull]</c> Device`s friendly name.</param>
	[Serializable]
	public record DeviceInfo(string Id, string Name, bool IsSystemDefault = false)
	{
		/// <summary>
		/// Empty device info. You can use this to indicate null device.
		/// </summary>
		public static DeviceInfo Empty { get; } = new("", "Empty") { IsAvailable = false };

		/// <summary>
		/// Device`s unique identifier.
		/// </summary>
		[field: SerializeField]
		public string Id { get; private set; } = Id;

		/// <summary>
		/// Device`s friendly name.
		/// </summary>
		[field: SerializeField]
		public string Name { get; private set; } = Name;

		/// <summary>
		/// If a device is system default, this info will be used instead of <see cref="Id"/>.
		/// </summary>
		[field: SerializeField]
		public bool IsSystemDefault { get; private set; } = IsSystemDefault;

		/// <summary>
		/// Value indicating if this device can be used.
		/// Do not serialize this value.
		/// </summary>
		public bool IsAvailable { get; internal set; } = true;

		public override string ToString() => $"{Name} ({Id})";

		public bool IsEmptyDevice => IsSameDevice(Empty);

		public bool IsSameDevice(DeviceInfo other)
		{
			if (IsSystemDefault || other.IsSystemDefault)
				return IsSystemDefault == other.IsSystemDefault;
			else
				return Id == other.Id;
		}
	}
}
