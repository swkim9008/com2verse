using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEngine.UI;
using TMPro;
using Sentry.Extensibility;
using System.Security.Policy;
using Google.Protobuf.WellKnownTypes;
using System.Web;

namespace Com2VerseEditor.UnityAssetTool
{
    public partial class AssetCheckTool : EditorWindow
    {
        // 각 리스트의 항목 설명용 변수.고훈(180827)
#region 각 리스트의 항목 설명 String.
        private readonly string _descriptionModelList = "항목,설명\n" +
                                                        "Sub Mesh Count,여러 개의 Node로 분리되어 Export된 Mesh입니다. Batching이 일어나지 않으면 Draw 성능을 저하시킵니다.\n" +
                                                        "Vertex Count,Mesh의 정점숫자 입니다.\n" +
                                                        "Triangles Count,Mesh의 Tri(삼각면) 숫자입니다.\n" +
                                                        "VPT,Vertex Count per Triangles Count 값입니다. Hard Surface가 많을 값이 큽니다. 이 경우 Vertex가 분리 되므로 용량과 메모리가 증가합니다. (6면체에 Hard Surface =0 일경우 Vertex Count = 4. Hard Surface = 6 일경우 Vertex count = 24)\n" +
                                                        "Mesh Compression,용량에 영향을 줍니다. 열화가 발생하지 않는 수준까지 최대한 압축합니다. (3D Max 1unit = 1cm 인 경우 Unity 에서는 0.01 이므로 3D Max Interface에서 접근 가능한 0.001 은 Unity에서 0.00001 값이 됩니다.)\n" +
                                                        "Optimize Model,Mesh의 Tri 순서를 정렬합니다.\n" +
                                                        "Animation Type,Static Mesh의 경우 None 으로 설정합니다.\n" +
                                                        "BoneCount,Skined Renderer를 사용하는 Mesh의 Bone Count 입니다. 적을 수록 성능과 용량 및 메모리에 유리합니다.";

        private readonly string _descriptionAnimclipList = "항목,설명\n" +
                                                           "FrameRate,AnimationClip 초당 재생빈도 입니다.\n" +
                                                           "Length,AnimationClip의 재생시간 입니다.\n" +
                                                           "keyState,FrameRate x Length 값을 이용하여 AnimationClip의 전체 Frame Count를 계산합니다. 숫자가 있을 경우 전체 Frame Count와 동일한 Key Frame이 있다고 추측합니다. 최적화 하여 용량 및 메모리를 줄일 수 있습니다.\n" +
                                                           "Triangle Pelvis,Thigh Node가 Spine Node에 붙어 있을 경우 3D Max 의 Bip 옵션 중 Triangle pelvis 옵션이 켜져 있다고 판단합니다. 이 경우 Thigh Node에 0보다 큰 Local Positon 값이 생성 되므로 최적화 시 열화가 눈에 띄이게 발생하거나 적절히 최적화 되지 않을 수 있습니다.\n" +
                                                           "Single Key,Key가 하나만 있는 Curve의 숫자입니다. Key가 하나만 있을 경우 오류가 날 수 있으므로 필요없는 Key라면 지워주거나 Key를 하나 더 만들어 Key가 최소 2개가 될 수 있게 해 주는 것이 좋습니다.\n" +
                                                           "Animation Type,Animation Import 형식을 정의 합니다.\n" +
                                                           "Anim. Compression,Import시 Error값을 기준으로 Key를 줄입니다. 용량과 메모리를 아낄수 있습니다. Animation Type 이 Generic 또는 Human일경우 Optimal을 사용하면 메모리를 아낄 수 있습니다.";

        private readonly string _descriptionTextureList = "항목,설명\n" +
                                                          "Override for Android,안드로이드용 텍스춰 옵션이 따로 정리되어 있는 있는지 확인합니다.\n" +
                                                          "Width,텍스춰의 가로 사이즈 입니다.\n" +
                                                          "Height,텍스춰의 세로 사이즈 입니다.\n" +
                                                          "MaxSize,Import시 최대 사이즈 입니다. Width 값과 Height 값에 따라 적절히 수정하도록 합니다.\n" +
                                                          "NPOT Scale,가로 세로의 크기가 2의 배승이 아닌 경우의 설정값입니다. None일 경우 원본비율을 사용합니다. NPOT일 경우 로딩시 메모리를 2배로 사용합니다.\n" +
                                                          "Format,로딩속도 및 용량과 사용 메모리에 영향을 줍니다.\n" +
                                                          "Compression Quality,압축시 퀄리티 입니다.기본값은 50입니다.로딩속도에 영향을 줍니다.\n" +
                                                          "etc1AlphaSplit,알파 채널있는 ETC1 포맷을 사용할경우 자동으로 알파 채널을 분리해 줍니다.\n" +
                                                          "Mipmap,UI에 사용되는 텍스춰의 경우 Mipmap을 끄도록 합니다.";

