/*===============================================================
* Product:		Com2Verse
* File Name:	UIClickFx.cs
* Developer:	haminjeong
* Date:			2022-06-17 19:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class UIClickFx : MonoBehaviour
	{
		private static readonly string EffectPrefabPath = "fx_UI_Touch_01.prefab";
		private GameObject _effectObject;
		private ParticleSystem _particleSystem;
		
#region Mono
		private void Start()
		{
			var handle = C2VAddressables.LoadAssetAsync<GameObject>(EffectPrefabPath);

			handle.OnCompleted += (operation) =>
			{
				_effectObject = Instantiate(operation.Result);
				_effectObject.SetActive(false);
				_effectObject.transform.SetParent(transform);
				_effectObject.transform.position = Vector3.zero;
				_particleSystem = _effectObject.GetComponent<ParticleSystem>();
				operation.Release();
			};
		}
#endregion	// Mono

		public void PlayEffect(Vector3 pos)
		{
			if (_effectObject.IsReferenceNull()) return;
			if (_particleSystem.IsReferenceNull()) return;
			if (_particleSystem.isPlaying || _effectObject.activeSelf)
			{
				_particleSystem.Stop();
				_effectObject.SetActive(false);
			}
			_effectObject.transform.position = pos + Vector3.up * 0.05f;
			_effectObject.SetActive(true);
			AutoDeactivate().Forget();
		}

		private async UniTaskVoid AutoDeactivate()
		{
			if (_effectObject.IsReferenceNull()) return;
			if (_particleSystem.IsReferenceNull()) return;
			await UniTask.WaitUntil(() =>
			{
				if (_particleSystem.IsReferenceNull()) return true;
				if (!_effectObject.activeSelf) return true;
				return !_particleSystem.isPlaying;
			});
			_effectObject.SetActive(false);
		}
	}
}
