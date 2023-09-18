/*===============================================================
* Product:		Com2Verse
* File Name:	TextExtensions.cs
* Developer:	wlemon
* Date:			2023-04-13 10:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using TMPro;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(TMP_Text))]
	[AddComponentMenu("[DB]/[DB] TextExtensions")]
	public sealed class TextExtensions : MonoBehaviour
	{
		[SerializeField, HideInInspector] private TMP_Text _text;
		
		public int IntText
		{
			get => int.TryParse(_text.text, out var value) ? value : 0;
			set => _text.text = value.ToString();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying) return;

			if (_text == null)
				if (!TryGetComponent(out _text))
					throw new NullReferenceException(nameof(_text));

			if (_text == null) return;
		}
#endif // UNITY_EDITOR
	}
}
