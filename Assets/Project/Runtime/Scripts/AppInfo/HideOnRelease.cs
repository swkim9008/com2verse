/*===============================================================
* Product:		Com2Verse
* File Name:	HideOnRelease.cs
* Developer:	jhkim
* Date:			2022-06-30 11:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse
{
	public sealed class HideOnRelease : MonoBehaviour
	{
		[SerializeField] private bool _forceEnable = false;
		private void Awake()
		{
			gameObject.SetActive(_forceEnable || AppInfo.Instance.Data.IsForceEnableAppInfo || AppInfo.Instance.Data.IsDebug);
		}
	}
}
