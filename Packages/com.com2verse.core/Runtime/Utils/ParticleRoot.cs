/*===============================================================
* Product:		Com2Verse
* File Name:	ParticleRoot.cs
* Developer:	eugene9721
* Date:			2023-04-20 15:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif // UNITY_EDITOR
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Utils
{
	public sealed class ParticleRoot : MonoBehaviour
	{
		[field: SerializeField] public float PlayTime { get; private set; }

		public ParticleSystem? ParticleSystem { get; private set; }

		private void Awake()
		{
			ParticleSystem = GetComponentInChildren<ParticleSystem>()!;
		}

		public void SetParticleSystem(ParticleSystem particle)
		{
			ParticleSystem = particle;
			PlayTime       = particle.GetPlayTime();
		}

#if UNITY_EDITOR
		[ContextMenu("Bake")]
		private void Bake()
		{
			PlayTime = GetComponentInChildren<ParticleSystem>()!.GetPlayTime();
			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
				EditorSceneManager.MarkSceneDirty(prefabStage.scene);
		}
#endif // UNITY_EDITOR
	}
}
