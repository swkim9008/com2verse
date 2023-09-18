/*===============================================================
* Product:		Com2Verse
* File Name:	BusinessCardRT.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Com2Verse.Mice
{
    public class BusinessCardRT : MonoBehaviour
    {
        public static readonly string SceneName = "BusinessCardRTScene.unity";
        private static BusinessCardRT _instance;
        
        [SerializeField] private Transform _posModel;
        [SerializeField] private Camera    _camera;
        [SerializeField] private string    _renderLayer;
        [SerializeField] private string    _targetBoneName;
        [SerializeField] private Vector3   _cameraOffset;
        
        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public void SetModel(GameObject model)
        {
            while (_posModel.childCount > 0)
            {
                var child = _posModel.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
            
            model.gameObject.SetActive(true);
            model.transform.SetParent(_posModel);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            var layer = LayerMask.NameToLayer(_renderLayer);
            var transforms = model.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms)
            {
                transform.gameObject.layer = layer;
            }

            var targetBone = model.transform.FindRecursive(_targetBoneName);
            if (!targetBone.IsUnityNull())
            {
                _camera.transform.position = targetBone.transform.position + _cameraOffset;
            }
        }

        public Texture2D Capture()
        {
			_camera.cullingMask = LayerMask.GetMask(_renderLayer);
            _camera.Render();

            var activeRenderTexture = RenderTexture.active;
            RenderTexture.active = _camera.targetTexture;

            var texture = new Texture2D(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            texture.Apply();

            RenderTexture.active = activeRenderTexture;
            return texture;
        }

        public static async UniTask<Texture2D> CreateAsync(GameObject model)
        {
            var handle = Addressables.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            await handle;

            _instance.SetModel(model);
            var texture = _instance.Capture();

            await Addressables.UnloadSceneAsync(handle.Result);
            return texture;
        }

        public static async UniTask<Texture2D> CreateAsync(AvatarInfo avatarInfo)
        {
            var avatarController = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.AVATAR_CUSTOMIZE, Vector3.zero, (int)Define.eLayer.CHARACTER);
            while (!avatarController.IsCompletedLoadAvatarParts) await UniTask.Yield();

            return await CreateAsync(avatarController.gameObject);
        }
    }
}
