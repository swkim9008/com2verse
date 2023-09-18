/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadNavigator.cs
* Developer:	tlghks1009
* Date:			2022-08-23 11:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class MyPadNavigator : MonoBehaviour
	{
		public void OpenMyPad()
		{
			MyPadManager.Instance.OpenMyPad();
		}
	}
}
