/*===============================================================
* Product:		Com2Verse
* File Name:	C2VResourceLocatorExtensions.cs
* Developer:	tlghks1009
* Date:			2023-02-28 15:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Com2Verse.AssetSystem
{
	public static class ResourceLocatorExtensions
	{
		public static IEnumerable<IResourceLocation> LocateAll(this IEnumerable<IResourceLocator> locators, eAssetBundleType key)
		{
			foreach (var locator in locators)
			{
				if (locator.Locate(key.ToString(), typeof(object), out var locations))
				{
					foreach (var location in locations)
					{
						yield return location;
					}
				}
			}
		}
	}
}
