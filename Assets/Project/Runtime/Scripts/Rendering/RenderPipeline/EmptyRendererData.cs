/*===============================================================
* Product:		Com2Verse
* File Name:	EmptyRendererData.cs
* Developer:	ljk
* Date:			2022-10-06 09:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Com2Verse.Rendering.RenderPipeline
{
	[Serializable, ReloadGroup, ExcludeFromPreset]
	public sealed class EmptyRendererData : ScriptableRendererData, ISerializationCallbackReceiver
	{
		private const int _latestAssetVersion = 1;
		// asset version 변수에 대한 워닝 disable
#pragma warning disable 0414
		[SerializeField] private int _assetVersion = 0;
#pragma warning restore 0414
		
		[SerializeField] public StencilStateData _defaultStencilState = new StencilStateData() { passOperation = StencilOp.Replace };

		protected override ScriptableRenderer Create()
		{
			if (!Application.isPlaying)
			{
				ReloadAllNullProperties();
			}
			return new EmptyRenderer(this);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			ReloadAllNullProperties();
		}

		private void ReloadAllNullProperties()
		{
#if UNITY_EDITOR
			ResourceReloader.TryReloadAllNullIn(this, UniversalRenderPipelineAsset.packagePath);
#endif
		}

		public void OnBeforeSerialize()
		{
			_assetVersion = _latestAssetVersion;
		}

		public void OnAfterDeserialize()
		{
			_assetVersion = _latestAssetVersion;
		}
	}
}
