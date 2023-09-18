/*===============================================================
* Product:		Com2Verse
* File Name:	FloatExtension.cs
* Developer:	jhkim
* Date:			2022-10-19 14:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] FloatExtension")]
	public sealed class FloatExtension : MonoBehaviour
	{
		[SerializeField] private float _value;

		public float Value
		{
			get => _value;
			set => _value = value;
		}
	}
}
