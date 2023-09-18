/*===============================================================
 * Product:		Com2Verse
 * File Name:	IRedDotSource.cs
 * Developer:	yangsehoon
 * Date:		2022-12-06 오전 11:07
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public interface IRedDotSource
	{
		public bool Enabled();
		
		public void Activate();
		public void Deactivate();
	}
}
