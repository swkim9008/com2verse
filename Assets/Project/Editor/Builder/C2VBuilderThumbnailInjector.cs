/*===============================================================
* Product:		Com2Verse
* File Name:	C2VBuilderThumbnailInjector.cs
* Developer:	yangsehoon
* Date:			2023-05-25 14:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Builder
{
	[CreateAssetMenu(fileName = "C2VBuilderThumbnailInjector", menuName = "Com2Verse/Builder/ThumbnailInjector")]
	public sealed class C2VBuilderThumbnailInjector : ScriptableObject
	{
	    [SerializeField] public ThumbnailDatabase BuilderThumbnailDatabase;
	}
}