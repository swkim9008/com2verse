/*===============================================================
 * Product:		Com2Verse
 * File Name:	UISoundPlayer.cs
 * Developer:	urun4m0r1
 * Date:		2023-07-06 21:47
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2Verse.Sound
{
	public class UISoundPlayer : MonoBehaviour
	{
		[SerializeField] private AssetReference _audioFile;

		[SerializeField] private bool _playOnEnable;
		[SerializeField] private bool _playOnDisable;

		private bool _isInitialized;

		public void OnEnable()
		{
			if (_isInitialized && _playOnEnable)
				Play();

			_isInitialized = true;
		}

		public void OnDisable()
		{
			if (_isInitialized && _playOnDisable)
				Play();

			_isInitialized = true;
		}

		public void Play()
		{
			if (SoundManager.InstanceExists)
				SoundManager.Instance.PlayUISound(_audioFile);
		}
	}
}
