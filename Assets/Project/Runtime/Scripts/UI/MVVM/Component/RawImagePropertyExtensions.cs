/*===============================================================
* Product:		Com2Verse
* File Name:	RawImagePropertyExtensions.cs
* Developer:	jhkim
* Date:			2022-09-29 17:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] RawImagePropertyExtensions")]
	[RequireComponent(typeof(RawImage))]
	public sealed class RawImagePropertyExtensions : MonoBehaviour
	{
		private RawImage _rawImage;

		public RawImage RawImage
		{
			get
			{
				if (_rawImage.IsReferenceNull())
					_rawImage = GetComponent<RawImage>();
				return _rawImage;
			}
			// ReSharper disable once ValueParameterNotUsed
			set => C2VDebug.LogWarningCategory(GetType().Name, "RawImage is read only");
		}

		public Texture Texture
		{
			get => RawImage.texture;
			set
			{
				RawImage.texture = value;
				RawImage.enabled = value != null;
			}
		}
	}
}
