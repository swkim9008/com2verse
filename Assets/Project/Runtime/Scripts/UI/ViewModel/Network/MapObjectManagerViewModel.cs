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

using Com2Verse.Network;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Network")]
	public sealed class MapObjectManagerViewModel : CollectionManagerViewModel<long, MapObjectViewModel>
	{
		public MapObjectManagerViewModel()
		{
			MapController.Instance.OnMapObjectCreate += OnMapObjectAdded;
			MapController.Instance.OnMapObjectRemove += OnMapObjectRemoved;

			if (MapController.Instance.StaticObjects != null)
			{
				foreach (var baseMapObject in MapController.Instance.StaticObjects.Values)
				{
					AddMapObject(baseMapObject);
				}
			}
		}

		private void OnMapObjectAdded(Protocols.ObjectState? objectState, BaseMapObject? baseMapObject)
		{
			AddMapObject(baseMapObject);
		}

		private void OnMapObjectRemoved(BaseMapObject? baseMapObject)
		{
			RemoveMapObject(baseMapObject);
		}

		private void AddMapObject(BaseMapObject? baseMapObject)
		{
			if (baseMapObject is not MapObject mapObject)
				return;

			Add(mapObject.ObjectID, new MapObjectViewModel(mapObject));
		}

		private void RemoveMapObject(BaseMapObject? baseMapObject)
		{
			if (baseMapObject is not MapObject mapObject)
				return;

			Remove(mapObject.ObjectID);
		}

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
