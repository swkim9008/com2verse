/*===============================================================
* Product:    Com2Verse
* File Name:  StringExtension.cs
* Developer:  tlghks1009
* Date:       2022-04-01 18:24
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.Extension
{
	public static class TransformExtension
	{
		public static string GetFullPathInHierachy(this Transform thisTransform)
		{
			string path = $"/{thisTransform.name}";
			while ( !object.ReferenceEquals( thisTransform.transform.parent , null ) )
			{
				thisTransform = thisTransform.parent;
				path = $"/{thisTransform.name}{path}";
			}
			return path;
		}
		
		public static Transform FindRecursive(this Transform self, string exactName) => self.FindRecursive(child => child.name == exactName);
		
		public static Transform FindRecursive(this Transform self, Func<Transform, bool> selector)
		{
			foreach (Transform child in self)
			{
				if (selector(child))
					return child;

				var finding = child.FindRecursive(selector);

				if (finding != null)
					return finding;
			}
			return null;
		}
	}
}
