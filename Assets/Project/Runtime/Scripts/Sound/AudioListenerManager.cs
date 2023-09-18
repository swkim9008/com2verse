// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	AudioListenerManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-19 오후 12:05
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.Network;
using UnityEngine;

namespace Com2Verse.Sound
{
	public class AudioListenerManager : MonoSingleton<AudioListenerManager>
	{
		public GameObject ListenerObject => _listenerHolder;
		private GameObject _listenerHolder;
	
		public void Initialize()
		{
			MapController.Instance.OnMapObjectRemove += OnObjectRemoved;
		}

		private void OnObjectRemoved(BaseMapObject mapObject)
		{
			if (User.InstanceExists)
			{
				var serial = mapObject.ObjectID;
				if (serial == User.Instance.CurrentUserData.ObjectID)
				{
					// My Avatar will be removed
					_listenerHolder.transform.SetParent(transform);
				}
			}
		}

		protected override void AwakeInvoked()
		{
			_listenerHolder = new GameObject("AudioListenerHolder");
			_listenerHolder.transform.SetParent(transform);

			_listenerHolder.AddComponent<AudioListener>();
		}

		public void ResetAudioListenerToCamera()
		{
			var mainCamera = CameraManager.InstanceOrNull?.MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			_listenerHolder.transform.SetParent(mainCamera!.transform);
			_listenerHolder.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
	}
}