        private readonly string _descriptionVFXList = "항목,설명\n" +
                                                      "Particle System Count,Effect Prefab 내에 Particle System의 숫자를 나타냅니다. 지나치게 큰 수치는 메모리와 성능에 영향을 줄 수 있습니다.\n" +
                                                      "Render Mode,Mesh Mode일 경우 성능 저하가 발생할 수 있습니다.Mesh Mode는 Sort Mode의 By Distance Mode와 유사한 비용을 발생시킵니다.\n" +
                                                      "Sort Mode,None < Oldest in Front = Youngest in Front < By Distance 순으로 비용이 발생하므로 적절히 선택하여 사용합니다.\n" +
                                                      "Looping,Particle이 반복하여 생성되는지 확인합니다.\n" +
                                                      "Max Particle,생성되는 최대 Particle Count입니다.이 숫자를 넘을 경우 먼저 생성된 순서로 사라집니다.Loopin이 켜져 있고 Start Life Time이 지나치게 클 경우 성능에 영향을 줄 수 있습니다.\n" +
                                                      "Start Life Time,생성시 설정된 Particle의 수명입니다.Looping 옵션이 켜져 있고 Life Time이 지나치게 클 경우 화면에 많은 Particle 이 생성되어 성능에 영향을 줄 수 있습니다.\n" +
                                                      "Shadow Casting Mode,조명에 영향을 받지 않을 경우 꺼두는 것이 좋습니다.\n" +
                                                      "Receive Shadow,조명에 영향을 받지 않을 경우 꺼두는 것이 좋습니다.";

        private readonly string _descriptionAudioClipList = "항목,설명\n" +
                                                            "Src Type,원본 파일의 Format 입니다.\n" +
                                                            "Compression Format,Runtime에 사용되는 Formet 입니다.PCM 의 경우 품질은 높지만 크기 및 메모리 사용량이 증가하므로 짧은 효과음 등에만 사용합니다.ADPCM의 경우 압축량이 PCM보다 3.5배 작지만 CPU 사용량이 낮으므로 대량으로 재생되는 AudioClip에 사용합니다.\n" +
                                                            "LoadType,Streaming < Compressed In Memory < Decompress On Load  순으로 메모리 사용량이 증가합니다.일반적으로 빈번하게 사용되는 짧은 효과음 등은 Decompress On load 방식을 사용하고 중간 길이 정도의 사운드는 Compressed In Memory. BGM등 크고 플레이 시간이 긴 파일의 경우 Streaming을 사용합니다.\n" +
                                                            "Channels,원본 파일에 포함된 Channel의 숫자입니다.\n" +
                                                            "Force To Mono,Import 시 강재로 Mono로 변환 할지를 설정합니다.\n" +
                                                            "Quality,최대한 열화가 적도록 값을 낮추어 설정합니다.\n" +
                                                            "Length(s),재생시간입니다.\n" +
                                                            "Samples,전체 Sample 값입니다.\n" +
                                                            "Frequency(Hz),Sample rate 값 (1초당 추출되는 샘플 갯수) 입니다.\n" +
                                                            "Load In Background,설정될경우 Background 로딩을 합니다.재생요청은 로딩이 모드 끝난후 진행됩니다.\n" +
                                                            "Preload Audio Data,Scene이 로딩될때 사용하는 사운드를 모드 로딩해 놓습니다.";

        private readonly string _descriptionUIPrefabList = "항목,설명\n" +
                                                           "srcName, UILabel UISprite UITexture 컴포넌트에  Rendering을 위한 src 가 연결되어 있지 않습니다.사용하지 않는 경우 제거하는 것이 용량과 메모리에 도움이 됩니다.\n" +
                                                           "srcSizeIsBig, 표시되는 사이즈에 비하여 원본의 크기가 큽니다.적절한 사이즈의 스프라이트를 사용하면 메모리와 성능에 도움이 됩니다.\n" +
                                                           "OverRate(%), 표시되는 사이즈 대비 원본의 크기 비율입니다.지나치게 큰 경우 적절한 사이즈의 스프라이트를 추가하여 제작하는 것이 좋습니다. 100에 근접할 수록 좋습니다..\n";
        #endregion
        /// <summary>
        /// 폴더명에 "External", "Packages", "Editor", "Debug", "__Work", "Atlas" 있는 경우 첵크하지 않는다.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsSkip(string path)
        {
            string[] filter = new string[] { "External", "Packages", "Editor", "Debug", "__Work", "Atlas" };  //"NBA"

            for (int i = 0; i < filter.Length; i++)
            {
                if (path.Contains(filter[i])) return true;
            }

            return false;
        }

        // SaveList Tab ----------------------------------------------------------------------------------------------------------------------------
        void TabAssetListSave()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("UI Prefab List", EditorStyles.boldLabel);

            if (GUILayout.Button("UGUI Prefb List 저장", GUILayout.Height(30))) {  GetUGUIPrefabList(); }
            if (GUILayout.Button("UGUI Prefb Sprite List 저장", GUILayout.Height(30))) { GetUGUIPrefabSpriteList(); }

            //GetNGUIPrefabList();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Check Model", EditorStyles.boldLabel);
            if (GUILayout.Button("Mesh(FBX) List 저장", GUILayout.Height(30))) { GetModelList(); }
            if (GUILayout.Button("Animation Clip List 저장", GUILayout.Height(30))) { GetAnimationClipList(); }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Check Texture", EditorStyles.boldLabel);

