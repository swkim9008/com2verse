/*===============================================================
* Product:		Com2Verse
* File Name:	MapObjectViewModel.cs
* Developer:	eugene9721
* Date:			2022-08-18 11:51
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Extension;
using Com2Verse.Network;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Network")]
	public class MapObjectViewModel : InitializableViewModel<MapObject>
	{
		public static MapObjectViewModel Empty { get; } = new(null);

#region ViewModelProperties
		// ReSharper disable Unity.NoNullPropagation
		[UsedImplicitly] public bool   IsMine   => Value?.IsMine   ?? false;
		[UsedImplicitly] public long   ObjectId => Value?.ObjectID ?? 0;
		[UsedImplicitly] public long   OwnerId  => Value?.OwnerID  ?? 0;
		[UsedImplicitly] public string Name => (IsMine ? User.Instance.CurrentUserData.UserName : Value?.Name) ?? string.Empty;
		// ReSharper restore Unity.NoNullPropagation
		[UsedImplicitly] public Transform? ObjectTransform => Value.IsUnityNull() ? null : Value!.transform;
		[UsedImplicitly] public Transform? UITransform     => Value.IsUnityNull() ? null : Value!.UIRoot;
#endregion Properties

		public MapObjectViewModel(MapObject? mapObject)
		{
			Initialize(mapObject);
		}

		protected override void OnPrevValueUnassigned(MapObject value)
		{
			value.NameChanged -= OnNameChanged;
		}

		protected override void OnCurrentValueAssigned(MapObject value)
		{
			value.NameChanged += OnNameChanged;
		}

		public override void RefreshViewModel()
		{
			InvokePropertyValueChanged(nameof(IsMine),   IsMine);
			InvokePropertyValueChanged(nameof(ObjectId), ObjectId);
			InvokePropertyValueChanged(nameof(OwnerId),  OwnerId);
			InvokePropertyValueChanged(nameof(Name),     Name);

			InvokePropertyValueChanged(nameof(ObjectTransform), ObjectTransform);
			InvokePropertyValueChanged(nameof(UITransform),     UITransform);
		}

		private void OnNameChanged(BaseMapObject? mapObject, string? prevName, string? newName)
		{
			InvokePropertyValueChanged(nameof(Name), Name);
		}
	}
}
