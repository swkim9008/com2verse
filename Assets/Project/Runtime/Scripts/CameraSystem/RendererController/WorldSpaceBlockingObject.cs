/*===============================================================
 * Product:		Com2Verse
 * File Name:	WorldSpaceBlockingUI.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-12 14:35
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Project.CameraSystem
{
	/// <summary>
	/// 3D 렌더링 화면을 블로킹하는 오브젝트 (ex. 전체화면 UI)
	/// </summary>
	[AddComponentMenu("[CameraSystem]/[CameraSystem] World Space Blocking Object")]
	public class WorldSpaceBlockingObject : MonoBehaviour
	{
		protected virtual void OnEnable()  => RegisterBlockingObject();
		protected virtual void OnDisable() => UnregisterBlockingObject();

		private void RegisterBlockingObject()
		{
			CameraMediator.Instance.TryAddWorldSpaceBlockingObject(this);
		}

		private void UnregisterBlockingObject()
		{
			CameraMediator.InstanceOrNull?.RemoveWorldSpaceBlockingObject(this);
		}
	}
}
