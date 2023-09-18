/*===============================================================
 * Product:		Com2Verse
 * File Name:	SceneOverlaySettings.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-02 14:30
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse
{
	[CreateAssetMenu(fileName = "SceneOverlaySettings", menuName = "Com2Verse/SceneOverlaySettings")]
	public class SceneOverlaySettings : ScriptableObject
	{
		public IReadOnlyList<SceneOverlayDefine> OverlayDefines => _overlayDefines;

		[SerializeField] private List<SceneOverlayDefine> _overlayDefines = new();
	}
}
