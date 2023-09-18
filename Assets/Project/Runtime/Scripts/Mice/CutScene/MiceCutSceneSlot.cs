/*===============================================================
* Product:		Com2Verse
* File Name:	MiceCutSceneSlot.cs
* Developer:	wlemon
* Date:			2023-07-14 18:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Extension;
using System.Collections.Generic;
using Com2Verse.Network;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.Mice
{
	[Serializable]
	public sealed partial class MiceCutSceneSlot
	{
		public enum eType
		{
			PC_W,
			PC_M,
			NPC_W_MODERATOR,
			NPC_M_MODERATOR,
			
			NONE = -1,
		}

		[SerializeField]
		private eType _type;

		[SerializeField]
		private Transform _socket;

		[SerializeField]
		private string[] _trackNames;
		
		[SerializeField, HideInInspector]
		private GameObject _boundGameObject;
		
		[SerializeField, HideInInspector]
		private Transform _orginalParent;

		[SerializeField, HideInInspector]
		private GameObject _preview;

		public eType Type => _type;
		
		private Vector3      _originalPosition;
		private Quaternion   _originalRotation;

		public void Bind(MiceCutSceneController controller, GameObject target)
		{
			Unbind(controller);
			if (target.IsUnityNull()) return;
			
			_boundGameObject   = target;
			_orginalParent     = _boundGameObject.transform.parent;
			_originalPosition  = _boundGameObject.transform.position;
			_originalRotation  = _boundGameObject.transform.rotation;
			if (!_socket.IsUnityNull())
			{
				_boundGameObject.transform.SetParent(_socket, false);
				_boundGameObject.transform.localPosition = Vector3.zero;
				_boundGameObject.transform.localRotation = Quaternion.identity;
			}

			foreach (var trackName in _trackNames)
			{
				controller.SetGameObjectBinding(trackName, _boundGameObject);
			}
		}

		public void Unbind(MiceCutSceneController controller)
		{
			if (!_boundGameObject.IsUnityNull())
			{
				if (_boundGameObject.TryGetComponent<BaseMapObject>(out var baseMapObject))
				{
					if (baseMapObject is ActiveObject activeObject)
					{
						activeObject.AvatarController.SetUseFadeIn(false);
					}
				}

				foreach (var trackName in _trackNames)
				{
					controller.RemoveGameObjectBinding(trackName);
				}
				if (_boundGameObject == _preview)
				{
					GameObject.DestroyImmediate(_preview);
					_preview          = null;
				}
				else
				{
					if (!_socket.IsUnityNull())
					{
						_boundGameObject.transform.SetParent(_orginalParent);
						_boundGameObject.transform.parent = _orginalParent;
						_orginalParent                    = null;
					}
					_boundGameObject.transform.position = _originalPosition;
					_boundGameObject.transform.rotation = _originalRotation;
				}
				_boundGameObject = null;
			}
		}

		public void SyncTransform()
		{
			if (_boundGameObject.IsUnityNull()) return;
			if (_socket.IsUnityNull()) return;
			
			_boundGameObject.transform.localPosition = Vector3.zero;
			_boundGameObject.transform.localRotation = Quaternion.identity;
		}
	}
	
#if UNITY_EDITOR
	public partial class MiceCutSceneSlot
	{
		private static readonly Dictionary<eType, string> PreviewPaths
			= new Dictionary<eType, string>
			{
				{ eType.PC_W, "Assets/Project/InternalAssets/Cutscene/PreviewAvatar/PreviewAvatar_PC_W.prefab" },
				{ eType.PC_M, "Assets/Project/InternalAssets/Cutscene/PreviewAvatar/PreviewAvatar_PC_M.prefab" },
				{ eType.NPC_W_MODERATOR, "Assets/Project/InternalAssets/Cutscene/PreviewAvatar/PreviewAvatar_NPC_W.prefab" },
				{ eType.NPC_M_MODERATOR, "Assets/Project/InternalAssets/Cutscene/PreviewAvatar/PreviewAvatar_NPC_M.prefab" }
			};
		
		public void CreatePreview(MiceCutSceneController controller)
		{
			RemovePreview(controller);

			if (!PreviewPaths.TryGetValue(_type, out var path)) return;
			
			var previewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (previewPrefab.IsUnityNull()) return;

			_preview = GameObject.Instantiate(previewPrefab);
			Bind(controller, _preview);
		}

		public void RemovePreview(MiceCutSceneController controller)
		{
			Unbind(controller);
			if (!_preview.IsUnityNull())
			{
				GameObject.DestroyImmediate(_preview);
				_preview = null;
			}
		}
	}
#endif
}
