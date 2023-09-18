/*===============================================================
* Product:		Com2Verse
* File Name:	AppInfoUI.cs
* Developer:	jhkim
* Date:			2022-06-10 15:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse
{
	public sealed class AppInfoUI : MonoBehaviour
	{
		[SerializeField] private Canvas _canvas;
		[SerializeField] private TMP_Text _titleObj;
		[SerializeField] private TMP_Text _labelObj;
		[SerializeField] private TMP_InputField _inputObj;
		[SerializeField] private MetaverseButton _btnObj;
		[SerializeField] private LayoutElement _spaceObj;

		private Transform _parent;
		private List<Transform> _uiChilds = new();
		private AppInfo _appInfo;
		private void Awake()
		{
			_appInfo = AppInfo.Instance;
			_parent = _labelObj.transform.parent;

			var showUI = _appInfo.Data.IsForceEnableAppInfo || _appInfo.Data.IsDebug;
			if (showUI)
			{
				Init();
				ClearUi();
				SetAppInfoUi();
			}
			ShowCanvas(showUI);
			_parent.gameObject.SetActive(showUI);
		}

		void Init()
		{
			_titleObj?.gameObject.SetActive(false);
			_labelObj?.gameObject.SetActive(false);
			_inputObj?.gameObject.SetActive(false);
			_btnObj?.gameObject.SetActive(false);
			_spaceObj?.gameObject.SetActive(false);
		}
		void ClearUi()
		{
			foreach (var child in _uiChilds)
				Destroy(child.gameObject);
			_uiChilds.Clear();
		}
		void SetAppInfoUi()
		{
			string GetOX(bool check) => check ? "O" : "X";
			var data = _appInfo.Data;
			AddTitle("App Info");
			AddLabel($"Name : {data.AppName}");
			AddLabel($"Version : {data.Version}");
			AddSpace();
			AddTitle("Build Info");
			AddLabel($"Build Time : {data.BuildTime}");
			AddLabel($"Environment : {data.Environment}");
			AddLabel($"BuildTarget : {data.BuildTarget}");
			AddLabel($"Asset Build Type : {data.AssetBuildType}");
			AddLabel($"Scripting Backend : {data.ScriptingBackend}");
			AddLabel($"IsDebug : {GetOX(data.IsDebug)}");
			AddLabel($"IsForceSingleInstance : {GetOX(data.IsForceSingleInstance)}");
			AddLabel($"IsForceEnableAppInfo : {GetOX(data.IsForceEnableAppInfo)}");
			AddLabel($"IsForceEnableSentry : {GetOX(data.IsForceEnableSentry)}");
			AddSpace();
			AddTitle("Git Info");
			AddLabel("Branch");
			AddInput(data.GitBuildBranch);
			AddLabel("HASH");
			AddInput(data.GitCommitHash);
			if(!string.IsNullOrWhiteSpace(data.GitCommitHash))
				AddButton("Open URL", () => Application.OpenURL(data.GetCommitHashURL()));
			AddLabel("Revision Count");
			AddInput(data.GitRevisionCount);
		}
		void AddTitleLabel(string title, string label)
		{
			AddTitle(title);
			AddLabel(label);
		}
		void AddTitle(string message) => AddLabelText(_titleObj, message);
		void AddLabel(string message) => AddLabelText(_labelObj, message);
		void AddLabelText(TMP_Text obj, string message)
		{
			var textObj = Instantiate(obj, _parent);
			textObj.text = message;
			textObj.gameObject.SetActive(true);
			_uiChilds.Add(textObj.transform);
		}

		void AddInput(string message, string placeHolder = "")
		{
			var inputObj = Instantiate(_inputObj, _parent);
			inputObj.text = message;
			inputObj.placeholder.GetComponent<TMP_Text>().text = placeHolder;
			inputObj.gameObject.SetActive(true);
			inputObj.onValueChanged.AddListener(str => inputObj.text = str);
			_uiChilds.Add(inputObj.transform);
		}

		void AddButton(string label, Action onClick)
		{
			var btnObj = Instantiate(_btnObj, _parent);
			btnObj.GetComponentInChildren<TMP_Text>().text = label;
			btnObj.onClick.RemoveAllListeners();
			btnObj.onClick.AddListener(() =>
			{
				onClick?.Invoke();
			});
			btnObj.gameObject.SetActive(true);
			_uiChilds.Add(btnObj.transform);
		}

		void AddSpace(float height = 10f)
		{
			var spaceObj = Instantiate(_spaceObj, _parent);
			spaceObj.preferredHeight = height;
			spaceObj.gameObject.SetActive(true);
			_uiChilds.Add(spaceObj.transform);
		}
		private void ShowCanvas(bool show) => _canvas.enabled = show;
	}
}
