/*===============================================================
* Product:		Com2Verse
* File Name:	ILoadingTask.cs
* Developer:	tlghks1009
* Date:			2022-06-07 19:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.Loading
{
	public interface ILoadingTask
	{
		event Action<ILoadingTask> OnCompleted;
	}
}
