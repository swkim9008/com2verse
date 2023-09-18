/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGroupBuilderServiceBase.cs
* Developer:	tlghks1009
* Date:			2023-03-31 14:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEditor;

namespace Com2VerseEditor.AssetSystem
{
	public abstract class C2VAddressablesGroupBuilderServiceBase
	{
		protected IServicePack ServicePack { get; }

		protected C2VAddressablesGroupBuilderServiceBase(IServicePack servicePack) => ServicePack = servicePack;

		public virtual void Initialize() { }

		public virtual void Release() { }


		protected void SaveAssets(UnityEngine.Object dirtyObject)
		{
			if (dirtyObject != null)
			{
				EditorUtility.SetDirty(dirtyObject);
			}
			SaveAssets();
		}

		protected void SaveAssets()
		{
			var assetDataBaseAdapter = ServicePack.GetAdapterPack()?.AssetDataBaseAdapter;
			assetDataBaseAdapter?.SaveAssets();
			assetDataBaseAdapter?.Refresh();
		}
	}
}
