using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace Com2VerseEditor.UnityAssetTool
{
    public partial class AssetCheckTool : EditorWindow
    {
        [MenuItem("Com2Verse/ART/AssetCheck Tool")]
        static void Init()
        {
            AssetCheckTool window = (AssetCheckTool)EditorWindow.GetWindow(typeof(AssetCheckTool));
            window.Show();
            window.minSize = new Vector2(220, 500);
        }

        public interface IAssetInfo
        {
            void Render();
        }


        [System.Serializable]
        public class TextureInfo : IAssetInfo
        {
            private string[] _maxSize = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096" };

            public Texture texture;
            public string path;
            public int textureWidth;
            public int textureHeight;
            public bool mipMap;
            public bool alphaIsTransparency;

            public bool overrideForAndroid;
            public int android_maxTextureSize;
            public int android_selectedmaxSize;
            public TextureImporterFormat android_textureFormat;
            public int android_compressionQuality;

            public bool overrideForIphone;
            public int iPhone_maxTextureSize;
            public int iPhone_selectedmaxSize;
            public TextureImporterFormat iPhone_textureFormat;
            public int iPhone_compressionQuality;

            public bool etc1AlphaSplitEnabled;
            private TextureImporter _textureImporter;
            private TextureImporterPlatformSettings _androidSettings;
            private TextureImporterPlatformSettings _iPhoneSettings;

            public TextureInfo(string guid)
            {
                this.path = AssetDatabase.GUIDToAssetPath(guid);

                this.texture = AssetDatabase.LoadAssetAtPath(this.path, typeof(Texture)) as Texture;

                this.textureWidth = texture.width;
                this.textureHeight = texture.height;

                this._textureImporter = (TextureImporter)AssetImporter.GetAtPath(this.path);
                this.mipMap = _textureImporter.mipmapEnabled;
                this.alphaIsTransparency = _textureImporter.alphaIsTransparency;
                string plaform = Application.platform.ToString();
                /** "Standalone", "iPhone", "Android", "WebGL", "Windows Store Apps", "Tizen", "PSP2", "PS4", "XboxOne", "Samsung TV", "Nintendo 3DS", "WiiU", "tvOS" **/

                this.overrideForAndroid = _textureImporter.GetPlatformTextureSettings("Android", out this.android_maxTextureSize, out this.android_textureFormat, out this.android_compressionQuality, out this.etc1AlphaSplitEnabled);
                this.overrideForIphone = _textureImporter.GetPlatformTextureSettings("iPhone", out this.iPhone_maxTextureSize, out this.iPhone_textureFormat, out this.iPhone_compressionQuality);
                this.android_selectedmaxSize = ArrayUtility.IndexOf<string>(this._maxSize, this.android_maxTextureSize.ToString());
                this.iPhone_selectedmaxSize = ArrayUtility.IndexOf<string>(this._maxSize, this.iPhone_maxTextureSize.ToString());
            }


            public void Render()
            {
                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(64));

                EditorGUILayout.BeginHorizontal(GUILayout.Width(220));
                EditorGUILayout.ObjectField(this.texture, typeof(Texture), true, GUILayout.Width(64), GUILayout.Height(64));
                EditorGUILayout.BeginVertical();
                GUILayout.Label(string.Format("{0}", Path.GetFileNameWithoutExtension(this.path)));
                GUILayout.Label(string.Format("Size : {0} x {1}", this.textureWidth, this.textureHeight));
                GUILayout.Label(string.Format("Alpha : {0}", _textureImporter.DoesSourceTextureHaveAlpha()));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(GUILayout.Width(500), GUILayout.Height(64));
                //Android -----------------------------------------------------------------------------
                EditorGUILayout.BeginHorizontal("Box", GUILayout.ExpandHeight(true));
                this.overrideForAndroid = GUILayout.Toggle(this.overrideForAndroid, " Andoid", GUILayout.Width(100));
                this.android_selectedmaxSize = EditorGUILayout.Popup(this.android_selectedmaxSize, this._maxSize, GUILayout.Width(100));
                if (GUILayout.Button(new GUIContent("A", "AUTO Size"), GUILayout.Width(20))) { }    //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                if (this.android_textureFormat == TextureImporterFormat.RGBA32) { GUI.color = Color.red; }
                this.android_textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup(this.android_textureFormat, GUILayout.Width(150));
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("RGB", TextureImporterFormat.ETC2_RGB4.ToString()), GUILayout.Width(50))) { this.android_textureFormat = TextureImporterFormat.ETC2_RGB4; }
                if (GUILayout.Button(new GUIContent("RGBA", TextureImporterFormat.ETC2_RGBA8.ToString()), GUILayout.Width(50))) { this.android_textureFormat = TextureImporterFormat.ETC2_RGBA8; }
                EditorGUILayout.EndHorizontal();

                //GUILayout.FlexibleSpace();

                //iPhone ------------------------------------------------------------------------------
                EditorGUILayout.BeginHorizontal("Box", GUILayout.ExpandHeight(true));
                this.overrideForIphone = GUILayout.Toggle(this.overrideForIphone, " iPhone", GUILayout.Width(100));

                this.iPhone_selectedmaxSize = EditorGUILayout.Popup(this.iPhone_selectedmaxSize, this._maxSize, GUILayout.Width(100));
                if (GUILayout.Button(new GUIContent("A", "AUTO Size"), GUILayout.Width(20))) { }    //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                if (this.iPhone_textureFormat == TextureImporterFormat.RGBA32) { GUI.color = Color.red; }
                this.iPhone_textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup(this.iPhone_textureFormat, GUILayout.Width(150));

                GUI.color = Color.white;
                if (GUILayout.Button(new GUIContent("RGB", TextureImporterFormat.ASTC_6x6.ToString()), GUILayout.Width(50))) { this.iPhone_textureFormat = TextureImporterFormat.ASTC_6x6; }
                if (GUILayout.Button(new GUIContent("RGBA", TextureImporterFormat.ASTC_6x6.ToString()), GUILayout.Width(50))) { this.iPhone_textureFormat = TextureImporterFormat.ASTC_6x6; }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                //Common -----------------------------------------------------------------------------
                EditorGUILayout.BeginVertical("box", GUILayout.Height(64));
                this.mipMap = GUILayout.Toggle(this.mipMap, " Mipmap");
                this.alphaIsTransparency = GUILayout.Toggle(this.alphaIsTransparency, " Alpha Is Transparency");
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Apply", GUILayout.Height(30), GUILayout.Width(100)))
                {
                    //Android
                    this._androidSettings = new TextureImporterPlatformSettings();
                    this._androidSettings.name = "Android";
                    this._androidSettings.overridden = this.overrideForAndroid;
                    this._androidSettings.maxTextureSize = this.android_maxTextureSize;
                    this._androidSettings.format = this.android_textureFormat;
                    this._androidSettings.compressionQuality = this.android_compressionQuality;
                    this._androidSettings.allowsAlphaSplitting = this.etc1AlphaSplitEnabled;
                    this._androidSettings.maxTextureSize = int.Parse(this._maxSize[this.android_selectedmaxSize]);

                    this._textureImporter.SetPlatformTextureSettings(this._androidSettings);

                    //iPhone
                    this._iPhoneSettings = new TextureImporterPlatformSettings();
                    this._iPhoneSettings.name = "iPhone";
                    this._iPhoneSettings.overridden = this.overrideForIphone;
                    this._iPhoneSettings.maxTextureSize = this.iPhone_maxTextureSize;
                    this._iPhoneSettings.format = this.iPhone_textureFormat;
                    this._iPhoneSettings.compressionQuality = this.iPhone_compressionQuality;
                    this._iPhoneSettings.maxTextureSize = int.Parse(this._maxSize[this.iPhone_selectedmaxSize]);

                    this._textureImporter.SetPlatformTextureSettings(this._iPhoneSettings);

                    //Common
                    this._textureImporter.mipmapEnabled = this.mipMap;
                    this._textureImporter.SaveAndReimport();
                }

                EditorGUILayout.EndHorizontal();
            }

        }


        [System.Serializable]
        public class MaterialInfo : IAssetInfo
        {
            public string name;
            public string path;
            public Material material;
            public Shader shader;

            public bool useMatCap;
            public bool useMatCapTexture;

            public MaterialInfo(string guid)
            {
                this.path = AssetDatabase.GUIDToAssetPath(guid);
                this.material = AssetDatabase.LoadAssetAtPath(this.path, typeof(Material)) as Material;

                this.useMatCap = this.material.HasProperty("_Matcap");
                this.useMatCapTexture = this.material.HasProperty("_MatCapTex");

                this.shader = this.material.shader;

                this.name = this.material.name;
            }

            public void Render()
            {
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.BeginVertical(GUILayout.Width(200));
                EditorGUILayout.ObjectField(this.material, typeof(Material), true);
                EditorGUILayout.ObjectField(this.shader, typeof(Shader), true);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal("box");
                int propertyCount = ShaderUtil.GetPropertyCount(material.shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    string propertyName = ShaderUtil.GetPropertyName(material.shader, i);

                    EditorGUILayout.BeginVertical(GUILayout.Width(150));
                    GUILayout.Label(propertyName);

                    switch (ShaderUtil.GetPropertyType(material.shader, i))
                    {
                        case ShaderUtil.ShaderPropertyType.Color:
                            {
                                Color value = material.GetColor(propertyName);
                                EditorGUILayout.ColorField(value);
                                break;
                            }
                        case ShaderUtil.ShaderPropertyType.Float:
                            {
                                float value = material.GetFloat(propertyName);
                                EditorGUILayout.FloatField(value);
                                break;
                            }
                        case ShaderUtil.ShaderPropertyType.Range:
                            {
                                float value = material.GetFloat(propertyName);
                                EditorGUILayout.FloatField(value);
                                break;
                            }
                        case ShaderUtil.ShaderPropertyType.Vector:
                            {
                                Vector4 value = material.GetVector(propertyName);
                                EditorGUILayout.Vector4Field("", value);
                                break;
                            }
                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            {
                                Texture value = material.GetTexture(propertyName);

                                EditorGUILayout.ObjectField(value, typeof(Texture), true);
                                break;
                            }
                    }
                    EditorGUILayout.EndVertical();
                }
                //GUILayout.Toggle(this.useMatCap, "Use MatCap", GUILayout.Width(100));
                //if (this.useMatCap) { GUILayout.Label(this.material.GetFloat("_Matcap").ToString()); }
                //if (this.useMatCapTexture) { EditorGUILayout.ObjectField(this.material.GetTexture("_MatCapTex"), typeof(Texture), true); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndHorizontal();



                //EditorGUILayout.ObjectField(this.material.mainTexture, typeof(Texture), true, GUILayout.Width(100), GUILayout.Height(100));


            }
        }

        [System.Serializable]
        public class MaterialObjInfo
        {
            public string name;
            public string path;

            public MeshRenderer meshRenderer;
            public Material material;

            public MaterialObjInfo(MeshRenderer _meshRenderer)
            {
                this.name = _meshRenderer.name;
                this.path = "";
                this.material = _meshRenderer.material;
                this.meshRenderer = _meshRenderer;
            }

            public void Render()
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.BeginVertical();
                GUILayout.Label(this.name);
                EditorGUILayout.ObjectField(this.meshRenderer, typeof(Mesh), true);
                EditorGUILayout.ObjectField(this.material, typeof(Material), true);
                EditorGUILayout.EndVertical();
                EditorGUILayout.ObjectField(this.material.mainTexture, typeof(Texture), true, GUILayout.Width(100), GUILayout.Height(100));

                EditorGUILayout.EndHorizontal();
            }
        }



        private int _menuTab = 0;
        private Vector2 _materialInfoScrollViewPos;
        public List<MaterialObjInfo> materialInfoList;     //검색된 재질의 리스트
        private Vector2 _assetInfoScrollViewPos;
        public readonly List<IAssetInfo> AssetInfoList = new List<IAssetInfo>();     //어셋의 리스트


        void OnGUI()
        {
            this._menuTab = GUILayout.Toolbar(this._menuTab, new string[] { "Asset List Save", "Check GameObject", "Check GameObject" }, GUILayout.Height(30));

            switch (this._menuTab)
            {
                case 0:
                    EditorGUILayout.BeginVertical();
                    TabAssetListSave();
                    EditorGUILayout.EndVertical();
                    break;
                case 1:
                    EditorGUILayout.BeginVertical();
                    //TabGameObjectCheck();
                    EditorGUILayout.EndVertical();
                    break;
                case 2:
                    EditorGUILayout.BeginVertical();
                    //TabCheckMaterial();
                    EditorGUILayout.EndVertical();
                    break;
            }
        }



        // CheckGameAsset Tab ----------------------------------------------------------------------------------------------------------
        void TabCheckAsset()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Texture Info", GUILayout.Height(30))) { FindFromSelection<Texture, TextureInfo>(); }
            if (GUILayout.Button("Material Info", GUILayout.Height(30))) { FindFromSelection<Material, MaterialInfo>(); }

            if (GUILayout.Button("Material None", GUILayout.Height(30))) { MaterialImportSetting(); }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("리스트 초기화", GUILayout.Height(30), GUILayout.Width(100))) { this.AssetInfoList.Clear(); }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (this.AssetInfoList == null || this.AssetInfoList.Count <= 0) return;

            this._assetInfoScrollViewPos = EditorGUILayout.BeginScrollView(this._assetInfoScrollViewPos);
            foreach (IAssetInfo assetInfo in this.AssetInfoList)
            {
                assetInfo.Render();
            }
            EditorGUILayout.EndScrollView();
        }


        // CheckGameObject Tab ----------------------------------------------------------------------------------------------------------
        void TabCheckMaterial()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Material List", GUILayout.Height(30))) { GetMaterialInfoFromSelection(); }
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("리스트 초기화", GUILayout.Height(30), GUILayout.Width(100))) { this.materialInfoList.Clear(); }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white;
            GUIStyle styleWarning = new GUIStyle("Label");
            if (this.materialInfoList == null || this.materialInfoList.Count <= 0) return;

            this._materialInfoScrollViewPos = EditorGUILayout.BeginScrollView(this._materialInfoScrollViewPos);

            foreach (MaterialObjInfo materialInfo in this.materialInfoList)
            {
                materialInfo.Render();
            }
            EditorGUILayout.EndScrollView();
        }


        private bool GetMaterialInfoFromSelection()
        {
            var rootObj = Selection.activeObject as GameObject;
            if (rootObj == null) return false;

            MeshRenderer[] meshRendererList = rootObj.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer meshRenderer in meshRendererList)
            {
                MaterialObjInfo materialInfo = new MaterialObjInfo(meshRenderer);
                this.materialInfoList.Add(materialInfo);
            }
            return true;
        }



        // AssetInfo -------------------------------------------------------------------------------------
        // -----------------------------------------------------------------------------------------------
        bool FindFromSelection<T, TInfoType>() where TInfoType : class
        {
            Object objSelect = Selection.activeObject;

            if (null == objSelect) return false;

            string typeName = objSelect.GetType().ToString();
            if (typeName != "UnityEditor.DefaultAsset") return false;

            string[] pathList = { AssetDatabase.GetAssetPath(objSelect) };
            string[] texturePathGuidList = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name), pathList);

            if (texturePathGuidList.Length <= 0) return false;

            this.AssetInfoList.Clear();

            foreach (string guid in texturePathGuidList)
            {
                this.AssetInfoList.Add(System.Activator.CreateInstance(typeof(TInfoType), new object[] { guid }) as IAssetInfo);
            }

            // Sort Atlas Name * Sprte Name
            if (typeof(MaterialInfo).IsSubclassOf(typeof(TInfoType)) || typeof(MaterialInfo).IsAssignableFrom(typeof(TInfoType)))
            {
                this.AssetInfoList.Sort((aValue, bValue) =>
                {
                    var valueA = aValue as MaterialInfo;
                    var valueB = bValue as MaterialInfo;
                    int resultA = string.Compare(valueA.shader.name, valueB.shader.name);
                    if (resultA == 0) { return string.Compare(valueA.material.name, valueB.material.name); }
                    return resultA;
                });
            }

            return true;
        }
        // ---------------------------------------------------------------------------------------------

        void MaterialImportSetting()
        {
            Object[] objSelection = Selection.objects;
            foreach (Object obj in objSelection)
            {
                string targetPath = AssetDatabase.GetAssetPath(obj);
                ModelImporter targetModelImporter = (ModelImporter)AssetImporter.GetAtPath(targetPath);

                targetModelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
        }




        // CSV 저장 ---------------------------------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------------------
        private static bool WriteCsvFile(string targetFileName, string outText)
        {
            string fileName = string.Format($"{targetFileName}_{System.DateTime.Now.ToString("yyyyMMdd")}");
            string defaultPath = Directory.GetParent((Directory.GetParent(Application.dataPath).FullName)).FullName;

            string targetOutPath = EditorUtility.SaveFilePanel("Select Save folder", defaultPath, fileName, "csv");
            if (targetOutPath.Length != 0)
            {
                File.WriteAllText(targetOutPath, outText, Encoding.UTF8);
            }

            System.Diagnostics.Process.Start("explorer.exe ", defaultPath + "\\");

            return true;
        }

        private void OnInspectorUpdate() { Repaint(); }
    }
}