/*===============================================================
* Product:    Com2Verse
* File Name:  Vector3Extension.cs
* Developer:  haminjeong
* Date:       2022-11-10 12:04
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Extension
{
	public static class Vector3Extension 
	{
		public static Vector3 SmoothStep(Vector3 prevVector, Vector3 targetVector, float time)
		{
			float x = Mathf.SmoothStep(prevVector.x, targetVector.x, time);
			float y = Mathf.SmoothStep(prevVector.y, targetVector.y, time);
			float z = Mathf.SmoothStep(prevVector.z, targetVector.z, time);
			return new Vector3(x, y, z);
		}
	}
}
