/*===============================================================
* Product:    Com2Verse
* File Name:  IUpdateSubject.cs
* Developer:  tlghks1009
* Date:       2022-05-03 10:32
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	public interface IUpdateOrganizer
	{
		void AddUpdateListener(Action onFunc, bool crossScene = false);
		void RemoveUpdateListener(Action onFunc);
		void OnUpdate();
	}
}
