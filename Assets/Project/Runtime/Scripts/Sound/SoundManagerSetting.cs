/*===============================================================
* Product:    Com2Verse
* File Name:  SoundManagerSetting.cs
* Developer:  yangsehoon
* Date:       2022-04-18 14:41
* History:    
* Documents:  Sound manager setting scriptable object file for global sound settings & sound file script access
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2Verse.SoundSystem
{
	[CreateAssetMenu(fileName = "SoundManager", menuName = "SoundManager")]
#if UNITY_EDITOR
	public sealed partial class SoundManagerSetting : ScriptableSingleton<SoundManagerSetting>
#else
	public sealed partial class SoundManagerSetting : ScriptableObject
#endif
	{
		[System.Serializable]
		public struct AudioClipScriptReference
		{
			public int Index;
			public AssetReference Reference;
		}

		public static AssetReference GetSoundClip(eSoundIndex index)
		{
			return _scriptTargetClips.TryGetValue((int) index, out string guid) ? new AssetReference(guid) : null;
		}

#if UNITY_EDITOR
		[SerializeField] private string _baseGeneratedScriptPath = "Assets/Project/Runtime/Scripts/Sound/Generated/";
		[SerializeField] private string _audioMixerIndexFileName = "AudioMixerIndex.cs";
		[SerializeField] private string _audioSnapshotIndexFileName = "AudioSnapshotIndex.cs";
		[SerializeField] private string _soundIndexFileName = "SoundIndex.cs";
		[SerializeField] private string _soundManagerDictionaryFileName = "SoundManagerDictionary.cs";

		public string AudioMixerIndexFileName => _baseGeneratedScriptPath + _audioMixerIndexFileName;
		public string AudioSnapshotIndexFileName => _baseGeneratedScriptPath + _audioSnapshotIndexFileName;
		public string SoundIndexFileName => _baseGeneratedScriptPath + _soundIndexFileName;
		public string SoundManagerDictionaryFileName => _baseGeneratedScriptPath + _soundManagerDictionaryFileName;

		public List<AudioClipScriptReference> ScriptTargetClips;
		
		public void ClearClips()
		{
			ScriptTargetClips.Clear();
		}
#endif
	}
}