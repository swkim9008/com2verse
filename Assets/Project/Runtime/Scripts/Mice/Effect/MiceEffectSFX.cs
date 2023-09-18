/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEffectSFX.cs
* Developer:	wlemon
* Date:			2023-07-19 19:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Sound;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Com2Verse.Mice
{
	public class MiceEffectSFX : MiceEffectObject
	{
		[SerializeField]
		private AssetReference _asset;

		private CancellationTokenSource _cts;
		private Queue<AudioSource>      _sfxList;

		private void Awake()
		{
			_cts     = new();
			_sfxList = new();
		}

		private void OnDestroy()
		{
			_cts.Cancel();
		}

		public override void Play(double offset)
		{
			PlayOneShot(_asset, _cts.Token).Forget();
		}

		private async UniTask PlayOneShot(AssetReference assetReference, CancellationToken token)
		{
			if (!_sfxList.TryDequeue(out var audioSource))
			{
				var audioSourceObject = new GameObject();
				audioSource = audioSourceObject.AddComponent<AudioSource>();
				audioSourceObject.transform.SetParent(transform);
			}

			await SoundManager.Instance.PlayOneShot(audioSource, assetReference);
			if (token.IsCancellationRequested) return;

			await UniTask.WaitUntil(() => !audioSource.isPlaying, cancellationToken: token);
			if (token.IsCancellationRequested) return;

			_sfxList.Enqueue(audioSource);
		}
	}
}
