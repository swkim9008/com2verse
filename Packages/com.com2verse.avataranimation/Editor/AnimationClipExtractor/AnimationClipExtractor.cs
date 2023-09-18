/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationClipExtractor.cs
* Developer:	eugene9721
* Date:			2023-02-23 14:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com2Verse.Logger;
using UnityEditor;
using Com2VerseEditor.UGC.UIToolkitExtension;
using Com2VerseEditor.UGC.UIToolkitExtension.Containers;
using Com2VerseEditor.UGC.UIToolkitExtension.Controls;
using Com2VerseEditor.UGC.UIToolkitExtension.Extensions;
using Cysharp.Text;
using UnityEngine.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2VerseEditor.AvatarAnimation
{
	public class AnimationClipExtractor : EditorWindowEx
	{
#region InternalClass
		private class Buttons
		{
			public RoundButton SelectAllClipOfCurrentType { get; }
			public RoundButton SelectAllClip              { get; }
			public ButtonEx    ExtractClip                { get; }

			public Buttons(RoundButton selectAllClipOfCurrentType, RoundButton selectAllClip, ButtonEx extractClip)
			{
				SelectAllClipOfCurrentType = selectAllClipOfCurrentType;
				SelectAllClip              = selectAllClip;
				ExtractClip                = extractClip;
			}
		}
#endregion InternalClass

#region Fields
		private readonly Dictionary<(eAnimationClipType, AnimationClip), bool> _selectAnimationClips = new();

		private eAnimationClipType _currentType = eAnimationClipType.MOVEMENT;
		private eAnimationClipType _lastType    = eAnimationClipType.NONE;

		private HashSet<(eAnimationClipType, AnimationClip)>? _animationClips;

		private Vector2 _scrollPosition;
		private string  _basePath               = string.Empty;
		private string  _selectedFolderName     = string.Empty;
		private string  _lastSelectedFolderName = string.Empty;

		private LabelEx?         _selectedFolderLabel;
		private Buttons?         _buttons;
		private VisualElementEx? _content;
		private VisualElementEx? _clipTypeButtonHeader;
		private ToggleEx?        _preserveEvent;

		private readonly List<ToggleEx> _clipToggles = new();
#endregion Fields

#region EditorWindowEx
		[MenuItem("Com2Verse/Animation/Extract AnimationClips", priority = 2)]
		public static void Open()
		{
			List<(eAnimationClipType, AnimationClip)> animationClips = new();
			if (!CheckSelection(animationClips, out var basePath, out var selectedFolderName)) return;

			var window = GetWindow<AnimationClipExtractor>();
			if (window == null)
			{
				C2VDebug.LogErrorCategory(nameof(AnimationClipExtractor), "Failed to open window");
				return;
			}

			window.SetConfig(window, new Vector2Int(600, 800), "AnimationClipExtractor");
			window.Initialize(animationClips, basePath, selectedFolderName);
			window.OnDraw();
		}

		protected override void OnStart(VisualElement root)
		{
			base.OnStart(root);
			if (!TryGetControls())
			{
				C2VDebug.LogError(nameof(AnimationClipExtractor), "Failed to check hierarchy");
				Clear();
				return;
			}

			Selection.selectionChanged += OnSelectionChanged;
			AddClipTypeButtons();
		}

		protected override void OnDraw(VisualElement root)
		{
			if (_selectedFolderLabel != null)
				_selectedFolderLabel.text = $"현재 선택된 폴더: {_selectedFolderName}";
			RefreshClipToggles();
			base.OnDraw(root);
		}

		protected override void OnClear(VisualElement root)
		{
			base.OnClear(root);
			Selection.selectionChanged -= OnSelectionChanged;
			Clear();
		}

		private void Clear()
		{
			ClearButtonEvents();
			ClearClipToggles();

			_buttons              = null;
			_content              = null;
			_clipTypeButtonHeader = null;
			_preserveEvent        = null;

			_selectAnimationClips.Clear();
			_animationClips = null;
		}
#endregion EditorWindowEx

#region GUI
		private void OnSelectionChanged()
		{
			List<(eAnimationClipType, AnimationClip)> animationClips = new();
			if (!CheckSelection(animationClips, out var basePath, out var selectedFolderName)) return;

			Initialize(animationClips, basePath, selectedFolderName);
			OnDraw();
		}

		private bool TryGetControls()
		{
			var header  = Find<VisualElementEx>("Header");
			var content = Find<VisualElementEx>("Content");
			var footer  = Find<VisualElementEx>("Footer");

			if (header == null || content == null || footer == null)
			{
				C2VDebug.LogErrorCategory(nameof(AnimationClipExtractor), "Failed to find header, content, footer");
				return false;
			}

			var selectAllClipOfCurrentType = Find<RoundButton>(header, "Btn_SelectAllClipOfCurrentType");
			var selectAllClip              = Find<RoundButton>(header, "Btn_SelectAllClip");
			var extractClip                = Find<ButtonEx>(footer, "Btn_ExtractAnimationClip");

			if (selectAllClipOfCurrentType == null || selectAllClip == null || extractClip == null)
			{
				C2VDebug.LogErrorCategory(nameof(AnimationClipExtractor), "Failed to find button control");
				return false;
			}

			_buttons = new Buttons(
				selectAllClipOfCurrentType,
				selectAllClip,
				extractClip
			);

			AddButtonEvents();

			_selectedFolderLabel  = Find<LabelEx>(header, "SelectedFolderLabel");
			_clipTypeButtonHeader = Find<VisualElementEx>(header, "ClipTypeButtonHeader");
			_preserveEvent        = Find<ToggleEx>(footer, "Toggle_PreserveEvent");
			_content              = content;

			return true;
		}

		private void AddButtonEvents()
		{
			if (_buttons == null) return;

			_buttons.SelectAllClipOfCurrentType.RegisterCallback<MouseDownEvent>(OnSelectAllClipOfCurrentType, TrickleDown.TrickleDown);
			_buttons.SelectAllClip.RegisterCallback<MouseDownEvent>(OnSelectAllClip, TrickleDown.TrickleDown);
			_buttons.ExtractClip.RegisterCallback<MouseDownEvent>(OnExtractClip, TrickleDown.TrickleDown);
		}

		private void ClearButtonEvents()
		{
			if (_buttons == null) return;

			_buttons.SelectAllClipOfCurrentType.UnregisterCallback<MouseDownEvent>(OnSelectAllClipOfCurrentType);
			_buttons.SelectAllClip.UnregisterCallback<MouseDownEvent>(OnSelectAllClip);
			_buttons.ExtractClip.UnregisterCallback<MouseDownEvent>(OnExtractClip);
		}

		private void AddClipTypeButtons()
		{
			foreach (eAnimationClipType animationClipType in Enum.GetValues(typeof(eAnimationClipType)))
			{
				if (animationClipType == eAnimationClipType.NONE) continue;

				var button = new RoundButton
				{
					text = animationClipType.ToString(),
					name = animationClipType.ToString(),
				};
				if (!AddVisualElement(_clipTypeButtonHeader!, button, animationClipType.ToString()))
				{
					C2VDebug.LogWarningCategory(nameof(AnimationClipExtractor), $"Failed to add button : {animationClipType.ToString()}");
					continue;
				}

				button.RegisterCallback(OnClipTypeButton(animationClipType), TrickleDown.TrickleDown);
			}
		}

		private void ClearClipToggles()
		{
			if (_content == null) return;

			_content.Clear();
			_clipToggles.Clear();
		}

		private void SelectAllClipOfCurrentType()
		{
			foreach (var clipToggle in _clipToggles)
				clipToggle.value = true;
			OnDraw();
		}

		private void RefreshClipToggles()
		{
			var isSelectionChanged = _lastType != _currentType || _lastSelectedFolderName != _selectedFolderName;
			if (isSelectionChanged && _content != null && _animationClips != null)
			{
				ClearClipToggles();

				foreach (var clip in _animationClips)
				{
					var type = clip.Item1;
					if (type != _currentType) continue;

					var toggleName = clip.Item2.name;
					var toggle = new ToggleEx
					{
						name  = toggleName,
						text  = $"{clip.Item1.ToString()}, {toggleName}",
						value = _selectAnimationClips[clip],
					};

					if (!AddVisualElement(_content, toggle, toggleName))
					{
						C2VDebug.LogWarningCategory(nameof(AnimationClipExtractor), $"Failed to add toggle: {toggleName}");
						continue;
					}

					toggle.RegisterValueChangedCallback(_ => _selectAnimationClips[clip] = toggle.value);
					_clipToggles.Add(toggle);
				}
			}

			_lastType               = _currentType;
			_lastSelectedFolderName = _selectedFolderName;
		}
#endregion GUI

#region ButtonEvents
		private void OnSelectAllClipOfCurrentType(MouseDownEvent evt)
		{
			SetCurrentTypeClips(_currentType);
			SelectAllClipOfCurrentType();
		}

		private void OnSelectAllClip(MouseDownEvent evt)
		{
			SetAllClips();
			SelectAllClipOfCurrentType();
		}

		private void OnExtractClip(MouseDownEvent evt)
		{
			ExtractClipAssets(_preserveEvent?.value ?? true);
		}

		private EventCallback<MouseDownEvent> OnClipTypeButton(eAnimationClipType clipType)
		{
			return _ =>
			{
				_currentType = clipType;
				OnDraw();
			};
		}
#endregion ButtonEvents

#region ClipData
		private void Initialize(ICollection<(eAnimationClipType, AnimationClip)> animationClips, string basePath, string folderName)
		{
			_animationClips = animationClips.ToHashSet();
			_selectAnimationClips.Clear();
			foreach (var clip in _animationClips)
				_selectAnimationClips.Add(clip, false);

			_basePath           = basePath;
			_selectedFolderName = folderName;
		}

		private void SetAllClips()
		{
			foreach (eAnimationClipType type in Enum.GetValues(typeof(eAnimationClipType)))
				SetCurrentTypeClips(type);
		}

		private void SetCurrentTypeClips(eAnimationClipType clipType)
		{
			if (_animationClips == null) return;
			var keys = _selectAnimationClips.Keys.ToList();
			foreach (var clip in keys)
			{
				var type                                          = clip.Item1;
				if (type == clipType) _selectAnimationClips[clip] = true;
			}
		}

		private void ExtractClipAssets(bool preserveResponse)
		{
			var fbxFolderPath  = Path.Combine(_basePath, AnimatorBuilderDefine.AnimFBXFolderName);
			var clipFolderPath = Path.Combine(_basePath, AnimatorBuilderDefine.AnimClipFolderName);

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"FBX 파일에서 애니메이션을 추출합니다...\n이벤트 보존 체크: {preserveResponse.ToString()}\n----------");
			foreach (eAnimationClipType animationClipType in Enum.GetValues(typeof(eAnimationClipType)))
			{
				var clipTypeName       = animationClipType.ToString().ToLower();
				var fbxTypeFolderPath  = Path.Combine(fbxFolderPath, clipTypeName);
				var clipTypeFolderPath = Path.Combine(clipFolderPath, clipTypeName);
				if (!Directory.Exists(fbxTypeFolderPath)) continue;
				var fbxClipPath = Directory.GetFiles(fbxTypeFolderPath);

				foreach (var fileName in fbxClipPath)
				{
					List<AnimationClip> animationClips = new();
					AnimationEditorUtils.LoadAnimationClips(fileName, animationClips);
					foreach (var clip in animationClips)
					{
						if (!_selectAnimationClips.ContainsKey((animationClipType, clip))) continue;
						if (!_selectAnimationClips[(animationClipType, clip)]) continue;
						if (clip.name.StartsWith(AnimatorBuilderDefine.PreviewHeader)) continue;
						if (clip.name.Contains(AnimatorBuilderDefine.PreviewAnimation)) continue;

						var clipPath = $"{Path.Combine(clipTypeFolderPath, clip.name)}.anim";

						var prevClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
						if (prevClip == null) SaveAnimationClip(clip, clipPath, stringBuilder);
						else OverwriteAnimationClip(clip, prevClip, clipPath, clipFolderPath, preserveResponse, stringBuilder);
					}
				}
			}

			C2VDebug.LogCategory(nameof(AnimationClipExtractor), stringBuilder.ToString());
			AssetDatabase.Refresh();
		}

		private void SaveAnimationClip(AnimationClip clip, string clipPath, Utf16ValueStringBuilder stringBuilder)
		{
			var temp = new AnimationClip();

			EditorUtility.CopySerialized(clip, temp);
			AssetDatabase.CreateAsset(temp, clipPath);
			AssetDatabase.SaveAssets();
			stringBuilder.AppendLine($"생성: {clipPath}");
		}

		private void OverwriteAnimationClip(AnimationClip clip, AnimationClip prevClip, string clipPath,
		                                    string clipTypeFolderPath, bool preserveResponse, Utf16ValueStringBuilder stringBuilder)
		{
			var temp = new AnimationClip();

			AssetDatabase.DisallowAutoRefresh();
			var tempDirectoryPath = Path.Combine(clipTypeFolderPath, AnimatorBuilderDefine.TempDirectoryName);
			Directory.CreateDirectory(tempDirectoryPath);

			AnimationEvent[]? animationEvents     = null;
			if (preserveResponse) animationEvents = prevClip.events;

			var tempPath = $"{Path.Combine(tempDirectoryPath, $"{clip.name}")}.anim";
			EditorUtility.CopySerialized(clip, temp);
			if (preserveResponse && animationEvents != null) AnimationUtility.SetAnimationEvents(temp, animationEvents);

			AssetDatabase.CreateAsset(temp, tempPath);
			FileUtil.ReplaceFile(tempPath, clipPath);

			AssetDatabase.DeleteAsset(tempPath);
			AssetDatabase.DeleteAsset(tempDirectoryPath);

			AssetDatabase.SaveAssets();
			AssetDatabase.ImportAsset(clipPath);
			AssetDatabase.AllowAutoRefresh();
			stringBuilder.AppendLine($"대체: {clipPath}");
		}

		private static bool CheckSelection(ICollection<(eAnimationClipType, AnimationClip)> animationClips, out string basePath, out string selectedFolderName)
		{
			Object[]? selectedObject = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);

			selectedFolderName = string.Empty;
			basePath           = string.Empty;

			if (!AnimationEditorUtils.FolderConditionCheck(selectedObject)) return false;

			selectedFolderName = selectedObject![0].name;
			basePath           = AssetDatabase.GetAssetPath(selectedObject[0]) ?? string.Empty;

			if (string.IsNullOrEmpty(basePath)) return false;

			var fbxFolderPath = Path.Combine(basePath, AnimatorBuilderDefine.AnimFBXFolderName!);

			foreach (eAnimationClipType animationClipType in Enum.GetValues(typeof(eAnimationClipType)))
			{
				var clipTypeName      = animationClipType.ToString().ToLower();
				var fbxTypeFolderPath = Path.Combine(fbxFolderPath, clipTypeName);
				if (!Directory.Exists(fbxTypeFolderPath)) continue;
				var fbxClipPath = Directory.GetFiles(fbxTypeFolderPath);

				List<AnimationClip> loadClips = new();
				foreach (var fileName in fbxClipPath)
				{
					loadClips.Clear();
					AnimationEditorUtils.LoadAnimationClips(fileName, loadClips);
					foreach (var clip in loadClips)
					{
						if (clip.name.StartsWith(AnimatorBuilderDefine.PreviewHeader)) continue;
						if (clip.name.Contains(AnimatorBuilderDefine.PreviewAnimation)) continue;
						animationClips.Add((animationClipType, clip));
					}
				}
			}

			return true;
		}
#endregion ClipData
	}
}
