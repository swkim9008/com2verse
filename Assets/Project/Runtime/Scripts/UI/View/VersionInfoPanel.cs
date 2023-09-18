/*===============================================================
* Product:		Com2Verse
* File Name:	VersionInfoPanel.cs
* Developer:	masteage
* Date:			2022-09-15 11:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class VersionInfoPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text txtVersionInfo;
#region Mono
		private void Awake()
		{
			var newText = "";
			if (AppInfo.Instance != null)
			{
				// v0.1.11630_d (4449ed86f)
				newText = $"v{AppInfo.Instance.Data.GetVersionInfo()}";
			}
			txtVersionInfo.text = newText;
		}
#endregion	// Mono
	}
}
