/*===============================================================
* Product:		Com2Verse
* File Name:	MinimapPopupViewModel.cs
* Developer:	haminjeong
* Date:			2023-06-29 12:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("ToolBar")]
	public sealed class MinimapPopupViewModel : ViewModelBase, IDisposable
	{
		private Collection<MinimapWarpIconViewModel> _warpIconCollection = new();
		public Collection<MinimapWarpIconViewModel> WarpIconCollection
		{
			get => _warpIconCollection;
			set => SetProperty(ref _warpIconCollection, value);
		}
		
		private Map _mapData;
		public  Map MapData => _mapData;

		private RawImage _mapImage;
		public RawImage MapImage
		{
			get => _mapImage;
			set => SetProperty(ref _mapImage, value);
		}

		private Vector2 _markerPosition;
		public Vector2 MarkerPosition
		{
			get => _markerPosition;
			set => SetProperty(ref _markerPosition, value);
		}

		private Quaternion _markerRotation;
		public Quaternion MarkerRotation
		{
			get => _markerRotation;
			set => SetProperty(ref _markerRotation, value);
		}

		private Vector3 _myPosition;
		public Vector3 MyPosition
		{
			get => _myPosition;
			set
			{
				float xRatio = (value.x - _mapData.LeftBottom.x) / _mapData.Size.x;
				float yRatio = (value.z - _mapData.LeftBottom.y) / _mapData.Size.y;
				
				if (MapImage != null)
					MarkerPosition = new Vector2(MapImage.texture.width * xRatio, MapImage.texture.height * yRatio);
				SetProperty(ref _myPosition, value);
			}
		}

		private Vector3 _myEulerAngle;
		public Vector3 MyEulerAngle
		{
			get => _myEulerAngle;
			set
			{
				float convertAngle = -(value.y % 360f);
				MarkerRotation = Quaternion.Euler(0, 0, convertAngle);
				SetProperty(ref _myEulerAngle, value);
			}
		}

		public MinimapPopupViewModel()
		{
			var tableMap = TableDataManager.Instance.Get<TableMap>();
			_mapData = tableMap!.Datas![1];
		}

		public void SetWarpIcons()
		{
			var tableWarp = TableDataManager.Instance.Get<TableWarpPosition>();
			_warpIconCollection!.Reset();
			for (int i = 1; i <= tableWarp!.Datas!.Count; ++i)
			{
				var data = tableWarp.Datas[i];
				if (!data.IsActivation) continue;
				var warpIconViewModel = new MinimapWarpIconViewModel();
				_warpIconCollection.AddItem(warpIconViewModel);
				warpIconViewModel.SetWarpIcon(this, data);
			}
		}

		public void Dispose()
		{
			_warpIconCollection!.DestroyAll();
		}
	}
}
