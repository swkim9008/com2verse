/*===============================================================
* Product:		Com2Verse
* File Name:	CreateJiraSubTask.cs
* Developer:	haminjeong
* Date:			2023-08-23 14:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Com2VerseEditor
{
	[CustomEditor(typeof(JiraSubTaskInfo))]
	public sealed class CreateJiraSubTask : Editor
	{
		private static readonly string RequestURL = "https://jira.com2us.com/jira/rest/api/latest/issue";
		private static readonly string JiraIssueURLFormat = "https://jira.com2us.com/jira/browse/{0}";

		[Serializable]
		internal class RequestFormat
		{
			public RequestField fields;
		}

		[Serializable]
		internal class RequestField
		{
			public RequestKey       project;
			public RequestKey       parent;
			public string           summary;
			public string           description;
			public RequestIssueType issuetype;
		}

		[Serializable]
		internal class RequestKey
		{
			public string key;
		}

		[Serializable]
		internal class RequestIssueType
		{
			public string id;
		}
		
		[Serializable]
		internal class ResultResponse
		{
			public string id;
			public string key;
			public string self;
		}

		private string[] _summaryArray =
		{
			"[인프라스트럭처] 오브젝트 데이터 최신화 요청",
			"[인프라스트럭처] 오브젝트 InteractionValue 최신화 요청",
			"[인프라스트럭처] 존 데이터 최신화 요청",
			"[인프라스트럭처] 빌딩 데이터 최신화 요청",
			"직접 입력",
		};
		private int    _summaryIndex = 0;

		private string[] _descTypeArray =
		{
			"오브젝트 데이터",
			"오브젝트 InteractionValue",
			"존 데이터",
			"빌딩 데이터",
			"직접 입력",
		};
		private int    _descTypeIndex = 0;
		private string _descTypeString;
		
		private string _folderString = string.Empty;
		private string _branchString = string.Empty;
		private string[] _descFileNameArray =
		{
			"Building.csv",
			"BuildingDetail.csv",
			"BuildingInteraction.csv",
			"World_Object.csv",
			"ObjectInteractionValue.csv",
			"World_Zone.csv",
			"World_ZoneInteraction.csv",
			"BuildingDetail.csv",
			"Space.csv",
			"SpaceDetail.csv",
		};
		private int _descFileIndex = 0;
		private string[] _descConfigArray =
		{
			"Live",
			"QA",
			"Dev-Int",
			"Test-RKE",
			"직접 입력",
		};
		private int    _descConfigIndex = 0;
		private string _descConfigString;
		
		private static readonly string       _descStringFormat = "안녕하세요~ {0}가 변경되어서 요청드립니다.. (truncate -> insert)\n\n{1}\n{2}\n 위 파일들을 봐주시면 됩니다. 감사합니다~\nHost : {3}\n";
		private                 List<string> _descFileList     = new();

		[MenuItem("Com2Verse/Create Jira SubTask")]
		private static void SelectWizard()
		{
			var thisObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Project/Editor/PhysicsSerializer/JiraSubTaskInfo.asset");
			Selection.activeObject = thisObject;
			EditorGUIUtility.PingObject(thisObject);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var token       = serializedObject.FindProperty("Token");
			var summary     = serializedObject.FindProperty("Summary");
			var description = serializedObject.FindProperty("Description");

			token.stringValue = EditorGUILayout.TextField("Token", token.stringValue);
			EditorGUILayout.Space();
			_summaryIndex       = EditorGUILayout.Popup("Summary", System.Math.Min(_summaryIndex, _summaryArray.Length - 1), _summaryArray);
			summary.stringValue = _summaryIndex >= _summaryArray.Length - 1 ? EditorGUILayout.TextField("Custom", summary.stringValue) : _summaryArray[_summaryIndex];

			EditorGUILayout.Space();
			_descTypeIndex  = EditorGUILayout.Popup("Type", System.Math.Min(_descTypeIndex, _descTypeArray.Length - 1), _descTypeArray);
			_descTypeString = _descTypeIndex >= _descTypeArray.Length - 1 ? EditorGUILayout.TextField("Custom", _descTypeString) : _descTypeArray[_descTypeIndex];
			_folderString   = EditorGUILayout.TextField("Folder", _folderString);
			_branchString   = EditorGUILayout.TextField("Branch", _branchString);
			EditorGUILayout.BeginHorizontal();
			_descFileIndex = EditorGUILayout.Popup("File", System.Math.Min(_descFileIndex, _descFileNameArray.Length - 1), _descFileNameArray);
			if (GUILayout.Button("+"))
				_descFileList.TryAdd(_descFileNameArray[_descFileIndex]);
			if (GUILayout.Button("Clear"))
				_descFileList.Clear();
			StringBuilder sb = new();
			_descFileList.ForEach((s) => sb.AppendLine(s));
			EditorGUILayout.EndHorizontal();
			_descConfigIndex  = EditorGUILayout.Popup("Config", System.Math.Min(_descConfigIndex, _descConfigArray.Length - 1), _descConfigArray);
			_descConfigString = _descConfigIndex >= _descConfigArray.Length - 1 ? EditorGUILayout.TextField("Custom", _descConfigString) : _descConfigArray[_descConfigIndex];

			var updatePosition = string.IsNullOrEmpty(_folderString) ? $"브랜치 : {_branchString}" : $"폴더 : {_folderString}";
			description.stringValue = string.Format(_descStringFormat, _descTypeString, updatePosition, sb, _descConfigString);

			EditorGUILayout.LabelField("Description");
			EditorGUILayout.LabelField(description.stringValue, GUILayout.Height(108 + _descFileList.Count * 18));
			
			if (GUILayout.Button("Create Jira SubTask"))
			{
				CreateJiraSubTaskFromJson(token.stringValue, summary.stringValue, description.stringValue);
			}

			if (serializedObject.hasModifiedProperties)
				serializedObject.ApplyModifiedProperties();
		}

		private static void CreateJiraSubTaskFromJson(string token, string summary, string desc)
		{
			var request = new RequestFormat
			{
				fields = new RequestField
				{
					project = new RequestKey
					{
						key = "CUHC2V"
					},
					parent = new RequestKey
					{
						key = "CUHC2V-4361"
					},
					summary     = summary,
					description = desc,
					issuetype = new RequestIssueType
					{
						id = "11902"
					}
				},
			};
			var json = JsonUtility.ToJson(request);
			RequestToJira(token, json).Forget();
		}

		private static async UniTask RequestToJira(string token, string json)
		{
			var    request    = UnityWebRequest.Post(RequestURL, json);
			byte[] jsonToSend = new UTF8Encoding().GetBytes(json);
			request.uploadHandler = new UploadHandlerRaw(jsonToSend);
			request.SetRequestHeader("Content-Type",  "application/json");
			request.SetRequestHeader("Authorization", "Bearer " + token);
			await request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				C2VDebug.LogError($"jira request failed to {request.error}");
				return;
			}
			
			var response = JsonUtility.FromJson<ResultResponse>(request.downloadHandler.text);
			var issueURL = ZString.Format(JiraIssueURLFormat, response.key);
			C2VDebug.Log($"jira request is success!\nURL {issueURL}");
			EditorGUIUtility.systemCopyBuffer = issueURL;
		}
	}
}