            if (GUILayout.Button("Texture List 저장", GUILayout.Height(30))) { GetTextureList(); }

            

            //GetNPOTList();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Check EFFECT", EditorStyles.boldLabel);
            if (GUILayout.Button("Audio List 저장", GUILayout.Height(30))) { GetAudioList(); }
            if (GUILayout.Button("VFX List 저장", GUILayout.Height(30))) { GetVFXPropetiesList(); }
            EditorGUILayout.EndVertical();


            GUI.color = Color.yellow;
            if (GUILayout.Button("사용하는? 스프라이트 목록", GUILayout.Height(30))) { GetSpriteList(true); }
            if (GUILayout.Button("사용하지 않는? 스프라이트 목록", GUILayout.Height(30))) { GetSpriteList(false); }
            GUI.color = Color.white;

            GUI.color = Color.cyan;
            if (GUILayout.Button("Text LocalizationUI List 저장", GUILayout.Height(30))) { GetLocalizationUIListInGUIPrefab(); }
            GUI.color = Color.white;
        }


        private void GetUGUIPrefabList()
        {
            if (!EditorUtility.DisplayDialog("확인", "UGUI 프리펩 리스트 저장합니다. 오래결러요.", "저장", "취소")) return;

            string[] prefabGuidArray = AssetDatabase.FindAssets("t:Prefab");

            StringBuilder tempText = new StringBuilder();   //전체 텍스트
            StringBuilder temp = new StringBuilder();       //프리펩 정보 텍스트

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            tempText.Append("\n");
            tempText.AppendLine(this._descriptionUIPrefabList);
            tempText.Append("\n");
            tempText.AppendLine(",Path,PrefabName,NodeCount");
            tempText.AppendLine(",,NodeName,Type,srcName,material,shader,raycastTarget,width,height,src.width,src.height,srcSizeIsBig,OverRate(%)");

            int totalPrefabCount = 0;

            int num = prefabGuidArray.Length;     //전체 프리펩개수
            for (int i = 0; i < num; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuidArray[i]);

                //특정 폴더는 빼고 첵크
                if (prefabPath.Contains("Editor")) continue;
                if (prefabPath.Contains("__Work")) continue;
                //if (prefabPath.Contains("_NBA")) continue;
                if (prefabPath.Contains("Debug")) continue;

                if (IsSkip(prefabPath)) continue;

                GameObject targetPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));

                MaskableGraphic[] maskableGraphics = targetPrefab.GetComponentsInChildren<MaskableGraphic>(true);

                //프로그레스바 그리기. 고훈(18 - 08 - 24).
                if (Common.ShowProgressBar("GUI List 저장", Path.GetFileName(prefabPath), 1, num, i))
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }

                int jnum = maskableGraphics.Length;
                if (0 < jnum)   //UIRect 객체가 있을 경우 UGUI 객체라 판단.
                {
                    totalPrefabCount++;

                    temp.AppendLine($",{Path.GetDirectoryName(prefabPath)},{Path.GetFileName(prefabPath)},{jnum}");

                    //컴포넌트 별로 구분
                    foreach (MaskableGraphic maskableGraphic in maskableGraphics)
                    {
                        temp.Append($",,{maskableGraphic.name},");
                        
                        Rect useRect = maskableGraphic.rectTransform.rect;

                        string materialName = string.Empty;
                        string shaderName = string.Empty;

                        if (maskableGraphic.material != null)
                        {
                            materialName = maskableGraphic.material.name == "Default UI Material" ? string.Empty : maskableGraphic.material.name;
                            shaderName = maskableGraphic.material.shader.name == "UI/Default" ? string.Empty : maskableGraphic.material.shader.name;
                        }

                        if (maskableGraphic is Image)
                        {
                            Image tmpImage = maskableGraphic.GetComponent<Image>();

                            if (null != tmpImage.sprite)
                            {
                                Rect srcRect = tmpImage.sprite.rect;
                                
                                var sizeCheckValue = GetSizeRate(srcRect, useRect);

                                temp.AppendLine($"Image,{tmpImage.sprite.texture.name},{materialName},{shaderName},{maskableGraphic.raycastTarget},{useRect.width},{useRect.height},{srcRect.width},{srcRect.height},{sizeCheckValue.Item2},{sizeCheckValue.Item1.ToString("N2")}");
                            }
                            else
                            {
                                temp.AppendLine($"Image,miss,{materialName},{shaderName},{maskableGraphic.raycastTarget},{useRect.x},{useRect.y}"); 
                            }

                            continue;
                        }
                        else if (maskableGraphic is RawImage)
                        {
                            RawImage tmpTexture = maskableGraphic.GetComponent<RawImage>();

                            if (null != tmpTexture.texture)
                            {
                                Rect srcRect = new Rect(0, 0, tmpTexture.mainTexture.width, tmpTexture.mainTexture.height);

                                var sizeCheckValue = GetSizeRate(srcRect, useRect);

                                temp.AppendLine($"Texture,{tmpTexture.mainTexture.name},{materialName},{shaderName},{maskableGraphic.raycastTarget},{useRect.width},{useRect.height},{srcRect.width},{srcRect.height},{sizeCheckValue.Item2},{sizeCheckValue.Item1.ToString("N2")}");
                            }
                            else
                            {
                                temp.AppendLine($"Texture,miss,{materialName},{shaderName},{tmpTexture.raycastTarget}");
                            }

                            continue;
                        }
                        else if (maskableGraphic is Text)
                        {
                            Text tmpText = maskableGraphic.GetComponent<Text>();
                            string fontName = null == tmpText.font ? "miss?" : tmpText.font.name;
                            
                            //컨스텀 폰트 구분 가능하면 나중에 추가하자...
                            temp.AppendLine($"Text,{fontName},{materialName},{shaderName},{maskableGraphic.raycastTarget},BestFit: {tmpText.resizeTextForBestFit}");
                            continue;
                        }
                        else if (maskableGraphic is TextMeshProUGUI)
                        {
                            TextMeshProUGUI tmpText = maskableGraphic.GetComponent<TextMeshProUGUI>();
                            string fontName = null == tmpText.font ? "miss?" : tmpText.font.name;
                            
                            temp.AppendLine($"TMP,{fontName},{materialName},{shaderName},{maskableGraphic.raycastTarget},BestFit: {tmpText.autoSizeTextContainer}");
                            continue;
                        }


                        ParticleSystem tmpParticleSystem = maskableGraphic.GetComponent<ParticleSystem>();
                        if (null != tmpParticleSystem)
                        {
                            temp.Append("PARTICLE");
                            temp.Append("\n");
                            continue;
                        }

                        temp.Append("\n");
                    }
                }
            }
            
            EditorUtility.ClearProgressBar();   // 프로그레스바 지우기. 고훈(18-08-24).

            tempText.AppendLine($",Total UGUIPrefab: {totalPrefabCount}");
            tempText.Append(temp);

            WriteCsvFile("TAT_UGUIPrefabList", tempText.ToString());
        }



        private void GetModelList()
        {
            if (!EditorUtility.DisplayDialog("Confirm", "Mesh 리스트를 저장할까요?", "예", "아니요")) return;

            string[] tempGuidArray = AssetDatabase.FindAssets("t:Model") ?? throw new ArgumentNullException("AssetDatabase.FindAssets(\"t:Model\")");
            int num = tempGuidArray.Length;
            int totalMesh = 0;

            StringBuilder tempText = new StringBuilder();
            StringBuilder temp = new StringBuilder();

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            tempText.Append("\n");
            tempText.AppendLine(this._descriptionModelList);
            tempText.Append("\n");

            // 먼저 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            tempText.AppendLine(",Model Name,Sub Mesh Count,Vertex Count,Triangles Count,VPT,Mesh Compression,Read/Write Enabled,Optimize Model,Animation Type,BoneCount,Normals,Tangents");

            int vertexCount = 0;
            int polyCount = 0;

            for (int i = 0; i < num; i++)
            {
                string targetPath = AssetDatabase.GUIDToAssetPath(tempGuidArray[i]);

                if (IsSkip(targetPath)) continue;

                string subMeshLength = string.Empty;
                int subMeshCount = 0;

                Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(targetPath, typeof(Mesh));

                ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(targetPath);
                GameObject gameObject = (GameObject)AssetDatabase.LoadAssetAtPath(targetPath, typeof(GameObject));

                SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

                if (mesh != null)
                {
                    UnityEngine.Object[] targetSubObject = AssetDatabase.LoadAllAssetRepresentationsAtPath(targetPath);
                    List<Mesh> subMeshList = new List<Mesh>();

                    for (int j = 0; j < targetSubObject.Length; j++)
                    {
                        if (targetSubObject[j].GetType() == typeof(Mesh))
                        {
                            subMeshList.Add((Mesh)targetSubObject[j]);
                            subMeshCount++;
                        }
                    }

                    vertexCount = mesh.vertexCount;
                    polyCount = mesh.triangles.Length / 3;
                    StringBuilder subMeshInfo = new StringBuilder();

                    if (subMeshCount > 1)
                    {
                        vertexCount = 0;
                        polyCount = 0;

                        for (int k = 0; k < subMeshList.Count; k++)
                        {
                            int subpolyCount = subMeshList[k].triangles.Length / 3;
                            float subvertexPerTriangles = ((float)subMeshList[k].vertexCount / (float)subpolyCount) * 100f;

                            subMeshInfo.AppendLine($",,{subMeshList[k].name},{subMeshList[k].vertexCount},{subpolyCount},{(int)subvertexPerTriangles}%");

                            vertexCount += subMeshList[k].vertexCount;
                            polyCount += subpolyCount;
                        }
                    }
                    float vertexPerTriangles = ((float)vertexCount / (float)polyCount) * 100f;

                    string boneCount = skinnedMeshRenderer == null ? "Static Mesh" : skinnedMeshRenderer.bones.Length.ToString();

                    temp.AppendLine($",{targetPath},{subMeshCount},{vertexCount},{polyCount},{(int)vertexPerTriangles}%,{modelImporter.meshCompression},{modelImporter.isReadable},{modelImporter.optimizeMeshVertices},{modelImporter.animationType},{boneCount},{modelImporter.importNormals},{modelImporter.importTangents}");

                    if (subMeshCount > 1)
                    {
                        temp.Append(subMeshInfo);
                    }

                    subMeshList.Clear();
                    totalMesh++;
                }

                // 프로그레스바 그리기. 고훈(18-08-24).                
                if (Common.ShowProgressBar("Mesh(FBX) List 저장", Path.GetFileName(targetPath), 1, num, i))
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
            }

            // 프로그레스바 지우기. 고훈(18-08-24).
            EditorUtility.ClearProgressBar();

            // 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            tempText.AppendLine(string.Format(",Total Mesh: {0}", totalMesh));

            tempText.AppendLine(temp.ToString());
            WriteCsvFile("TAT_ModelList", tempText.ToString());

        }


        private void GetAnimationClipList()
        {

            if (!EditorUtility.DisplayDialog("Confirm", "AnimationClip 리스트를 저장할까요?", "예", "아니요")) return;

            string[] tempGuidArray = AssetDatabase.FindAssets("t:AnimationClip");

            StringBuilder temp = new StringBuilder();

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            temp.Append("\n");
            temp.AppendLine(this._descriptionAnimclipList);
            temp.Append("\n");

            // 먼저 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            // Single Key 항목을 Triangle Pelvis 뒤에 추가.고훈(180824).
            temp.Append(", AnimationClip,FrameRate,Length,KeyState, Triangle Pelvis, Single Key, Animation Type, Anim. Compression, Rotation Error, Position Error, Scale Error\n");

            int iNum = tempGuidArray.Length;

            // 먼저 한셀을 띄고 시작한다.고훈(180827). 레이아웃을 위해'ㅂ').
            temp.Append(",");

            temp.Append(string.Format("Total AnimationClip: {0}\n", iNum));
            for (int i = 0; i < iNum; i++)
            {
                string keyState = string.Empty;
                string targetPath = AssetDatabase.GUIDToAssetPath(tempGuidArray[i]);

                if (IsSkip(targetPath)) continue;   

                // Debug.Log(Path.GetExtension(targetPath));

                AnimationClip targetAnimationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(targetPath, typeof(AnimationClip));
                EditorCurveBinding[] targetCurveBinding = AnimationUtility.GetCurveBindings(targetAnimationClip);

                string animationType = ".anim";
                string animationCompression = string.Empty;
                string animatinoRotationError = string.Empty;
                string animatinoPositionError = string.Empty;
                string animatinoScaleError = string.Empty;

                if (Path.GetExtension(targetPath.ToLower()) == ".fbx")
                {
                    ModelImporter targetModelImporter = (ModelImporter)AssetImporter.GetAtPath(targetPath);
                    animationType = targetModelImporter.animationType.ToString();
                    animationCompression = targetModelImporter.animationCompression.ToString();

                    if (animationCompression != "Off")
                    {
                        animatinoRotationError = targetModelImporter.animationRotationError.ToString();
                        animatinoPositionError = targetModelImporter.animationPositionError.ToString();
                        animatinoScaleError = targetModelImporter.animationScaleError.ToString();
                    }
                }

                string isTrianglePelvis = string.Empty;

                //targetAnimationClip.humanMotion; <<< 휴머노이드인지 확인

                // 첫키만 있는 커브 카운트.고훈(180824)
                int singleKeyCount = 0;

                int jNum = targetCurveBinding.Length;
                for (int j = 0; j < jNum; j++)
                {

                    // Single Key 검사. 고훈(180824).
                    // 첫키 하나만 있는 커브를 찾는다. 나중에 문제가 생길 수 있다.
                    AnimationCurve animationCurve = AnimationUtility.GetEditorCurve(targetAnimationClip, targetCurveBinding[j]);
                    if (animationCurve.keys.Length == 1)
                    {
                        singleKeyCount++;
                    }

                    // Thigh >> UpperLeg

                    if (targetCurveBinding[j].path.Contains("Thigh"))
                    {
                        string[] nodeList = targetCurveBinding[j].path.Split('/');
                        if (nodeList[nodeList.Length - 1].Contains("Thigh"))
                        {
                            if (nodeList[nodeList.Length - 2].Contains("Spine"))
                            {
                                isTrianglePelvis = "Triangle Pelvis ON";
                            }
                            else { isTrianglePelvis = "Triangle Pelvis OFF"; }
                        }
                    }

                    AnimationCurve targetAnimationCurve = AnimationUtility.GetEditorCurve(targetAnimationClip, targetCurveBinding[j]);

                    Keyframe[] targetKeyFrame = targetAnimationCurve.keys;  // 커브의 각Key 프레임정보

                    float totalKeyCount = Mathf.RoundToInt(targetAnimationClip.frameRate * targetAnimationClip.length);

                    if (totalKeyCount == targetKeyFrame.Length)
                    {
                        keyState = targetKeyFrame.Length.ToString();
                    }
                }

                // 먼저 한셀을 띄고 시작한다.고훈(180827). 레이아웃을 위해'ㅂ').

                temp.Append($",{targetPath},{targetAnimationClip.frameRate},{targetAnimationClip.length},{keyState},{isTrianglePelvis},{singleKeyCount},{animationType},{animationCompression},{animatinoRotationError},{animatinoPositionError},{animatinoScaleError}");
                temp.Append("\n");

                // 프로그레스바 그리기. 고훈(18-08-24).                
                if (Common.ShowProgressBar("Animation Clip List 저장", Path.GetFileName(targetPath), 1, iNum, i))
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
            }

            // 프로그레스바 지우기. 고훈(18-08-24).
            EditorUtility.ClearProgressBar();

            WriteCsvFile("TAT_AnimationClipList", temp.ToString());

        }


        private void GetTextureList()
        {
            if (!EditorUtility.DisplayDialog("Confirm", "Texture 리스트를 저장할까요?", "예", "아니요")) return;

            string[] tempGuidArray = AssetDatabase.FindAssets("t:Texture2D");
            StringBuilder temp = new StringBuilder();

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            temp.Append("\n");
            temp.AppendLine(this._descriptionTextureList);
            temp.Append("\n");

            // 먼저 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            temp.AppendLine(",Path,Texture Name,Override for Android,Width,Height,MaxSize,NPOT Scale,Format,Compression Quality,etc1AlphaSplit,Read/Write Enabled,Mipmap,Sprite Packing Tag,Generate Physics Shape");

            int num = tempGuidArray.Length;

            // 먼저 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            temp.AppendLine(string.Format(",Total Texture :{0} (Hive_SDK 포함)", num));

            for (int i = 0; i < num; i++)
            {
                bool isOverrideAndroid;
                bool isOverrideIOS;
                string targetPath = AssetDatabase.GUIDToAssetPath(tempGuidArray[i]);

                if (targetPath.Contains("Hive_SDK")) continue;
                //if (targetPath.Contains("__Work")) continue;
                //if (targetPath.Contains("Atlas")) continue; //Atlas로 만들 이미지는 첵크하지 않는다.

                if (IsSkip(targetPath)) continue;   

                int maxSize;
                TextureImporterFormat androidImporterFormat;
                TextureImporterFormat iosImporterFormat;
                int compressionQuality;
                TextureImporterNPOTScale npotScale;

                try
                {
                    TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(targetPath);

                    bool isSplitAlpha;
                    string splitAlpha = string.Empty;

                    isOverrideAndroid = textureImporter.GetPlatformTextureSettings("Android", out maxSize, out androidImporterFormat, out compressionQuality, out isSplitAlpha);
                    isOverrideIOS = textureImporter.GetPlatformTextureSettings("IPhonePlayer", out maxSize, out iosImporterFormat, out compressionQuality, out isSplitAlpha);

                    if (isOverrideAndroid)
                    {
                        splitAlpha = isSplitAlpha.ToString();
                    }

                    Texture texture = AssetDatabase.LoadAssetAtPath(targetPath, typeof(Texture)) as Texture;
                    npotScale = textureImporter.npotScale;
                    //if (targetTextureImporter.npotScale == 0) {isNPOT = "NPOT"; }

                    // 프로그레스바 그리기. 고훈(18-08-24).                
                    if (Common.ShowProgressBar("Texture List 저장", Path.GetFileName(targetPath), 1, num, i))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }

                    temp.AppendFormat($",{Path.GetDirectoryName(targetPath)},{Path.GetFileName(targetPath)},{isOverrideAndroid},{texture.width},{texture.height},{maxSize},{npotScale},{androidImporterFormat},{compressionQuality},{splitAlpha},{textureImporter.isReadable},{textureImporter.mipmapEnabled},");

                    if (TextureImporterType.Sprite == textureImporter.textureType)
                    {
                        string packingTag = textureImporter.spritePackingTag;

                        if (packingTag.Length <= 0) { packingTag = "none"; }

                        temp.AppendFormat($"{packingTag},");

                        TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
                        textureImporter.ReadTextureSettings(textureImporterSettings);

                        //temp.Append(textureImporterSettings.spriteGenerateFallbackPhysicsShape);
                        temp.Append("-");   //5.x대라서 없다??/
                    }

                    temp.Append(",\n");
                }
                catch { Debug.Log(targetPath); }
            }

            // 프로그레스바 지우기. 고훈(18-08-24).
            EditorUtility.ClearProgressBar();

            WriteCsvFile("TAT_TextureList", temp.ToString());
        }


        private void GetVFXPropetiesList()
        {
            if (!EditorUtility.DisplayDialog("Confirm", "VFX Prefab 리스트를 저장할까요?", "예", "아니요")) return;

            int vfxCount = 0;
            int totalPSCount = 0;

            string[] tempGuidArray = AssetDatabase.FindAssets("t:Prefab");

            StringBuilder tempText = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            //tempText.AppendLine("프로젝트내의 Prefab 중 하위 Node에 Particle System이 있을 경우 VFX파일이라고 가정합니다.\n");

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            tempText.Append("\n");
            tempText.AppendLine(this._descriptionVFXList);
            tempText.Append("\n");

            // 먼저 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            tempText.AppendLine(",Path, Prefab Name, Particle System Count\n\t,, Node Name,Render Mode, Sort Mode,Looping, Max Particle, Start Life Time Min, Start Life Time Max, Shadow Casting Mode, Receive Shadow");

            int num = tempGuidArray.Length;

            for (int i = 0; i < num; i++)
            {
                string targetPath = AssetDatabase.GUIDToAssetPath(tempGuidArray[i]);
                if (IsSkip(targetPath)) continue;

                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(targetPath, typeof(GameObject));

                ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
                //ParticleSystemRenderer[] targetPSRList = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);

                // 프로그레스바 그리기. 고훈(18-08-24).                
                if (Common.ShowProgressBar("VFX List 저장", Path.GetFileName(targetPath), 1, num, i))
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }

                int jnum = particleSystems.Length; // 파티클 관련 컴포넌트가 있으면 이펙트임
                if (0 < jnum)
                {
                    temp.AppendLine($",{Path.GetDirectoryName(targetPath)},{prefab.name},{jnum}");

                    for (int j = 0; j < jnum; j++)
                    {
                        ParticleSystemRenderer particleSystemRenderer = particleSystems[j].gameObject.GetComponent<ParticleSystemRenderer>();
                        string renderMode = "No Render";
                        string sortMode = string.Empty;
                        string shadowCastingMode = string.Empty;
                        string receiveShadows = string.Empty;

                        if (null != particleSystemRenderer)
                        {
                            renderMode = particleSystemRenderer.renderMode.ToString();
                            sortMode = particleSystemRenderer.sortMode.ToString();
                            shadowCastingMode = particleSystemRenderer.shadowCastingMode.ToString();
                            receiveShadows = particleSystemRenderer.receiveShadows.ToString();
                        }

                        temp.AppendLine($",,{particleSystems[j].name},{renderMode},{sortMode},{particleSystems[j].main.loop},{particleSystems[j].main.maxParticles},{particleSystems[j].main.startLifetime.constantMin},{particleSystems[j].main.startLifetime.constantMax},{shadowCastingMode},{receiveShadows}");

                        totalPSCount++;
                    }
                    vfxCount++;
                }
            }

            // 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            tempText.AppendLine(string.Format(",Total VFX Prefab: {0}", vfxCount));

            // 한셀을 띄고 시작하기 위해 ','를 앞에 붙임.고훈(180827). 레이아웃을 위해'ㅂ').
            tempText.AppendLine(string.Format(",Total Particle System Count: {0}", totalPSCount));
            tempText.AppendLine(temp.ToString());

            // 프로그레스바 지우기. 고훈(18-08-24).
            EditorUtility.ClearProgressBar();

            WriteCsvFile("TAT_VFXList", tempText.ToString());

        }


        private void GetAudioList()
        {
            if (!EditorUtility.DisplayDialog("Confirm", "Audion File 리스트를 저장할까요?", "예", "아니요")) return;

            string[] tempGuidArray = AssetDatabase.FindAssets("t:AudioClip");
            StringBuilder temp = new StringBuilder();

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            temp.Append("\n");
            temp.AppendLine(this._descriptionAudioClipList);
            temp.Append("\n");

            // 먼저 한셀을 띄고 시작한다.고훈(180827). 레이아웃을 위해'ㅂ').
            temp.Append(",");

            temp.AppendLine("Audio Clip Name,Src Type,Compression Format,LoadType,channels,Force To Mono,Quality,Length(s),Samples,Frequency(Hz)");

            int num = tempGuidArray.Length;

            // 먼저 한셀을 띄고 시작한다.고훈(180827). 레이아웃을 위해'ㅂ').
            temp.Append(",");
            temp.AppendLine(string.Format("Total Audio Clip: {0}", num));

            for (int i = 0; i < num; i++)
            {
                string targetPath = AssetDatabase.GUIDToAssetPath(tempGuidArray[i]);

                if (IsSkip(targetPath)) continue;

                AudioClip targetAudioClip = (AudioClip)AssetDatabase.LoadAssetAtPath(targetPath, typeof(AudioClip));
                AudioImporter targetAudioImpoter = (AudioImporter)AssetImporter.GetAtPath(targetPath);

                try
                {
                    temp.AppendFormat($",{targetPath},{Path.GetExtension(targetPath)},{targetAudioImpoter.defaultSampleSettings.compressionFormat},{targetAudioImpoter.defaultSampleSettings.loadType},{targetAudioClip.channels}");
                    temp.AppendFormat($",{targetAudioImpoter.forceToMono},{targetAudioImpoter.defaultSampleSettings.quality},{targetAudioClip.length},{targetAudioClip.samples},{targetAudioClip.frequency}");
                    temp.Append(",\n");
                }
                catch { Debug.Log(targetPath); }

                // 프로그레스바 그리기. 고훈(18-08-24).                
                if (Common.ShowProgressBar("Audio List 저장", Path.GetFileName(targetPath), 1, num, i))
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }
                //Debug.Log(targetAudionClip.name + " : " + targetAudionClip.channels.ToString());
            }

            EditorUtility.ClearProgressBar();       // 프로그레스바 지우기. 고훈(18-08-24).

            WriteCsvFile("TAT_AudioClipList", temp.ToString());
        }


        /// <summary>
        /// 사용 이미지 대비 사용 면적을 비교하여 비율을 리턴
        /// </summary>
        /// <param name="srcRect">사용한 이미지</param>
        /// <param name="useRect">사용중인 영역</param>
        (float, bool) GetSizeRate(Rect srcRect, Rect useRect)
        {
            float srcSize = Mathf.Abs(srcRect.width * srcRect.height);
            float recSize = Mathf.Abs(useRect.width * useRect.height);

            bool isSrcBig = srcSize > recSize;
            float sizeRate = (srcSize / recSize) * 100f;

            return (sizeRate, isSrcBig);
        }

        /// <summary>
        /// 스프라이트 리스트 저장
        /// </summary>
        /// <param name="isUsed">사용여부 true 사용하는 스프라이트 목록,false 사용하지 않는 스프라이트 목록 </param>
        void GetSpriteList(bool isUsed)
        {
            if (!EditorUtility.DisplayDialog("확인", "사용하지 않는 것으로 생각되는 Sprite 목록을 만듦니다.", "저장", "취소")) return;

            List<Sprite> spriteListinProject = GetSpriteListInProject();            //프로젝트에서 사용중인 Sprite 목록
            List<Sprite> spriteListinGUIPrefeb = GetSpriteListInGUIPrefab();        //UI Prefab 에서 사용중인 Sprite 목록
            List<Sprite> spriteListUnUsed = new List<Sprite>();                     //사용하지 않는다고 생각되는 Sprite 목록 테이블에서 접근하는 놈들은 어떻게 하지..-_-a

            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder tmp = new StringBuilder();

            int count = 0;
            
            foreach (Sprite sprite in spriteListinProject)
            {
                if (spriteListinGUIPrefeb.Contains(sprite) == isUsed)
                {
                    spriteListUnUsed.Add(sprite);
                    //Debug.Log($"Unused Sprite: {AssetDatabase.GetAssetPath(sprite)} >> {sprite.name}");
                    string path = AssetDatabase.GetAssetPath(sprite);
                    tmp.AppendLine($"{Path.GetDirectoryName(path)}, {Path.GetFileName(path)}, {sprite.name}");

                    count++;
                }
            }

            string infoString = isUsed ? "사용중인 스프라이트" : "사용하지 않는 스프라이트";
            stringBuilder.AppendLine($"{infoString}");
            stringBuilder.AppendLine("Sprite Path, File Name ,Sprite Name");
            stringBuilder.AppendLine($"count: {count}");
            stringBuilder.AppendLine(tmp.ToString());

            string fileName = isUsed ? "TAT_UsedSpriteList" : "TAT_UnusedSpriteList";
            WriteCsvFile(fileName, stringBuilder.ToString());
        }


        private List<Sprite> GetSpriteListInGUIPrefab()
        {
            string[] prefabGuidArray = AssetDatabase.FindAssets("t:Prefab");
            
            List<Sprite> spriteListinGUIPrefeb = new List<Sprite>();                       

            int totalPrefabCount = 0;
            int num = prefabGuidArray.Length;     //전체 프리펩개수
            for (int i = 0; i < num; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuidArray[i]);
                
                if (prefabPath.Contains("External")) continue;
                if (prefabPath.Contains("Packages")) continue;

                GameObject targetPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                MaskableGraphic[] maskableGraphics = targetPrefab.GetComponentsInChildren<MaskableGraphic>(true);
                if (maskableGraphics.Length <= 0) continue;   //maskableGraphics 객체가 있을 경우 UGUI 객체라 판단.
                
                foreach (MaskableGraphic maskableGraphic in maskableGraphics)
                {
                    if (maskableGraphic is Image)
                    {
                        Image tmpImage = maskableGraphic.GetComponent<Image>();

                        if (null != tmpImage.sprite && !spriteListinGUIPrefeb.Contains(tmpImage.sprite))
                        {
                            spriteListinGUIPrefeb.Add(tmpImage.sprite);
                        }
                        continue;
                    }
                }
                totalPrefabCount++;
            }

            EditorUtility.ClearProgressBar();   // 프로그레스바 지우기. 고훈(18-08-24).
            return spriteListinGUIPrefeb;
        }


        private List<Sprite> GetSpriteListInProject()
        {
            string[] spriteGuidArray = AssetDatabase.FindAssets("t:Sprite");
            
            List<Sprite> spriteList = new List<Sprite>();

            int num = spriteGuidArray.Length;     //전체 프리펩개수
            for (int i = 0; i < num; i++)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(spriteGuidArray[i]);

                if (spritePath.Contains("External")) continue;
                if (spritePath.Contains("Packages")) continue;
                if (spritePath.Contains("PartsItem")) continue;

                Sprite targetSprite = (Sprite)AssetDatabase.LoadAssetAtPath(spritePath, typeof(Sprite));

                spriteList.Add(targetSprite);
            }

            return spriteList;
        }

    }
}