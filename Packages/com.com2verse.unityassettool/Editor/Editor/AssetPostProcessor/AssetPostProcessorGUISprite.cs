using UnityEngine;
using UnityEditor;

namespace Com2VerseEditor.UnityAssetTool
{
    public sealed class AssetPostProcessorGUISprite : AssetPostprocessor
    {
        //private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        //{
        //    // TODO : 에셋 처리 방식 확인 후 코드 수정
        //    return;

        //    foreach (var asset in importedAssets) ProcessAssetLabels(asset);
        //    foreach (var asset in movedAssets) ProcessAssetLabels(asset);
        //}


        //private static void ProcessAssetLabels(string asset)
        //{
        //    // TODO : 에셋 처리 방식 확인 후 코드 수정
        //    return;

        //    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset);

        //    //var labels = AssetDatabase.GetLabels(obj);
        //    //var labelsArray = new string[] { string.Join('-', labels) };
        //    if (!(obj is Texture)) return;

        //    if (asset.Contains("GUISprite"))
        //    {
        //        AssetDatabase.SetLabels(obj, new string[] { "GUISprite" });
        //    }
        //    else
        //    {
        //        AssetDatabase.ClearLabels(obj);
        //    }
        //}


        //private void OnPreprocessTexture()
        //{
        //    // TODO: 에셋 처리 방식 확인 후 코드 수정
        //    return;

        //    if (!assetPath.Contains("GUISprite")) return;

        //    var textureImporter = assetImporter as TextureImporter;
        //    if (!textureImporter)
        //        return;

        //    //Debug.Log($"IMPORT!!! Texture:{assetPath}");
        //    textureImporter.textureType = TextureImporterType.Sprite;

        //    textureImporter.mipmapEnabled = false;

        //    TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
        //    textureImporter.ReadTextureSettings(textureImporterSettings);

        //    textureImporterSettings.spriteMeshType = SpriteMeshType.FullRect;
        //    textureImporterSettings.spriteGenerateFallbackPhysicsShape = false;

        //    textureImporter.SetTextureSettings(textureImporterSettings);
        //}


        //private void OnPostprocessTexture(Texture2D texture)
        //{
        //}
    }
}