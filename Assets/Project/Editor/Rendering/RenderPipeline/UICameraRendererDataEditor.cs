/*===============================================================
* Product:		Com2Verse
* File Name:	UICameraRendererDataEditor.cs
* Developer:	ljk
* Date:			2022-08-18 18:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System.Linq;
using Com2Verse.Rendering.RenderPipeline;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Com2VerseEditor.Rendering.RenderPipeline
{
    [CustomEditor(typeof(UICameraRendererData))]
    public class UICameraRendererDataEditor : Editor
    {
    
        public override void OnInspectorGUI()
        {
            UICameraRendererData cameraRendererData = (target as UICameraRendererData);
            GUILayout.Label("[ UI 카메라 전용 렌더스케일 고정된 렌더러 입니다 ]");
            GUILayout.Label("UI 는 모두 Transparent 이므로 Opaque 렌더를 안합니다");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Transparent Layer Mask");
            string[] layers = Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            cameraRendererData.TransparentLayerMask = EditorGUILayout.MaskField(cameraRendererData.TransparentLayerMask,layers);
            GUILayout.EndHorizontal();
        }
	
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        private class CreateUIRendererAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var rendererData = CreateInstance<UICameraRendererData>();
                ScriptableRendererData data = rendererData;
             
                AssetDatabase.CreateAsset(data, pathName);
                
                Selection.activeObject = data;
            }
        }

        [MenuItem("Assets/Create/Rendering/(MV)URP UI Renderer", priority = CoreUtils.Sections.section3 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 2)]
        public static void CreateUniversalRendererData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateUIRendererAsset>(), "New Custom Universal Renderer Data.asset", null, null);
        }
    }

}
