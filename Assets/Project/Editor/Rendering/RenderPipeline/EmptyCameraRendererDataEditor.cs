/*===============================================================
* Product:		Com2Verse
* File Name:	EmptyRendererDataEditor.cs
* Developer:	ljk
* Date:			2022-10-07 18:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Com2Verse.Rendering.RenderPipeline;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Com2VerseEditor.Rendering.RenderPipeline
{
    [CustomEditor(typeof(EmptyRendererData))]
    public class EmptyRendererDataEditor : Editor
    {
    
        public override void OnInspectorGUI()
        {
            GUILayout.Label("[ 미니모드용 검은화면 렌더러 입니다 ]");
       //     base.OnInspectorGUI();
        }
	
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        private class CreateEmptyRendererAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var rendererData = CreateInstance<EmptyRendererData>();
                ScriptableRendererData data = rendererData;
             
                AssetDatabase.CreateAsset(data, pathName);
                
                Selection.activeObject = data;
            }
        }

        [MenuItem("Assets/Create/Rendering/(MV)URP Empty Renderer", priority = CoreUtils.Sections.section3 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 2)]
        public static void CreateUniversalRendererData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateEmptyRendererAsset>(), "New Custom Universal Renderer Data.asset", null, null);
        }
    }

}
