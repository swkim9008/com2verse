/*===============================================================
* Product:		Com2Verse
* File Name:	BuildReportExporter.cs
* Developer:	jhkim
* Date:			2022-06-15 16:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using Com2Verse.Logger;
using Com2Verse.Serialization;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com2VerseEditor.Build
{
	public static class BuildReportExporter
	{
		private static readonly string BuildPath = Path.Combine("Build", "Executable");
		private static readonly string BuildReportFilePath = Path.Combine(BuildPath, "..", "Report", "BuildReport.json");
		private static StringBuilder _sb, _sb2;
		private static int _indentLevel = 0;
		public static void ExportToJson(BuildReport report)
		{
			if (report == null)
			{
				C2VDebug.LogError($"No Build Report");
				return;
			}

			var data = Convert(report);
			//var reportHtml = ToHtml(data);
			var reportJson = ToJson(data);
			CreateFile(BuildReportFilePath, reportJson);
		}

#region Parsing
		static BuildReportData Convert(BuildReport report)
		{
			var    buildReportPath = "Library/LastBuild.buildreport";
			string buildTime       = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			if (File.Exists(buildReportPath))
				buildTime = File.GetCreationTime(buildReportPath).ToString("yyyy/MM/dd HH:mm:ss");

			return new BuildReportData
			{
				buildName           = Application.productName,
				platform            = report.summary.platform.ToString(),
				totalTime           = FormatTime(report.summary.totalTime),
				totalSize           = FormatSize(report.summary.totalSize),
				buildResult         = report.summary.result.ToString(),
				buildTime           = buildTime,
				scriptDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
			};
		}
#endregion // Parsing

#region Json
		static string ToJson(BuildReportData data)
		{
			var json = JsonUtility.ToJson(data);
			return json;
		}
#endregion

#region HTML
		// static string ToHtml(BuildReportData data)
		// {
		// 	_sb = new StringBuilder();
		// 	_sb2 = new StringBuilder();
		// 	BeginHtml();
		// 	BeginHead();
		// 	ImportScript();
		// 	EndHead();
		// 	BeginBody();
		// 	AppendBriefInfo(data);
		// 	// AppendDetailInfo(data);
		// 	EndBody();
		// 	EndHtml();
		// 	return _sb.ToString();
		// }
		//
		// static void AppendBriefInfo(BuildReportData data)
		// {
		// 	H1("Report Info");
		// 	CreateTable(new string[]
		// 	{
		// 		"Category",
		// 		"Value"
		// 	}, new string[][]
		// 	{
		// 		new string[] {"Build Name", data.BuildInfoData.buildName},
		// 		new string[] {"Platform", data.BuildInfoData.platform},
		// 		new string[] {"Build Time", data.BuildInfoData.buildTime},
		// 		new string[] {"Total Time", data.BuildInfoData.totalTime},
		// 		new string[] {"Total Size", data.BuildInfoData.totalSize},
		// 		new string[] {"Build Result", data.BuildInfoData.buildResult},
		// 	});
		// }
		//
		// static void AppendDetailInfo(BuildReportData data)
		// {
		// 	var categories = new TabCategory[]
		// 	{
		// 		new() {Name = "BuildSteps", OnDrawInfo = AppendBuildSteps},
		// 		new() {Name = "SourceAssets", OnDrawInfo = AppendSourceAssets},
		// 		new() {Name = "OutputFiles", OnDrawInfo = AppendOutputFiles},
		// 		new() {Name = "Stripping", OnDrawInfo = AppendStripping},
		// 		new() {Name = "SceneUsingAssets", OnDrawInfo = AppendSceneUsingAssets},
		// 	};
		// 	CreateTabs(categories.Select(category => category.Name).ToArray(), id =>
		// 	{
		// 		var idx = Array.FindIndex(categories, item => item.Name.Equals(id));
		// 		if (idx >= 0)
		// 			categories[idx].OnDrawInfo(data);
		// 	});
		// }
		//
		// static void AppendBuildSteps(BuildReportData data)
		// {
		// 	AppendLine("Build Steps");
		// }
		// static void AppendSourceAssets(BuildReportData data)
		// {
		// 	AppendLine("Source Assets");
		// }
		// static void AppendOutputFiles(BuildReportData data)
		// {
		// 	AppendLine("Output Files");
		// }
		// static void AppendStripping(BuildReportData data)
		// {
		// 	AppendLine("Stripping");
		// }
		// static void AppendSceneUsingAssets(BuildReportData data)
		// {
		// 	AppendLine("Scene Using Assets");
		// }
		//
		// static void BeginHtml() => Begin("html");
		// static void EndHtml() => End("html");
		// static void BeginHead() => Begin("head");
		// static void EndHead() => End("head");
		// static void BeginScript() => Begin("script");
		// static void EndScript() => End("script");
		// static void BeginBody() => Begin("body");
		// static void EndBody() => End("body");
		// static void ImportScript()
		// {
		// 	AppendLine("<!--Compiled and minified CSS-->");
		// 	AppendLine("<link rel = \"stylesheet\" href = \"https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css\">");
		// 	AppendLine( "<!--Compiled and minified JavaScript-->");
		// 	AppendLine("<script src = \"https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/js/materialize.min.js\"></script>");
		// 	InitMaterialize();
		// }
		//
		// static void InitMaterialize()
		// {
		// 	BeginScript();
		// 	Indent();
		// 	AppendLine("document.addEventListener('DOMContentLoaded', function() {");
		// 	Indent();
		// 	AppendLine("M.Tabs.init(document.querySelectorAll('.tabs'), {});");
		// 	Outdent();
		// 	AppendLine("});");
		// 	Outdent();
		// 	EndScript();
		// }
		// static void H1(string msg) => AddWithTag("h1", msg);
		// static void H2(string msg) => AddWithTag("h2", msg);
		// static void H3(string msg) => AddWithTag("h3", msg);
		// static void H4(string msg) => AddWithTag("h4", msg);
		// static void H5(string msg) => AddWithTag("h5", msg);
		//
		// static void CreateList(params string[] items)
		// {
		// 	BeginList();
		// 	AddListItem(items);
		// 	EndList();
		// }
		// static void BeginList(string id = "", params string[] classItems) => Begin("ul", id, classItems);
		// static void EndList() => End("ul");
		// static void AddListItem(string[] items, params string[] classItems)
		// {
		// 	foreach (var item in items)
		// 		AddWithTag("li", item, classItems);
		// }
		//
		// static void CreateTable(string[] headerItems, string[][] bodyItems)
		// {
		// 	BeginTable("highlight");
		// 	BeginTableHeader();
		// 	foreach (var header in headerItems)
		// 		AddTableHeaderItem(header);
		// 	EndTableHeader();
		// 	BeginTableBody();
		// 	foreach (var items in bodyItems)
		// 	{
		// 		BeginTableRow();
		// 		foreach (var item in items)
		// 			AddTableItem(item);
		// 		EndTableRow();
		// 	}
		// 	EndTableBody();
		// 	EndTable();
		// }
		// static void BeginTable(string id = "", params string[] classItems) => Begin("table", id, classItems);
		// static void EndTable() => End("table");
		// static void BeginTableHeader() => Begin("thead");
		// static void EndTableHeader() => End("thead");
		// static void BeginTableBody() => Begin("tbody");
		// static void EndTableBody() => End("tbody");
		// static void BeginTableRow() => Begin("tr");
		// static void EndTableRow() => End("tr");
		// static void AddTableHeaderItem(string msg) => AddWithTag("th", msg);
		// static void AddTableItem(string msg) => AddWithTag("td", msg);
		// static void BeginDiv(string id = "", params string[] classItems) => Begin("div", id, classItems);
		// static void EndDiv() => End("div");
		//
		// static void CreateTabs(string[] tabs, Action<string> onTabContents)
		// {
		// 	BeginDiv(string.Empty, "row");
		// 	BeginDiv(string.Empty, "col", "s12");
		// 	BeginList(string.Empty, "tabs", "tabs-fixed-width");
		//
		// 	var listItems = new string[tabs.Length];
		// 	var styles = new[] {"tab", "col", "s3"};
		// 	for(int i = 0; i < listItems.Length; ++i)
		// 		listItems[i] = $"<a href='#{tabs[i]}'>{tabs[i]}</a>";
		// 	AddListItem(listItems, styles);
		// 	EndList();
		// 	EndDiv();
		// 	foreach (var tab in tabs)
		// 	{
		// 		BeginDiv(tab, "col", "s12");
		// 		onTabContents?.Invoke(tab);
		// 		EndDiv();
		// 	}
		// 	EndDiv();
		// }
		// static void Begin(string tag, string id = "", params string[] classItems)
		// {
		// 	var classStr = GetClassStr(classItems);
		// 	var idStr = string.IsNullOrWhiteSpace(id) ? string.Empty : $"id=\"{id}\"";
		// 	AppendLine($"<{tag} {idStr} {classStr}>");
		// 	Indent();
		// }
		// static void End(string tag)
		// {
		// 	Outdent();
		// 	AppendLine($"</{tag}>");
		// }
		// static void Indent() => _indentLevel++;
		// static void Outdent() => _indentLevel--;
		//
		// static void AddWithTag(string tag, string msg, params string[] classItems)
		// {
		// 	var classStr = GetClassStr(classItems);
		// 	AppendLine($"<{tag} {classStr}>{msg}</{tag}>");
		// }
		//
		// static void AppendLine(string msg)
		// {
		// 	for (int i = 0; i < _indentLevel; ++i)
		// 		_sb.Append("\t");
		// 	_sb.AppendLine(msg);
		// }
		//
		// static string GetClassStr(params string[] classItems)
		// {
		// 	var classStr = string.Empty;
		// 	if (classItems.Length > 0)
		// 	{
		// 		_sb2.Clear();
		// 		_sb2.Append("class=\"");
		// 		foreach (var item in classItems)
		// 			_sb2.Append($"{item} ");
		// 		_sb2.Append("\"");
		// 		classStr = _sb2.ToString();
		// 	}
		// 	return classStr;
		// }
#endregion // HTML

#region Util
		static string FormatTime(System.TimeSpan t) => $"{t.Hours}:{t.Minutes.ToString("D2")}:{t.Seconds.ToString("D2")}.{t.Milliseconds.ToString("D3")}";
		static string FormatSize(ulong size)
		{
			if (size < 1024)
				return size + " B";
			if (size < 1024 * 1024)
				return (size / 1024.00).ToString("F2") + " KB";
			if (size < 1024 * 1024 * 1024)
				return (size / (1024.0 * 1024.0)).ToString("F2") + " MB";
			return (size / (1024.0 * 1024.0 * 1024.0)).ToString("F2") + " GB";
		}
		static void CreateFile(string filePath, string text)
		{
			Util.CreateDirectory(Path.GetDirectoryName(filePath));
			if(File.Exists(filePath))
				File.Delete(filePath);
			File.WriteAllText(filePath, text, Encoding.UTF8);
			C2VDebug.Log($"CreateFile: {filePath}\n{text}");
		}
#endregion // Util

#region Data
		[Serializable]
		struct BuildReportData
		{
			public string buildName;
			public string platform;
			public string totalTime;
			public string totalSize;
			public string buildResult;
			public string buildTime;
			public string scriptDefineSymbols;
		}

		[Serializable]
		struct BuildInfoData
		{
		
		}

		[Serializable]
		struct BuildStepsData
		{
		}
		[Serializable]
		struct SourceAssetsData { }
		[Serializable]
		struct OutputFilesData
		{
		}
		[Serializable]
		struct StrippingData
		{
		}
		struct TabCategory
		{
			public string Name;
			public Action<BuildReportData> OnDrawInfo;
		}
#endregion // Data
		[MenuItem("Com2Verse/Build/Export BuildReport Test")]
		static void ExportBuildReportTest()
		{
			var buildReportPath = "Library/LastBuild.buildreport";
			var buildReportAssetPath = "Assets/LastBuildReport.buildreport";
			BuildReport report = null;
			if (File.Exists(buildReportPath))
			{
				if(File.Exists(buildReportAssetPath))
					File.Delete(buildReportAssetPath);
				File.Copy(buildReportPath, buildReportAssetPath);
				AssetDatabase.ImportAsset(buildReportAssetPath);
				report = AssetDatabase.LoadAssetAtPath<BuildReport>(buildReportAssetPath);
				ExportToJson(report);
				File.Delete(buildReportAssetPath);
			}
			else
				C2VDebug.LogError("No Build Report");
		}
	}
}
