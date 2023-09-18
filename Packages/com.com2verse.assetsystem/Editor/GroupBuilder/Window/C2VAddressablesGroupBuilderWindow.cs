/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGroupRule.cs
* Developer:	tlghks1009
* Date:			2023-03-02 16:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Com2VerseEditor.AssetSystem
{
	public sealed class C2VAddressablesGroupBuilderWindow : EditorWindow
	{
		private C2VAddressablesEditorCompositionRoot _compositionRoot;

		private C2VAddressablesGroupRuleTreeView _groupRuleTreeView;

		private TreeViewState _treeViewState;

		public readonly C2VAddressablesEvent               OnCreateMenuClick                = new();
		public readonly C2VAddressablesEvent               OnSaveButtonClick                = new();
		public readonly C2VAddressablesEvent               OnRemoveButtonClick              = new();
		public readonly C2VAddressablesEvent               OnCreateGroupButtonClick         = new();
		public readonly C2VAddressablesEvent               OnCleanCreateGroupButtonClick    = new();
		public readonly C2VAddressablesEvent               OnAutoPackagingButtonClick       = new();
		public readonly C2VAddressablesEvent<eEnvironment> OnEnvironmentSettingButtonClick  = new();
		public readonly C2VAddressablesEvent<eEnvironment> OnBuildButtonClick               = new();
		public readonly C2VAddressablesEvent<eEnvironment> OnCleanBuildButtonClick          = new();
		public readonly C2VAddressablesEvent<eEnvironment> OnBuildAndUploadButtonClick      = new();
		public readonly C2VAddressablesEvent               OnUpdatePreviousBuildButtonClick = new();


		public C2VAddressablesGroupRuleTreeView GroupRuleTreeView => _groupRuleTreeView;

		private void OnEnable()
		{
			if (EditorApplication.isUpdating || EditorApplication.isCompiling)
			{
				return;
			}

			_treeViewState = new TreeViewState();
			_groupRuleTreeView = new C2VAddressablesGroupRuleTreeView(_treeViewState);

			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("생성"), false, () => OnCreateMenuClick?.Invoke());
			menu.AddItem(new GUIContent("삭제"), false, () => OnRemoveButtonClick?.Invoke());
			menu.ShowAsContext();

			_groupRuleTreeView.RightClickMenu = menu;

			_compositionRoot = C2VAddressablesEditorCompositionRoot.RequestInstance(this);

			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}


		private void OnDisable()
		{
			_groupRuleTreeView?.Dispose();
			_groupRuleTreeView = null;

			_treeViewState = null;

			_compositionRoot?.Dispose();
			_compositionRoot = null;

			OnCreateMenuClick?.Dispose();
			OnSaveButtonClick?.Dispose();
			OnRemoveButtonClick?.Dispose();
			OnCreateGroupButtonClick?.Dispose();
			OnCleanCreateGroupButtonClick?.Dispose();
			OnAutoPackagingButtonClick?.Dispose();
			OnEnvironmentSettingButtonClick?.Dispose();
			OnBuildButtonClick?.Dispose();
			OnCleanBuildButtonClick?.Dispose();
			OnBuildAndUploadButtonClick?.Dispose();
			OnUpdatePreviousBuildButtonClick?.Dispose();

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}


		private void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				if (GUILayout.Button("메뉴", EditorStyles.toolbarDropDown, GUILayout.Width(80)))
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("생성"), false, () => OnCreateMenuClick?.Invoke());
					menu.AddItem(new GUIContent("툴 저장"), false, () => OnSaveButtonClick?.Invoke());
					menu.ShowAsContext();
				}

				if (GUILayout.Button("패키징", EditorStyles.toolbarDropDown, GUILayout.Width(80)))
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("패키징"), false, () => OnCreateGroupButtonClick?.Invoke());
					menu.AddItem(new GUIContent("클리어 후 패키징"), false, () => OnCleanCreateGroupButtonClick?.Invoke());
					menu.ShowAsContext();
				}

				if (GUILayout.Button("환경설정", EditorStyles.toolbarDropDown, GUILayout.Width(100)))
				{
					var storageService    = _compositionRoot.ServicePack.GetService<C2VAddressablesGroupBuilderStorageService>();
					var data              = storageService.AddressableGroupRuleData;
					var environment = data.Environment;

					var menu      = new GenericMenu();

					menu.AddItem(new GUIContent("Local 설정"),
					             environment == eEnvironment.LOCAL,
					             () => OnEnvironmentSettingButtonClick?.Invoke(eEnvironment.LOCAL));

					menu.AddItem(new GUIContent("Editor Hosted 설정"),
					             environment == eEnvironment.EDITOR_HOSTED,
					             () => OnEnvironmentSettingButtonClick?.Invoke(eEnvironment.EDITOR_HOSTED));

					menu.AddItem(new GUIContent("Remote 설정"),
					             environment == eEnvironment.REMOTE,
					             () => OnEnvironmentSettingButtonClick?.Invoke(eEnvironment.REMOTE));

					menu.ShowAsContext();
				}


				if (GUILayout.Button("빌드", EditorStyles.toolbarDropDown, GUILayout.Width(80)))
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Local 빌드"),
					             false,
					             () => OnBuildButtonClick?.Invoke(eEnvironment.LOCAL));
					menu.AddItem(new GUIContent("Editor Hosted 빌드"),
					             false,
					             () => OnBuildButtonClick?.Invoke(eEnvironment.EDITOR_HOSTED));
					menu.AddItem(new GUIContent("Remote 빌드"),
					             false,
					             () => OnBuildButtonClick?.Invoke(eEnvironment.REMOTE));

					//menu.AddSeparator(string.Empty);

					// menu.AddItem(new GUIContent("Remote 업데이트 빌드"),
					//              false,
					//              () => OnUpdatePreviousBuildButtonClick?.Invoke());

					menu.AddSeparator(string.Empty);

					menu.AddItem(new GUIContent("Remote 빌드 And 업로드"),
					             false,
					             () => OnBuildAndUploadButtonClick?.Invoke(eEnvironment.REMOTE));

					// menu.AddItem(new GUIContent("Remote 업데이트 빌드 And 업로드"),
					//              false,
					//              () => OnBuildAndUploadButtonClick?.Invoke(eBuildType.UPDATE, eEnvironment.REMOTE));
					menu.AddSeparator(string.Empty);

					menu.AddItem(new GUIContent("클린 Editor Hosted 빌드"),
					             false,
					             () => OnCleanBuildButtonClick?.Invoke(eEnvironment.EDITOR_HOSTED));

					menu.ShowAsContext();
				}

				if (GUILayout.Button("옵션", EditorStyles.toolbarDropDown, GUILayout.Width(80)))
				{
					var storageService = _compositionRoot.ServicePack.GetService<C2VAddressablesGroupBuilderStorageService>();
					bool isOn = storageService.IsAutoPackaging();

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("자동 패키징"), isOn, () => OnAutoPackagingButtonClick?.Invoke());
					menu.ShowAsContext();
				}
			}

			var toolbarHeight = EditorStyles.toolbar.fixedHeight;
			var treeViewRect = EditorGUILayout.GetControlRect(false);
			treeViewRect.height = position.height - toolbarHeight;

			_groupRuleTreeView?.OnGUI(treeViewRect);
		}


		[MenuItem("Com2Verse/AssetSystem/Addressable Group Builder %#F2")]
		public static void OpenWindow()
		{
			var window = GetWindow<C2VAddressablesGroupBuilderWindow>("Addressable Group Builder");
			window.minSize = new Vector2(1000, 300);
		}


		private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			switch (playModeStateChange)
			{
				case PlayModeStateChange.EnteredPlayMode:
				case PlayModeStateChange.EnteredEditMode:
				{
					OnDisable();
					OnEnable();

					Repaint();
				}
					break;
			}
		}
	}
}
