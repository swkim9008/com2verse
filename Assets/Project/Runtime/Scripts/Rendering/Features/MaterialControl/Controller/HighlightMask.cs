/*===============================================================
* Product:		Com2Verse
* File Name:	HighlightMask.cs
* Developer:	ljk
* Date:			2022-08-22 15:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Com2Verse.Rendering.Effect
{
	public sealed class HighlightMask : MonoBehaviour
	{
		private readonly int MAX_SELECTABLE_PARTS = 128;
		private readonly int MAX_SELECTABLES_AT_ONE_TIME = 128;

		class HighlightItem
		{
			public int _index;
			public float _intensity;
			public float _runtime;
			public float _lifetime;
		}
		
		private MaterialPropertyBlock _propertyBlock;
		private Texture2D _maskIndexer;
		private List<HighlightItem> _runningHighlightItems;
		private Color[] _indexerColors;
		private Material _highlightMaterial;
		public float _highlightTime = 1;
		private bool _isInit = false;

		private Renderer[] _renderers;

		#region Mono

		private void Awake()
		{
			Init();	
		}

		private void Update()
		{
			RefreshItems();
			RefreshTexture();
		}

		#endregion Mono

		private void Init()
		{
			_renderers = GetComponentsInChildren<Renderer>();
			_propertyBlock = new MaterialPropertyBlock();

			_indexerColors = new Color[MAX_SELECTABLES_AT_ONE_TIME];
			for(int i = 0; i < _indexerColors.Length ; i++)
				_indexerColors[i] = Color.black;

			_maskIndexer = new Texture2D(MAX_SELECTABLES_AT_ONE_TIME,1,TextureFormat.RG16,false);
			_maskIndexer.filterMode = FilterMode.Point;
			_maskIndexer.SetPixels(_indexerColors);
			_runningHighlightItems = new List<HighlightItem>();
			_isInit = true;
		}

		private void RefreshItems()
		{
			for (int i = _runningHighlightItems.Count - 1; i >= 0; i--)
			{
				_runningHighlightItems[i]._runtime -= Time.deltaTime;
				if (_runningHighlightItems[i]._runtime < 0)
				{
					if (i > 0)
						_runningHighlightItems.RemoveAt(i);
					continue;
				}

				_runningHighlightItems[i]._intensity =
					_runningHighlightItems[i]._runtime / _runningHighlightItems[i]._lifetime;
			}
		}
		
		private void RefreshTexture()
		{
			for (int i = 0; i < MAX_SELECTABLES_AT_ONE_TIME; i++)
			{
				if (i < _runningHighlightItems.Count)
				{
					_indexerColors[i].r = _runningHighlightItems[i]._index / (float)MAX_SELECTABLE_PARTS;
					_indexerColors[i].g = _runningHighlightItems[i]._intensity;
				}
				else
				{
					_indexerColors[i].r = 0;
					_indexerColors[i].g = 0;
				}
			}
			
			_maskIndexer.SetPixels(_indexerColors);
			_maskIndexer.Apply();
			_propertyBlock.SetTexture("_highLightIndex",_maskIndexer);
			_propertyBlock.SetFloat("_highLightPartsCount",_runningHighlightItems.Count);
			
			for (int i = 0; i < _renderers.Length; i++)
			{
				_renderers[i].SetPropertyBlock(_propertyBlock);
			}
		}
		
		public void SetupTexture(Texture2D partsMask, string oldShaderName = "Simple Lit", string newShaderName = "Metaverse/Extended SimpleLit")
		{
			if(!_isInit)
				Init();
			
			for (int i = 0; i < _renderers.Length; i++)
			{
				Renderer childRenderer = _renderers[i];
				List<Material> instancedTargets = new List<Material>( childRenderer.materials );

				for (int j = 0; j < instancedTargets.Count; j++)
				{
					if (instancedTargets[j].shader.name.Contains(oldShaderName))
					{
						instancedTargets[j].shader = Shader.Find(newShaderName);
						instancedTargets[j].SetKeyword(new LocalKeyword(instancedTargets[j].shader,"_USE_HIGHLIGHT"),true); 
						//CoreUtils.SetKeyword(instancedTargets[j],"_USE_HIGHLIGHT",true);
					}
				}
			}
			
			_propertyBlock.SetTexture("_highLightParts",partsMask);
			_propertyBlock.SetTexture("_highLightIndex",_maskIndexer);
			_propertyBlock.SetFloat("_highLightMaxIndex",MAX_SELECTABLE_PARTS);
		}
		
		public void HighLightIndex(int partsIndex)
		{
			HighlightItem item = _runningHighlightItems.Find(x => x._index == partsIndex);

			if (item == null)
			{
				item = new HighlightItem();
				_runningHighlightItems.Add(item);
			}

			item._index = partsIndex;
			item._runtime = _highlightTime;
			item._lifetime = _highlightTime;
		}

		public void HighLightIndexes(List<int> partsIndexes)
		{
			partsIndexes.ForEach(x => HighLightIndex(x));
		}
	}
}
