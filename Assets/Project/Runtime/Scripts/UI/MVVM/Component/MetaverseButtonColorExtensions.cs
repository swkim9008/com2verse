/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseButtonColorExtensions.cs
* Developer:	jhkim
* Date:			2022-10-27 10:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] MetaverseButtonColorExtensions")]
	public sealed class MetaverseButtonColorExtensions : MonoBehaviour
	{
		[SerializeField] private ColorInfo[] _colorInfos;
		private MetaverseButton _button;

		public bool IsEnable
		{
			get => _button.interactable;
			set => UpdateColor(value);
		}

		private MetaverseButton GetComponent()
		{
			if (_button.IsReferenceNull())
				_button = GetComponent<MetaverseButton>();
			return _button;
		}

		private void UpdateColor(bool enable)
		{
			foreach (var colorInfo in _colorInfos)
				colorInfo.Graphic.color = enable ? colorInfo.NormalColor : colorInfo.DisableColor;
		}
		[Serializable]
		public struct ColorInfo
		{
			public Graphic Graphic;
			public Color NormalColor;
			public Color DisableColor;
		}
	}
}
