/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGroupBuilderAdapterPack.cs
* Developer:	tlghks1009
* Date:			2023-03-31 14:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2VerseEditor.AssetSystem
{
	public class C2VAddressablesGroupBuilderAdapterPack
	{
		public C2VAssetDataBaseAdapter AssetDataBaseAdapter { get; private set; }

		public C2VAddressablesEditorAdapter AddressablesEditorAdapter { get; private set; }


		public C2VAddressablesGroupBuilderAdapterPack()
		{
			AssetDataBaseAdapter = new C2VAssetDataBaseAdapter();

			AddressablesEditorAdapter = new C2VAddressablesEditorAdapter();
		}

		public void Release()
		{
			AddressablesEditorAdapter.Release();
			AddressablesEditorAdapter = null;

			AssetDataBaseAdapter = null;
		}
	}
}
