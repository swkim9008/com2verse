/*===============================================================
* Product:		Com2Verse
* File Name:	INotifyPropertyChanged.cs
* Developer:	tlghks1009
* Date:			2022-12-08 14:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	public interface INotifyPropertyChanged<out T>
	{
		void AddListener(Action<string, T> handler);

		void RemoveListener(Action<string, T> handler);
	}

	public interface INotifyPropertyChanged
	{
		void AddListener(Action<string> handler);

		void RemoveListener(Action<string> handler);
	}
}
