/*===============================================================
* Product:		Com2Verse
* File Name:	ClickDelegate.cs
* Developer:	haminjeong
* Date:			2023-05-31 22:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Network
{
	public sealed class ClickDelegate : MonoBehaviour
	{
		private IClickableObject _delegateObject;

		public void SetDelegate(IClickableObject target) => _delegateObject = target;

		public void OnClickEvent() => _delegateObject?.OnClickObject();

		private void OnDestroy() => _delegateObject = null;
	}
}
