/*===============================================================
* Product:    Com2Verse
* File Name:  ICommand.cs
* Developer:  tlghks1009
* Date:       2022-03-29 18:18
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public interface ICommand
	{
		public void Invoke(object additionalData);
	}
}
