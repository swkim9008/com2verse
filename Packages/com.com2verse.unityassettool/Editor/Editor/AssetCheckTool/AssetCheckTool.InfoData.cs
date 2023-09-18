using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Com2VerseEditor.UnityAssetTool
{
    public partial class AssetCheckTool : EditorWindow
    {
        class InfoData
        {
            public virtual void Render() {}
        }


        class InfoDataMaskableGraphic : InfoData
        {
            readonly string _uiMaterialCommonPath = "Assets/BundleResources/Shader/DontLoad/UI/Material"; //UI에 공통으로 사용되는 재질 경로

            public MaskableGraphic MaskableGraphic { get; }
            public Material Material { get; }

            public InfoDataMaskableGraphic(MaskableGraphic maskableGraphic)
            {
                MaskableGraphic = maskableGraphic;
                this.Material = maskableGraphic.material == null ? null : maskableGraphic.material;
            }

            public override void Render()
            {
                if (this.MaskableGraphic == null) return;

                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.ObjectField(this.MaskableGraphic.gameObject, typeof(GameObject), true, GUILayout.Width(200));
                //EditorGUILayout.LabelField(this.MaskableGraphic.gameObject.name);
                EditorGUILayout.LabelField(this.MaskableGraphic.material.name);
                EditorGUILayout.ObjectField(this.Material, typeof(GameObject), true, GUILayout.Width(200));

                GUI.color = IsCommonMaterial() ? Color.gray : Color.yellow;
                EditorGUI.BeginDisabledGroup(IsCommonMaterial());
                if (GUILayout.Button("Move", GUILayout.Height(30), GUILayout.Width(100)))
                {
                    MoveMaterialToCommonPath();
                }
                GUI.color = Color.white;
                EditorGUI.EndDisabledGroup();
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }


            bool IsCommonMaterial()
            {
                if (this.Material.name == "Default UI Material") return true;

                string filePath = Path.Combine(_uiMaterialCommonPath, $"{this.Material.name}.mat");

                return File.Exists(filePath);
            }

            void MoveMaterialToCommonPath()
            {
                string oldPath = AssetDatabase.GetAssetPath(this.Material);
                string newPath = Path.Combine(_uiMaterialCommonPath, $"{this.Material.name}.mat");

                AssetDatabase.MoveAsset(oldPath, newPath);
            }
        }
    }
}