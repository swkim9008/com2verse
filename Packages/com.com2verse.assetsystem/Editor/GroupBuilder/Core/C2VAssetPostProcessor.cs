/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAssetPostprocessor.cs
* Developer:	tlghks1009
* Date:			2023-03-03 16:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEditor;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAssetPostProcessor : AssetPostprocessor
	{
		// FIXME : 당장 사용하지 않지만, 추 후 사용 가능성이 있어 주석처리 하였습니다.


		// private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		// {
		// 	return;
		//
		// 	if (EditorApplication.isCompiling || EditorApplication.isUpdating)
		// 	{
		// 		return;
		// 	}
		//
		// 	var compositionRoot = C2VAddressablesEditorCompositionRoot.RequestInstance();
		// 	var storageService  = compositionRoot.ServicePack.GetService<C2VAddressablesGroupBuilderStorageService>();
		//
		// 	if (storageService?.IsAutoPackaging() != true)
		// 	{
		// 		compositionRoot.Dispose();
		// 		return;
		// 	}
		//
		// 	var groupBuilderController = compositionRoot.GroupBuilderController;
		// 	var entryOperationDictionary = new C2VEntryOperationDictionary();
		//
		// 	groupBuilderController.ProcessInitialize();
		//
		// 	for (int i = 0; i < movedAssets.Length; i++)
		// 	{
		// 		var movedAsset = movedAssets[i];
		// 		var movedFromAssetPath = movedFromAssetPaths[i];
		//
		// 		groupBuilderController.ProcessMovedAsset(movedAsset, movedFromAssetPath, entryOperationDictionary);
		// 	}
		//
		// 	groupBuilderController.ProcessApply(entryOperationDictionary);
		//
		//
		// 	foreach (var importedAsset in importedAssets)
		// 	{
		// 		groupBuilderController.ProcessImportedAsset(importedAsset, entryOperationDictionary);
		// 	}
		//
		// 	groupBuilderController.ProcessApply(entryOperationDictionary);
		//
		//
		// 	foreach (var deletedAsset in deletedAssets)
		// 	{
		// 		groupBuilderController.ProcessDeletedAsset(deletedAsset, entryOperationDictionary);
		// 	}
		//
		// 	groupBuilderController.ProcessApply(entryOperationDictionary);
		//
		// 	compositionRoot.Dispose();
		// }
	}
}
