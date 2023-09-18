/*===============================================================
* Product:    Com2Verse
* File Name:  RecyclableScrollRectEditor.cs
* Developer:  eugene9721
* Date:       2022-04-11 16:58
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.RecyclableScroll;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;

namespace Com2VerseEditor.RecyclableScroll
{
	[CustomEditor(typeof(RecyclableScrollRectBase), true)]
	[CanEditMultipleObjects]
	public class RecyclableScrollRectEditor : Editor
	{
		private static readonly string LAYOUT_DATA_KEY      = $"{nameof(RecyclableScrollRectEditor)}{nameof(_isShowLayoutData)}";
		private static readonly string INFINITE_SETTING_KEY = $"{nameof(RecyclableScrollRectEditor)}{nameof(_isShowInfiniteSetting)}";
		private static readonly string SNAP_SETTING_KEY     = $"{nameof(RecyclableScrollRectEditor)}{nameof(_isShowSnapSetting)}";

#region ScrollRect Fields
		private SerializedProperty _content;
		private SerializedProperty _movementType;
		private SerializedProperty _elasticity;
		private SerializedProperty _inertia;
		private SerializedProperty _decelerationRate;
		private SerializedProperty _scrollSensitivity;
		private SerializedProperty _viewport;
		private SerializedProperty _horizontalScrollbar;
		private SerializedProperty _verticalScrollbar;
		private SerializedProperty _onValueChanged;

		private AnimBool _showElasticity;
		private AnimBool _showDecelerationRate;
#endregion ScrollRect Fields

#region Enhanced Scroll Fields
		private SerializedProperty _scrollType;
		private SerializedProperty _scrollDirection;
		private SerializedProperty _scrollbarVisibility;
		private SerializedProperty _maxVelocity;
		private SerializedProperty _scrollPosition;

		private SerializedProperty _spacing;
		private SerializedProperty _padding;

		private SerializedProperty _loop;
		private SerializedProperty _loopWhileDragging;

		private SerializedProperty _snapping;
		private SerializedProperty _snapVelocityThreshold;
		private SerializedProperty _snapWatchOffset;
		private SerializedProperty _snapJumpToOffset;
		private SerializedProperty _snapCellCenterOffset;
		private SerializedProperty _snapUseCellSpacing;
		private SerializedProperty _snapTweenType;
		private SerializedProperty _snapTweenTime;
		private SerializedProperty _snapWhileDragging;
#endregion Enhanced Scroll Fields

		private SerializedProperty _prefab;

		private bool _isShowLayoutData;
		private bool _isShowInfiniteSetting;
		private bool _isShowSnapSetting;

		protected virtual void OnEnable()
		{
			OnEnableScrollRect();
			OnEnableEnhancedScrollRect();

			GetEditorToggleData();

			serializedObject.FindProperty("m_ScrollSensitivity").floatValue = 10.0f;
			serializedObject.ApplyModifiedProperties();
		}

		private void GetEditorToggleData()
		{
			_isShowLayoutData      = EditorPrefs.GetBool(LAYOUT_DATA_KEY, false);
			_isShowInfiniteSetting = EditorPrefs.GetBool(INFINITE_SETTING_KEY, false);
			_isShowSnapSetting     = EditorPrefs.GetBool(SNAP_SETTING_KEY, false);
		}

		private void OnEnableScrollRect()
		{
			_content             = serializedObject.FindProperty("m_Content");
			_movementType        = serializedObject.FindProperty("m_MovementType");
			_elasticity          = serializedObject.FindProperty("m_Elasticity");
			_inertia             = serializedObject.FindProperty("m_Inertia");
			_decelerationRate    = serializedObject.FindProperty("m_DecelerationRate");
			_scrollSensitivity   = serializedObject.FindProperty("m_ScrollSensitivity");
			_viewport            = serializedObject.FindProperty("m_Viewport");
			_horizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
			_verticalScrollbar   = serializedObject.FindProperty("m_VerticalScrollbar");
			_onValueChanged      = serializedObject.FindProperty("m_OnValueChanged");

			_showElasticity       = new AnimBool(Repaint);
			_showDecelerationRate = new AnimBool(Repaint);
			SetAnimBools(true);
		}

		private void OnEnableEnhancedScrollRect()
		{
			_prefab = serializedObject.FindProperty("_prefab");

			_scrollType          = serializedObject.FindProperty("_scrollType");
			_scrollDirection     = serializedObject.FindProperty("_scrollDirection");
			_scrollbarVisibility = serializedObject.FindProperty("_scrollbarVisibility");
			_maxVelocity         = serializedObject.FindProperty("_maxVelocity");
			_scrollPosition      = serializedObject.FindProperty("_scrollPosition");

			_spacing           = serializedObject.FindProperty("_spacing");
			_padding           = serializedObject.FindProperty("_padding");
			_loop              = serializedObject.FindProperty("_loop");
			_loopWhileDragging = serializedObject.FindProperty("_loopWhileDragging");

			_snapping              = serializedObject.FindProperty("_snapping");
			_snapVelocityThreshold = serializedObject.FindProperty("_snapVelocityThreshold");
			_snapWatchOffset       = serializedObject.FindProperty("_snapWatchOffset");
			_snapJumpToOffset      = serializedObject.FindProperty("_snapJumpToOffset");
			_snapCellCenterOffset  = serializedObject.FindProperty("_snapCellCenterOffset");
			_snapUseCellSpacing    = serializedObject.FindProperty("_snapUseCellSpacing");
			_snapTweenType         = serializedObject.FindProperty("_snapTweenType");
			_snapTweenTime         = serializedObject.FindProperty("_snapTweenTime");
			_snapWhileDragging     = serializedObject.FindProperty("_snapWhileDragging");
		}

		protected virtual void OnDisable()
		{
			_showElasticity.valueChanged.RemoveListener(Repaint);
			_showDecelerationRate.valueChanged.RemoveListener(Repaint);
		}

		void SetAnimBools(bool instant)
		{
			SetAnimBool(_showElasticity, !_movementType.hasMultipleDifferentValues && _movementType.enumValueIndex == (int)ScrollRect.MovementType.Elastic, instant);
			SetAnimBool(_showDecelerationRate, !_inertia.hasMultipleDifferentValues && _inertia.boolValue, instant);
		}

		void SetAnimBool(AnimBool a, bool value, bool instant)
		{
			if (instant) a.value = value;
			else a.target        = value;
		}

		public override void OnInspectorGUI()
		{
			SetAnimBools(false);

			serializedObject.Update();
			// Once we have a reliable way to know if the object changed, only re-cache in that case.
			DrawScrollRectBase();

			DrawLayoutDataFields();
			DrawInfiniteSettingFields();
			DrawSnapSettingFields();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawScrollRectBase()
		{
			EditorGUILayout.PropertyField(_prefab);
			EditorGUILayout.PropertyField(_scrollType);
			EditorGUILayout.PropertyField(_content);
			EditorGUILayout.PropertyField(_scrollDirection);

			EditorGUILayout.PropertyField(_movementType);
			if (EditorGUILayout.BeginFadeGroup(_showElasticity.faded))
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_elasticity);
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.PropertyField(_inertia);
			if (EditorGUILayout.BeginFadeGroup(_showDecelerationRate.faded))
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_decelerationRate);
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.PropertyField(_scrollSensitivity);
			EditorGUILayout.PropertyField(_maxVelocity);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_viewport);
			DrawScrollBarField();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_scrollPosition);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_onValueChanged);
		}

		private void DrawScrollBarField()
		{
			if ((ScrollDirectionEnum)_scrollDirection.enumValueIndex == ScrollDirectionEnum.HORIZONTAL)
			{
				EditorGUILayout.PropertyField(_horizontalScrollbar);
				if (!_horizontalScrollbar.objectReferenceValue || _horizontalScrollbar.hasMultipleDifferentValues) return;

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_scrollbarVisibility);
				EditorGUI.indentLevel--;
			}
			else
			{
				EditorGUILayout.PropertyField(_verticalScrollbar);
				if (!_verticalScrollbar.objectReferenceValue || _verticalScrollbar.hasMultipleDifferentValues) return;

				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_scrollbarVisibility);
				EditorGUI.indentLevel--;
			}
		}

		private void DrawLayoutDataFields()
		{
			var prevValue = _isShowLayoutData;
			_isShowLayoutData = EditorGUILayout.Toggle("Show Layout Data", _isShowLayoutData);

			if (prevValue != _isShowLayoutData)
				EditorPrefs.SetBool(LAYOUT_DATA_KEY, _isShowLayoutData);

			if (!_isShowLayoutData) return;

			EditorGUI.indentLevel += 1;
			EditorGUILayout.PropertyField(_spacing);
			EditorGUILayout.PropertyField(_padding);
			EditorGUILayout.Space();
			EditorGUI.indentLevel -= 1;
		}

		private void DrawInfiniteSettingFields()
		{
			var prevValue = _isShowInfiniteSetting;
			_isShowInfiniteSetting = EditorGUILayout.Toggle("Show Infinite Scroll Setting", _isShowInfiniteSetting);

			if (prevValue != _isShowInfiniteSetting)
				EditorPrefs.SetBool(INFINITE_SETTING_KEY, _isShowInfiniteSetting);

			if (!_isShowInfiniteSetting) return;

			EditorGUI.indentLevel += 1;
			EditorGUILayout.PropertyField(_loop);
			EditorGUILayout.PropertyField(_loopWhileDragging);
			EditorGUILayout.Space();
			EditorGUI.indentLevel -= 1;
		}

		private void DrawSnapSettingFields()
		{
			var prevValue = _isShowSnapSetting;
			_isShowSnapSetting = EditorGUILayout.Toggle("Show Snap Setting", _isShowSnapSetting);

			if (prevValue != _isShowSnapSetting)
				EditorPrefs.SetBool(SNAP_SETTING_KEY, _isShowSnapSetting);

			if (!_isShowSnapSetting) return;

			EditorGUI.indentLevel += 1;
			EditorGUILayout.PropertyField(_snapping);
			EditorGUILayout.PropertyField(_snapVelocityThreshold);
			EditorGUILayout.PropertyField(_snapWatchOffset);
			EditorGUILayout.PropertyField(_snapJumpToOffset);
			EditorGUILayout.PropertyField(_snapCellCenterOffset);
			EditorGUILayout.PropertyField(_snapUseCellSpacing);
			EditorGUILayout.PropertyField(_snapTweenType);
			EditorGUILayout.PropertyField(_snapTweenTime);
			EditorGUILayout.PropertyField(_snapWhileDragging);
			EditorGUILayout.Space();
			EditorGUI.indentLevel -= 1;
		}
	}
}
