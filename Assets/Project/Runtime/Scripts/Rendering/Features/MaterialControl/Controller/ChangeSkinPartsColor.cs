/*===============================================================
* Product:		Com2Verse
* File Name:	ChangeSkinPartsColor.cs
* Developer:	ljk
* Date:			2022-08-26 11:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/
#define _USE_SRP_BATCH


using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse.Rendering.MaterialControl
{
	[ExecuteAlways]
	public sealed class ChangeSkinPartsColor : MonoBehaviour
	{
		private const string SHADER_NAME = "Metaverse/Extended SimpleLit";
		private const int CHANNEL_GRADING = 256;
#if _USE_SRP_BATCH
		private List<Material> _targetMaterials;
#else
		private List<Renderer> _targetRenderers;
#endif
		
		private Texture2D _lookupTexture;
		private MaterialPropertyBlock _propertyBlock;
		private Color[] _colorTable;
		private bool _isInit = false;
		public bool Init()
		{
			List<Renderer> renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
			#if _USE_SRP_BATCH
			_targetMaterials = new List<Material>();
			#else
			_targetRenderers = new List<Renderer>();
			#endif
			
			
			renderers.ForEach(x =>
			{
#if _USE_SRP_BATCH
				for (int i = 0; i < x.materials.Length; i++)
				{
					if (x.materials[i].shader.name.Equals(SHADER_NAME))
					{
						_targetMaterials.Add(x.materials[i]);
					}
				}
#else
				for (int i = 0; i < x.sharedMaterials.Length; i++)
				{
					if (x.sharedMaterials[i].shader.name.Equals(SHADER_NAME))
					{
						_targetRenderers.Add(x);
						break;
					}
				}
#endif
			});
			
			if (_lookupTexture == null || _colorTable == null)
			{
				_colorTable = new Color[CHANNEL_GRADING];
				_lookupTexture = new Texture2D(CHANNEL_GRADING, 1, TextureFormat.RGBA32, false);
				_lookupTexture.filterMode = FilterMode.Point;
				_lookupTexture.wrapMode = TextureWrapMode.Clamp;	
			}
			_propertyBlock = new MaterialPropertyBlock();

			return true;
		}

		public void SetColor(int id, Color color)
		{
			if(!_isInit)
				_isInit = Init();

			id = Mathf.Clamp(id,0, _colorTable.Length-1);
			
			_colorTable[id] = color;
			RefreshTexture();
		}

		public void SetAllColors(Color[] colors)
		{
			if(!_isInit)
				_isInit = Init();
			
			_colorTable = colors;
			RefreshTexture();
		}

		private void RefreshTexture()
		{
			_lookupTexture.SetPixels(_colorTable);
			_lookupTexture.Apply();
			#if _USE_SRP_BATCH
			_targetMaterials.ForEach( x => x.SetTexture("_colorLookupMap",_lookupTexture));
			#else
			_propertyBlock.SetTexture("_colorLookupMap", _lookupTexture);
			_targetRenderers.ForEach( x => x.SetPropertyBlock(_propertyBlock) );
			#endif
		}
	}
}
