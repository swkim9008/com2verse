/*===============================================================
* Product:    Com2Verse
* File Name:  SoundImporter.cs
* Developer:  yangsehoon
* Date:       2022-04-04 13:18
* History:    
* Documents:  Sound import postprocessor
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Com2Verse.Sound
{
    public class SoundImporter : AssetPostprocessor
    {
        private readonly long MaxInMemorySize = 1 << 20; // 1024KiB

        private void OnPreprocessAudio()
        {
            AudioImporter audioImporter = assetImporter as AudioImporter;
            AudioImporterSampleSettings defaultSampleSettings = audioImporter.defaultSampleSettings;

            string path = audioImporter.assetPath.ToLower();

            defaultSampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            defaultSampleSettings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            defaultSampleSettings.quality = 0.4f;

            if (path.Contains("bgm"))
            {
                audioImporter.loadInBackground = true;
                defaultSampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                defaultSampleSettings.loadType = AudioClipLoadType.Streaming;
            }
            else if (path.Contains("sfx") || path.Contains("se"))
            {
                audioImporter.forceToMono = true;
                defaultSampleSettings.loadType = AudioClipLoadType.CompressedInMemory;

                // if original file is too big
                if (new FileInfo(path).Length > MaxInMemorySize)
                {
                    defaultSampleSettings.loadType = AudioClipLoadType.CompressedInMemory;
                    defaultSampleSettings.compressionFormat = AudioCompressionFormat.ADPCM;
                }
            }
            
            // overwrite settings if sound is important
            if (path.Contains("important"))
            {
                audioImporter.forceToMono = false;
            }

            audioImporter.defaultSampleSettings = defaultSampleSettings;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string movedAsset in movedAssets)
            {
                if (movedAsset.Contains("Sound"))
                {
                    AssetDatabase.ImportAsset(movedAsset);
                }
            }
        }
    }
}
#endif