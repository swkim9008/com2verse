/*===============================================================
* Product:		Com2Verse
* File Name:	IUpdatableCamera.cs
* Developer:	urun4m0r1
* Date:			2022-11-01 11:48
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.CameraSystem
{
	public interface IUpdatableCamera
	{
		void OnUpdate();
		void OnLateUpdate();
	}
}
