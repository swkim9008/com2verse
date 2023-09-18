/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBICapture.cs
* Developer:	sprite
* Date:			2023-06-13 17:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Com2Verse.AssetSystem;

namespace Com2Verse.Mice
{
	public sealed class MiceBICapture : MonoBehaviour
	{
		private const string ASSET_NAME = "MiceBICapture.prefab";

        private static MiceBICapture _current;

        public Camera Camera { get; private set; }

        private void Awake()
        {
			this.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

			this.Camera = this.GetComponentInChildren<Camera>();
        }

        private void OnDestroy()
        {
			Addressables.ReleaseInstance(this.gameObject);
        }

        public static async UniTask<MiceBICapture> GetOrInstantiate()
		{
            if (_current == null || !_current)
            {
                var asset = await C2VAddressables.LoadAssetAsync<GameObject>(ASSET_NAME).ToUniTask();
                var go = Instantiate(asset);
                if (go == null || !go) return null;

                _current = go.GetComponent<MiceBICapture>();
            }

            return _current;
        }

        public static void Remove()
        {
            if (_current == null || !_current) return;

            DestroyImmediate(_current.gameObject);
            _current = null;
        }
	}
}
