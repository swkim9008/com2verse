/*===============================================================
* Product:		Com2Verse
* File Name:	Editor.cs
* Developer:	jhkim
* Date:			2023-03-10 10:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEditor;
using Com2VerseEditor.UGC.UIToolkitExtension;
using Com2VerseEditor.UGC.UIToolkitExtension.Containers;
using Com2VerseEditor.UGC.UIToolkitExtension.Controls;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

namespace Com2Verse.BannedWords
{
	public class Editor : EditorWindowEx
	{
		private ListViewEx _bannedWordsList;
		private EditorModel _model;
		private Button _btnLoad;
		private TextFieldEx _inputBannedWord;
		private TextFieldEx _inputBannedWordResult;

		[MenuItem("Com2Verse/Tools/금칙어 확인", priority = 0)]
		public static void Open()
		{
			var window = GetWindow<Editor>();
			window.SetConfig(window, new Vector2Int(800, 600), "금칙어 확인");
		}

		protected override void OnStart(VisualElement root)
		{
			base.OnStart(root);

			if (metaData.modelObject != null)
				_model = metaData.modelObject as EditorModel;
			else if (modelObject != null)
				_model = modelObject as EditorModel;

			Initialize(root);
		}

		protected override void OnDraw(VisualElement root)
		{
			base.OnDraw(root);
		}

		protected override void OnClear(VisualElement root)
		{
			base.OnClear(root);
		}

#region Initialize
		private void Initialize(VisualElement root)
		{
			_bannedWordsList = root.Q<ListViewEx>("ListBannedWords");
			InitializeButtons(root);
			InitializeToggles(root);
			InitializeTextField(root);

			ClearTextField();
		}

		private void InitializeButtons(VisualElement root)
		{
			_btnLoad = ButtonQ(root, "BtnLoad", OnClickLoad);
			ButtonQ(root, "BtnTest", OnClickTest);
			ButtonQ(root, "BtnUsageAll", OnClickUsageAll);
			ButtonQ(root, "BtnUsageName", OnClickUsageName);
			ButtonQ(root, "BtnUsageSentence", OnClickUsageSentence);
		}

		private void InitializeToggles(VisualElement root)
		{
			var toggleIsStaging = root.Q<ToggleEx>("ToggleIsStaging");
			toggleIsStaging.UnregisterValueChangedCallback(OnToggleIsStaging);
			toggleIsStaging.RegisterValueChangedCallback(OnToggleIsStaging);
		}

		private void InitializeTextField(VisualElement root)
		{
			var inputFind = root.Q<TextFieldEx>("InputFind");
			inputFind.RegisterValueChangedCallback(value => RefreshTableUI());
		}
#endregion // Initialize

		private Button ButtonQ(VisualElement root, string name, Action onClick)
		{
			var btn = root.Q<Button>(name);
			if (btn == null) return null;

			btn.clicked -= onClick;
			btn.clicked += onClick;
			return btn;
		}

		private TableInfo RefreshTableUI()
		{
			if (_model.Info?.WordInfoMap == null) return null;

			_bannedWordsList.hierarchy.Clear();

			var map = _model.Info.WordInfoMap;
			var header = RowInfo.CreateRow(
				RowItemInfo.CreateLabel("Word"),
				RowItemInfo.CreateLabel("Lang"),
				RowItemInfo.CreateLabel("Country"),
				RowItemInfo.CreateLabel("Usage")
			);

			_bannedWordsList.hierarchy.Add(header.View);
			using var builder = TableInfo.Builder.New();
			foreach (var key in map.Keys)
			{
				foreach (var wordInfo in map[key])
				{
					if (string.IsNullOrWhiteSpace(_model.InputFind) || wordInfo.Word.Contains(_model.InputFind))
					{
						builder.AddRow(RowInfo.CreateRow(RowItemInfo.CreateButton(wordInfo.Word, () => { EditorGUIUtility.systemCopyBuffer = wordInfo.Word; }, 200),
						                                 RowItemInfo.CreateLabel(wordInfo.Lang, 40),
						                                 RowItemInfo.CreateLabel(wordInfo.Country, 40),
						                                 RowItemInfo.CreateLabel(wordInfo.Usage, 40)));
					}
				}
			}

			var table = builder.Create();
			table.OnNextPage += () =>
			{
				_bannedWordsList.hierarchy.Clear();
				_bannedWordsList.hierarchy.Add(header.View);
				_bannedWordsList.hierarchy.Add(table.View);
			};
			table.OnPrevPage += () =>
			{
				_bannedWordsList.hierarchy.Clear();
				_bannedWordsList.hierarchy.Add(header.View);
				_bannedWordsList.hierarchy.Add(table.View);
			};

			if (table.View != null)
				_bannedWordsList.hierarchy.Add(table.View);
			return table;
		}

#region Button Events
		private async void OnClickLoad()
		{
			if (_model == null)
			{
				Debug.LogWarning("Model is null");
				return;
			}

			var appDefine = new AppDefine
			{
				AppId = _model.AppId,
				Game = _model.GameName,
				Revision = _model.Revision,
				IsStaging = _model.IsStaging,
			};

			_btnLoad.SetEnabled(false);
			await RequestAsync(appDefine);

			if (_model == null || _model.Info == null)
			{
				_btnLoad.SetEnabled(true);
				return;
			}

			var table = RefreshTableUI();
			_btnLoad.SetEnabled(true);

			var menu = rootVisualElement.Q<VisualElement>("Menu");
			menu.hierarchy.Clear();

			if (table != null)
				menu.hierarchy.Add(table.PageView);
		}

		private void OnClickTest()
		{
			var testStr = _model.InputBannedWord;
			BannedWords.SetLanguageCode(_model.Lang);
			BannedWords.SetCountryCode(_model.Country);
			BannedWords.SetUsage(_model.Usage);
			_model.InputBannedWordResult = BannedWords.ApplyFilter(testStr, _model.InputReplace);
		}

		private void OnClickUsageAll() => SetUsage("All");
		private void OnClickUsageName() => SetUsage("Name");
		private void OnClickUsageSentence() => SetUsage("Sentence");
		private void SetUsage(string usage)
		{
			_model.Usage = usage;
			BannedWords.SetUsage(usage);
		}
#endregion // Button Events

		private async UniTask RequestAsync(AppDefine appDefine)
		{
			await BannedWords.CheckAndUpdateAsync(appDefine);
			var info = await BannedWords.LoadAsync(appDefine);
			_model.Info = info;
		}
		private void OnToggleIsStaging(ChangeEvent<bool> value) => _model.IsStaging = value.newValue;
		private void ClearTextField()
		{
			_model.Lang = _model.Country = _model.Usage = "All";
			_model.InputBannedWord = string.Empty;
			_model.InputReplace = string.Empty;
			_model.InputBannedWordResult = string.Empty;
		}
#region Table
		private class TableInfo
		{
			public VisualElement View
			{
				get
				{
					if (_pages == null || _pages.Count < _currentPage)
						_currentPage = 0;
					return _pages.Count == 0 ? null : _pages?[_currentPage];
				}
			}

			public VisualElement PageView => _pageView;
			private List<VisualElement> _pages = new();
			private int _currentPage = 0;
			private int _maxPage = 0;

			private VisualElement _pageView;
			private Label _pageLabel;

			public event Action OnNextPage = () => { };
			public event Action OnPrevPage = () => { };

			private void NextPage()
			{
				_currentPage = _currentPage + 1 < _maxPage ? _currentPage + 1 : 0;
				_pageLabel.text = GetPageLabelText();
				OnNextPage?.Invoke();
			}

			private void PrevPage()
			{
				_currentPage = _currentPage - 1 >= 0 ? _currentPage - 1 : _maxPage - 1;
				_pageLabel.text = GetPageLabelText();
				OnPrevPage?.Invoke();
			}

			private TableInfo() => _pageView = CreatePageView();
			private static TableInfo CreateTable(int itemPerPage, params RowInfo[] rowInfos)
			{
				var info = new TableInfo();
				var pageIdx = -1;
				for (var i = 0; i < rowInfos.Length; i++)
				{
					if (i % itemPerPage == 0)
					{
						info._pages.Add(new VisualElement());
						pageIdx++;
					}

					var view = info._pages[pageIdx];
					view.hierarchy.Add(rowInfos[i].View);
				}

				info._maxPage = pageIdx + 1;
				info._pageLabel.text = info.GetPageLabelText();
				return info;
			}

			private VisualElement CreatePageView()
			{
				var root = new VisualElement();
				var buttonLayout = new VisualElement();

				var prevBtn = new Button(PrevPage);
				var nextBtn = new Button(NextPage);

				prevBtn.text = "<";
				nextBtn.text = ">";

				buttonLayout.hierarchy.Add(prevBtn);
				buttonLayout.hierarchy.Add(nextBtn);
				buttonLayout.style.flexDirection = FlexDirection.Row;

				_pageLabel = new Label(GetPageLabelText());
				root.hierarchy.Add(_pageLabel);
				root.hierarchy.Add(buttonLayout);
				return root;
			}

			private string GetPageLabelText() => $"{_currentPage + 1} / {_maxPage}";
			public class Builder : IDisposable
			{
				private List<RowInfo> _rows = new();
				private int _itemPerPage = 100;
				private Builder() { }
				public static Builder New() => new Builder();
				public void AddRow(RowInfo rowInfo) => _rows.Add(rowInfo);
				public void SetItemPerPage(int itemPerPage)
				{
					_itemPerPage = itemPerPage < 1 ? 1 : itemPerPage;
				}
				public TableInfo Create() => CreateTable(_itemPerPage, _rows.ToArray());
				public void Dispose()
				{
					_rows.Clear();
					_rows = null;
				}
			}
		}

		private struct RowInfo
		{
			public VisualElement View { get; private set; }

			public static RowInfo CreateRow(params RowItemInfo[] rowItems)
			{
				var info = new RowInfo();
				info.View = new VisualElement();
				foreach (var rowItemInfo in rowItems)
					info.View.hierarchy.Add(rowItemInfo.View);

				info.View.style.flexDirection = FlexDirection.Row;
				info.View.style.justifyContent = Justify.SpaceBetween;
				info.View.style.minHeight = 25;
				return info;
			}
		}

		private struct RowItemInfo
		{
			public VisualElement View { get; private set; }

			public static RowItemInfo CreateLabel(string text, int? width = null)
			{
				var info = new RowItemInfo();
				var label = new Label(text);
				if (width != null)
					label.style.width = width.Value;
				info.View = label;
				return info;
			}

			public static RowItemInfo CreateButton(string text, Action onClick, int? width = null)
			{
				var info = new RowItemInfo();
				var button = new Button(onClick);
				button.text = text;
				if (width != null)
					button.style.width = width.Value;
				info.View = button;
				return info;
			}
		}
#endregion // Table
	}
}