/*===============================================================
* Product:		Com2Verse
* File Name:	IntExtension.cs
* Developer:	jhkim
* Date:			2022-10-14 10:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] IntExtension")]
	public sealed class IntExtension : MonoBehaviour
	{
		[SerializeField] private int _value;

		public int Value
		{
			get => _value;
			set => _value = value;
		}
	}
}
