/*===============================================================
* Product:		Com2Verse
* File Name:	IClickableObject.cs
* Developer:	haminjeong
* Date:			2023-05-20 22:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Network
{
	public interface IClickableObject
	{
		public void     OnClickObject();
		public bool     IsClickableEnable { get; }
		public Collider ClickCollider     { get; }
		public void     InitCollider(bool isCreateCollider);
	}
}
