/*===============================================================
* Product:		Com2Verse
* File Name:	IModule.cs
* Developer:	urun4m0r1
* Date:			2022-04-14 20:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	/// <summary>
	/// Interface for toggling modules.
	/// You might need to implement IDisposable to be able to dispose of the module.
	/// </summary>
	public interface IModule
	{
		bool IsRunning { get; set; }

		event Action<bool>? StateChanged;
	}
}
