/*===============================================================
* Product:		Com2Verse
* File Name:	ThumbnailDatabase.cs
* Developer:	yangsehoon
* Date:			2023-03-31 12:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;
using Cysharp.Text;

namespace Com2Verse.Builder
{
	[CreateAssetMenu(fileName ="ThumbnailDatabase", menuName = "Com2Verse/ThumbnailDatabase")]
	public sealed class ThumbnailDatabase : ScriptableObject
	{
		[SerializeField] private string _thumbnailPath = "Packages/com.com2verse.builder/Prefab_AAG/Thumbnail";
		[SerializeField] private ThumbnailGenerateOption _option = new();
		[SerializeField] private List<GameObject> _targetObjects;
		[SerializeField] private List<Material> _targetMaterials;
		[SerializeField] private List<Texture2D> _sprites;
		private Dictionary<string, Sprite> _thumbnailMap = new Dictionary<string, Sprite>();

		public ThumbnailGenerateOption Option => _option;
		public string ThumbnailPath => _thumbnailPath;
		public Dictionary<string, Sprite> ThumbnailMap => _thumbnailMap;
		public List<GameObject> TargetObjects => _targetObjects;
		public List<Material> TargetMaterials => _targetMaterials;
		public List<Texture2D> Sprites => _sprites;

		public void Initialize()
		{
			_thumbnailMap.Clear();
			for (int i = 0; i < _sprites.Count; i++)
			{
				string[] fileNameToken = _sprites[i].name.Split('.');
				
				_thumbnailMap.Add(ZString.Format("{0}.{1}", fileNameToken[0], fileNameToken[1]), Sprite.Create(_sprites[i], new Rect(0, 0, _sprites[i].width, _sprites[i].height), Vector2.one / 2));
			}
		}

		public Sprite GetThumbnail(string address)
		{
			if (_thumbnailMap.TryGetValue(address, out Sprite image))
			{
				return image;
			}

			return null;
		}
	}

	[System.Serializable]
	public class ThumbnailGenerateOption
	{
		public float FitRatio => _fitRatio;
		public float FOV => _fieldOfView;
		public bool IncludeAlpha => _includeAlpha;
		public Color AmbientLightColor => _ambientLightColor;
		public Color DirectionalLightColor => _directionalLightColor;
		public bool IncludeShadow => _includeShadow;
		public Sprite BackgroundImage => _backgroundImage;
		public Vector3 CameraRotation => _cameraRotation;
		public Vector3 DirectionalLightRotation => _directionalLightRotation;
		public Vector3 ShadowOffset => _shadowOffset;
		public Vector2 ImageSize => _imageSize;
		public int IterationCount => _iterationCount;

		[SerializeField] private float _fitRatio = 1.0f;
		[SerializeField] private float _fieldOfView = 30.0f;
		[SerializeField] private bool _includeAlpha = true;
		[SerializeField] private Color _ambientLightColor = Color.white * 0.8f;
		[SerializeField] private Color _directionalLightColor = Color.white;
		[SerializeField] private bool _includeShadow = true;
		[SerializeField] private Sprite _backgroundImage = null;
		[SerializeField] private Vector3 _cameraRotation = new Vector3(30, 180, 0);
		[SerializeField] private Vector3 _directionalLightRotation = new Vector3(50, 150, 0);
		[SerializeField] private Vector3 _shadowOffset = new Vector3(0, 0, 0);
		[SerializeField] private Vector2 _imageSize = new Vector2(128, 128);
		[SerializeField] private int _iterationCount = 2;
	}
}
