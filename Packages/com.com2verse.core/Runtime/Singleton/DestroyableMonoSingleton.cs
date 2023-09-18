/*===============================================================
* Product:		Com2Verse
* File Name:	DestroyableMonoSingleton.cs
* Developer:	urun4m0r1
* Date:			2022-10-20 11:03
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse
{
	/// <inheritdoc />
	public class DestroyableMonoSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
	{
		static DestroyableMonoSingleton()
		{
			IsDestroyable = new GenericValue<T, bool>(true);
		}
	}
}
