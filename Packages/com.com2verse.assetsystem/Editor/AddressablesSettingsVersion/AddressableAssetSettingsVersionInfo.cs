/*===============================================================
* Product:		Com2Verse
* File Name:	AddressableAssetSettingsVersionInfo.cs
* Developer:	tlghks1009
* Date:			2023-05-26 15:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2VerseEditor.AssetSystem
{
	[CreateAssetMenu(fileName = "AddressableAssetSettingsVersionInfo", menuName = "Addressables/Initialization/C2V Addressables Asset Settings Version")]
	public sealed class AddressableAssetSettingsVersionInfo : ScriptableObject
	{
		[field: SerializeField] public string Version { get; set; }
	}
}
