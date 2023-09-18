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
using Com2verseEditor.UnityAssetTool;

namespace Com2VerseEditor.UnityAssetTool
{
    public partial class AssetCheckTool : EditorWindow
    {
        private readonly string _description_LocalizationUI = "항목,설명\n"
                                                       + "Type, UGUI 기존 Text 와 Text Mesh Pro 의 Text를 구분합니다.\n"
                                                       + "Has Component,게임 오브젝트에 LocalizationUI 컴포넌트 적용 여부입니다.\n"
                                                       + "Localization Text Key, LocalizationUI 컴포넌트에 적용되어 있는 TextKey 입니다.\n"
                                                       + "TextString, 작업 시 임시로 입력해 놓은 내용입니다. 실재 로컬 내용과 다를 수 있습니다.\n";

        private void GetLocalizationUIListInGUIPrefab()
        {
            if (!EditorUtility.DisplayDialog("확인", "UGUI 프리펩의 텍스트 리스트 저장합니다. 오래결러요.", "저장", "취소")) return;

            string[] prefabGuidArray = AssetDatabase.FindAssets("t:Prefab");

            StringBuilder stringBuilderFinal = new StringBuilder();   //전체 텍스트
            StringBuilder stringBuilder = new StringBuilder();       //프리펩 정보 텍스트



            int totalPrefabCount = 0;

            int num = prefabGuidArray.Length;     //전체 프리펩개수
            for (int i = 0; i < num; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuidArray[i]);

                //특정 폴더는 빼고 첵크
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

                    //stringBuilder.AppendLine($",{Path.GetDirectoryName(prefabPath)},{Path.GetFileName(prefabPath)},{jnum}");

                    //컴포넌트 별로 구분
                    foreach (MaskableGraphic maskableGraphic in maskableGraphics)
                    {
                        string gameObjectName = maskableGraphic.name;
                        string textString = string.Empty;
                        string textKey = string.Empty;
                        bool hasComponent = false;

                        if (maskableGraphic is Text)
                        {
                            Text tmpText = maskableGraphic.GetComponent<Text>();
                            textString = tmpText.text;
                            Com2Verse.UI.LocalizationUI localizationUI = maskableGraphic.GetComponent<Com2Verse.UI.LocalizationUI>();   //로컬용 컴포넌트
                            textKey = localizationUI != null ? localizationUI.TextKey : "-";
                            hasComponent = localizationUI != null;
                            stringBuilder.AppendLine($",{Path.GetDirectoryName(prefabPath)},{Path.GetFileName(prefabPath)},{gameObjectName},Text,{hasComponent},{textKey},\"{textString}\"");
                        }
                        else if (maskableGraphic is TextMeshProUGUI)
                        {
                            TextMeshProUGUI tmpText = maskableGraphic.GetComponent<TextMeshProUGUI>();
                            textString = tmpText.text;

                            Com2Verse.UI.LocalizationUI localizationUI = maskableGraphic.GetComponent<Com2Verse.UI.LocalizationUI>();   //로컬용 컴포넌트
                            hasComponent = localizationUI != null;
                            textKey = localizationUI != null ? localizationUI.TextKey : "-";
                            stringBuilder.AppendLine($",{Path.GetDirectoryName(prefabPath)},{Path.GetFileName(prefabPath)},{gameObjectName},TMPText,{hasComponent},{textKey},\"{textString}\"");
                        }
                    }
                }
            }
            
            EditorUtility.ClearProgressBar();   // 프로그레스바 지우기. 고훈(18-08-24).
                                                // 항목 설명을 먼저 붙인다.(고훈.180827).
            stringBuilderFinal.Append("\n");
            stringBuilderFinal.AppendLine(this._description_LocalizationUI);
            
            stringBuilderFinal.AppendLine(",Path,PrefabName,GameObject Name,Type,Has Component,Localization Text Key,Sample Text");
            stringBuilderFinal.AppendLine($",Total UGUIPrefab: {totalPrefabCount}");
            stringBuilderFinal.Append(stringBuilder);

