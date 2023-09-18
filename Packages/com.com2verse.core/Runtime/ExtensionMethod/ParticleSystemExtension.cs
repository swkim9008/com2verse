/*===============================================================
* Product:		Com2Verse
* File Name:	ParticleSystemExtension.cs
* Developer:	eugene9721
* Date:			2023-04-20 16:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Extension
{
	public static class ParticleSystemExtension
	{
		public static float GetPlayTime(this ParticleSystem particle)
		{
			var duration = particle.main.duration + particle.main.startLifetime.constantMax;
			foreach (var subParticle in particle.GetComponentsInChildren<ParticleSystem>())
				duration = Mathf.Max(duration, subParticle.main.duration + subParticle.main.startLifetime.constantMax);
			return duration;
		}
	}
}
