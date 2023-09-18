/*===============================================================
* Product:		Com2Verse
* File Name:	IInitializable.cs
* Developer:	urun4m0r1
* Date:			2022-05-30 09:43
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.UI
{
	public interface IInitializable
	{
		bool IsInitialized { get; }
		void Initialize();
		void Terminate();
	}

	public interface IInitializable<in T>
	{
		bool IsInitialized { get; }
		void Initialize(T? parameter);
		void Terminate();
	}
}