            WriteCsvFile("TAT_LocalizationUIList", stringBuilderFinal.ToString());
        }



        private void GetUGUIPrefabSpriteList()
        {
            if (!EditorUtility.DisplayDialog("확인", "UGUI 프리펩 리스트 저장합니다. 오래결러요.", "저장", "취소")) return;

            string[] prefabGuidArray = AssetDatabase.FindAssets("t:Prefab");

            StringBuilder tempText = new StringBuilder();   //전체 텍스트
            StringBuilder temp = new StringBuilder();       //프리펩 정보 텍스트

            // 항목 설명을 먼저 붙인다.(고훈.180827).
            //tempText.Append("\n");
            //tempText.AppendLine(this._descriptionUIPrefabList);
            //tempText.Append("\n");
            //tempText.AppendLine(",Path,PrefabName,NodeName,Type,srcName,material,shader,width,height,src.width,src.height,srcSizeIsBig,OverRate(%)");
            tempText.AppendLine(",Path,PrefabName,NodeName,Type,srcName");

            int totalPrefabCount = 0;

            int num = prefabGuidArray.Length;     //전체 프리펩개수
            for (int i = 0; i < num; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuidArray[i]);
                if (IsSkip(prefabPath)) continue;                   //특정 폴더는 빼고 첵크
                                                                    //프로그레스바 그리기. 고훈(18 - 08 - 24).
                if (Common.ShowProgressBar("GUI List 저장", Path.GetFileName(prefabPath), 1, num, i))
                {
                    EditorUtility.ClearProgressBar();
                    break;
                }

                GameObject targetPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                MaskableGraphic[] maskableGraphics = targetPrefab.GetComponentsInChildren<MaskableGraphic>(true);

                int jnum = maskableGraphics.Length;
                if (0 < jnum)   //UIRect 객체가 있을 경우 UGUI 객체라 판단.
                {
                    totalPrefabCount++;

                    //컴포넌트 별로 구분
                    foreach (MaskableGraphic maskableGraphic in maskableGraphics)
                    {
                        string srcName = "miss";

                        (float, bool) sizeCheckValue = (0,false);
                        Rect srcRect = new Rect();
                        Rect useRect = maskableGraphic.rectTransform.rect;
                        string materialName = string.Empty;
                        string shaderName = string.Empty;

                        //if (maskableGraphic.material != null)
                        //{
                        //    materialName = maskableGraphic.material.name == "Default UI Material" ? string.Empty : maskableGraphic.material.name;
                        //    shaderName = maskableGraphic.material.shader.name == "UI/Default" ? string.Empty : maskableGraphic.material.shader.name;
                        //}

                        if (maskableGraphic is Image)
                        {
                            Image tmpImage = maskableGraphic.GetComponent<Image>();

                            if (null != tmpImage.sprite)
                            {
                                srcRect = tmpImage.sprite.rect;
                                sizeCheckValue = GetSizeRate(srcRect, useRect);
                                srcName = tmpImage.sprite.name;
                            }
                        }
                        else if (maskableGraphic is RawImage)
                        {
                            RawImage tmpTexture = maskableGraphic.GetComponent<RawImage>();

                            if (null != tmpTexture.texture)
                            {
                                srcRect = new Rect(0, 0, tmpTexture.mainTexture.width, tmpTexture.mainTexture.height);
                                sizeCheckValue = GetSizeRate(srcRect, useRect);
                                srcName = tmpTexture.mainTexture.name;
                            }
                        }
                        else if (maskableGraphic is Text)
                        {
                            Text tmpText = maskableGraphic.GetComponent<Text>();
                            srcName = null == tmpText.font ? "miss?" : tmpText.font.name;
                        }
                        else if (maskableGraphic is TextMeshProUGUI)
                        {
                            TextMeshProUGUI tmpText = maskableGraphic.GetComponent<TextMeshProUGUI>();
                            srcName = null == tmpText.font ? "miss?" : tmpText.font.name;
                        }

                        //temp.AppendLine($",{Path.GetDirectoryName(prefabPath)},{Path.GetFileName(prefabPath)},{maskableGraphic.name},{maskableGraphic.GetType()},{srcName},{materialName},{shaderName},{useRect.width},{useRect.height},{srcRect.width},{srcRect.height},{sizeCheckValue.Item2},{sizeCheckValue.Item1.ToString("N2")}");
                        temp.AppendLine($",{Path.GetDirectoryName(prefabPath)},{Path.GetFileNameWithoutExtension(prefabPath)},{maskableGraphic.name},{maskableGraphic.GetType().Name},{srcName}");
                        //ParticleSystem tmpParticleSystem = maskableGraphic.GetComponent<ParticleSystem>();
                        //if (null != tmpParticleSystem)
                        //{
                        //    temp.Append("PARTICLE");
                        //    temp.Append("\n");
                        //    continue;
                        //}

                        //temp.Append("\n");
                    }
                }
            }

            EditorUtility.ClearProgressBar();   // 프로그레스바 지우기. 고훈(18-08-24).

            tempText.AppendLine($",Total UGUIPrefab: {totalPrefabCount}");
            tempText.Append(temp);

            WriteCsvFile("TAT_UGUIPrefabSpriteList", tempText.ToString());
        }
    }




    

}