/*===============================================================
* Product:		Com2Verse
* File Name:	SpriteIconDrawer.cs
* Developer:	tlghks1009
* Date:			2023-05-24 10:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.UI;

namespace Com2verseEditor.UI
{
    public class SpriteDrawer : HierarchyIconController.HierarchyIconDrawer
    {
        private static Dictionary<string, Color> _spriteColorDictionary;

        private static Dictionary<string, Color> _colorMap = new()
        {
            {nameof(eAssetBundleType.BUILT_IN), Color.black},
            {nameof(eAssetBundleType.MICE), Color.red},
            {nameof(eAssetBundleType.OFFICE), Color.yellow},
            {nameof(eAssetBundleType.WORLD), Color.green},
            {nameof(eAssetBundleType.COMMON), Color.blue},
        };

        private Image _image;

        public SpriteDrawer(Texture defaultTexture) : base(defaultTexture)
        {
            _spriteColorDictionary = new Dictionary<string, Color>();

            InitializeAtlasGroupData();
        }

        public override bool TryInitialize(int instanceId)
        {
            InstanceId = instanceId;

            var objInHierarchy = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

            if (objInHierarchy.IsUnityNull())
            {
                return false;
            }

            _image = objInHierarchy.GetComponent<Image>();

            if (_image.IsUnityNull())
            {
                return false;
            }

            return _image.sprite != null;
        }

        public override bool TryDrawHierarchyIcon(Rect selectedRect)
        {
            var sprite    = _image.sprite;
            var assetPath = AssetDatabase.GetAssetPath(sprite);

            var originColor = GUI.color;
            if (_spriteColorDictionary != null && _spriteColorDictionary.TryGetValue(assetPath, out var color))
            {
                GUI.color = color;
                GUI.DrawTexture(selectedRect, base.DefaultTexture);

                GUI.color = originColor;

                return true;
            }

            return false;
        }

        private void InitializeAtlasGroupData()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }

            var atlasExtensionName = ".spriteatlasv2";

            foreach (var group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                var assetBundleType = string.Empty;

                foreach (var assetEntry in group.entries)
                {
                    var extension = Path.GetExtension(assetEntry.AssetPath);
                    if (extension != atlasExtensionName)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(assetBundleType))
                    {
                        var found = FindAssetBundleType(assetEntry, out assetBundleType);
                        if (!found)
                        {
                            continue;
                        }
                    }

                    if (_colorMap.TryGetValue(assetBundleType, out var color))
                    {
                        foreach (var spritePath in AssetDatabase.GetDependencies(assetEntry.AssetPath))
                        {
                            _spriteColorDictionary.TryAdd(spritePath, color);
                        }
                    }
                }
            }
        }


        private bool FindAssetBundleType(AddressableAssetEntry assetEntry, out string result)
        {
            foreach (eAssetBundleType assetBundleType in Enum.GetValues(typeof(eAssetBundleType)))
            {
                var found = assetEntry.labels.TryGetValue(assetBundleType.ToString(), out var value);
                if (found)
                {
                    result = value;
                    return true;
                }
            }

            result = string.Empty;
            return false;
        }
    }
}
