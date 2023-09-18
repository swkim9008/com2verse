/*===============================================================
* Product:		Com2Verse
* File Name:	C2VBuilderThumbnailInjector.cs
* Developer:	yangsehoon
* Date:			2023-05-25 14:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Builder;
using Com2Verse.Data;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2VerseEditor.Builder
{
	[CustomEditor(typeof(C2VBuilderThumbnailInjector))]
	public sealed class C2VBuilderThumbnailInjectorEditor : Editor
	{
	    private bool _isProcessing = false;
	    private int _totalObjectCount;
	    private int _currentObjectCount;
	    private static Dictionary<long, BaseObject> _objectData;
	    private WaitForEndOfFrame _delay = new WaitForEndOfFrame();

	    private IEnumerator LoadTableData()
	    {
		    var objectDataHandle = Addressables.LoadAssetAsync<TextAsset>("BaseObject.bytes");
		    while (objectDataHandle.PercentComplete < 1)
		    {
			    EditorApplication.QueuePlayerLoopUpdate();
			    yield return null;
		    }

		    if (objectDataHandle.Status == AsyncOperationStatus.Succeeded)
		    {
			    var objectData = objectDataHandle.Result;
			    var objectTableTaskHandle = TableBaseObject.LoadAsync(objectData?.bytes);
			    
			    yield return objectTableTaskHandle;

			    _objectData = objectTableTaskHandle.Result.Datas;
		    }
		    else
		    {
			    UnityEngine.Debug.LogError("BaseObject 기획데이터를 로딩하는데 실패했습니다!!!");
			    _objectData = new Dictionary<long, BaseObject>();
		    }
	    }

	    private void ClearThumbnailData(ThumbnailDatabase thumbnailDatabase)
	    {
		    thumbnailDatabase.TargetObjects.Clear();
		    
		    thumbnailDatabase.Sprites.Clear();
		    thumbnailDatabase.ThumbnailMap.Clear();

		    AssetDatabase.DeleteAsset(thumbnailDatabase.ThumbnailPath);
	    }

	    private IEnumerator Inject(ThumbnailDatabase thumbnailDatabase)
	    {
		    yield return LoadTableData();
		    ClearThumbnailData(thumbnailDatabase);

		    _totalObjectCount = 0;
		    var data = new string[_objectData.Count];
		    int i = 0;
		    foreach (var objectData in _objectData.Values)
		    {
			    if (objectData.ObjectType == eObjectType.OBJECT)
			    {
				    data[i] = objectData.Name;
				    _totalObjectCount++;
			    }

			    i++;
		    }
		    var settings = AddressableAssetSettingsDefaultObject.Settings;

		    foreach (var group in settings.groups)
		    {
			    foreach (var entry in group.entries)
			    {
				    if (data.Contains(entry.address))
				    {
					    thumbnailDatabase.TargetObjects.Add((GameObject)entry.TargetAsset);
					    UnityEngine.Debug.Log($"{entry.address} 추가됨");
					    _currentObjectCount++;
				    }
			    }

			    yield return _delay;
		    }
		    
		    _isProcessing = false;
	    }

	    public override void OnInspectorGUI()
	    {
		    serializedObject.Update();
		    
		    base.OnInspectorGUI();
		    
		    if (_isProcessing)
		    {
			    EditorGUILayout.HelpBox("작업중...", MessageType.Info);
			    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), (float)_currentObjectCount / System.Math.Max(1, _totalObjectCount), "Progress");
		    }
		    else
		    {
			    if (GUILayout.Button("Inject Table Data"))
			    {
				    _isProcessing = true;
				    EditorCoroutineUtility.StartCoroutineOwnerless(Inject((target as C2VBuilderThumbnailInjector).BuilderThumbnailDatabase));
			    }
		    }

		    serializedObject.ApplyModifiedProperties();
	    }
	}
}