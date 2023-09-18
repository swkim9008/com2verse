/*===============================================================
* Product:		Com2Verse
* File Name:	ToggleColorSwapExtensions.cs
* Developer:	jhkim
* Date:			2022-10-26 17:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse
{
	[AddComponentMenu("[DB]/[DB] ToggleColorSwapExtensions")]
	public sealed class ToggleColorSwapExtensions : MonoBehaviour
	{
		[SerializeField] private ColorBlock _onColor;
		[SerializeField] private ColorBlock _offColor;
		private MetaverseToggle _toggle;
		private TogglePropertyExtensions _togglePropertyExtensions;

#region Properties
		public ColorBlock OnColor
		{
			get => _onColor;
			set => _onColor = value;
		}

		public ColorBlock OffColor
		{
			get => _offColor;
			set => _offColor = value;
		}

		public MetaverseToggle Toggle => GetComponent();
#endregion // Properties

		private void Awake()
		{
			// Initialize
			GetComponent();
			InitToggleExtensions();
		}

#region Public Methods
		public void SetColorFromToggle()
		{
			var colors = GetComponent().colors;
			_onColor = colors;
			_offColor = GetInversedColorBlock(colors);
			Save();
		}

		public void SetColorFromToggleFlat()
		{
			SetColorFromToggle();
			_onColor = Flatten(_onColor);
			_offColor = Flatten(_offColor);
			Save();
		}

		public void Swap()
		{
			(_onColor, _offColor) = (_offColor, _onColor);
			Save();
		}

		public void SetToggleColor()
		{
			var toggle = GetComponent();
			toggle.colors = toggle.isOn ? _onColor : _offColor;
			Save();
		}
#endregion // Public Methods
		private ColorBlock GetInversedColorBlock(ColorBlock colorBlock)
		{
			var inversedColor = colorBlock;
			inversedColor.normalColor = colorBlock.pressedColor;
			inversedColor.pressedColor = colorBlock.normalColor;
			return inversedColor;
		}

		private ColorBlock Flatten(ColorBlock colorBlock)
		{
			var flatten = colorBlock;
			flatten.highlightedColor = flatten.normalColor;
			flatten.pressedColor = flatten.normalColor;
			return flatten;
		}
		private void SetColorBlock(bool isOn) => GetComponent().colors = isOn ? _onColor : _offColor;

#region Toggle
		private MetaverseToggle GetComponent()
		{
			if (_toggle.IsReferenceNull())
			{
				_toggle = GetComponent<MetaverseToggle>();
				_toggle.onValueChanged.RemoveListener(OnValueChanged);
				_toggle.onValueChanged.AddListener(OnValueChanged);
			}

			return _toggle;
		}
		private void OnValueChanged(bool isOn) => SetColorBlock(isOn);
#endregion // Toggle

#region Toggle Extensions
		private void InitToggleExtensions()
		{
			if (TryGetComponent(out _togglePropertyExtensions))
			{
				_togglePropertyExtensions.SetOnValueWithoutNotify(OnValueWithoutNotifyChanged);
				if (_togglePropertyExtensions is TogglePropertyEnumExtensions togglePropertyEnumExtensions)
					togglePropertyEnumExtensions.SetOnValueEnumEqualWithoutNotify(OnValueWithoutNotifyChanged);
			}
		}

		private void OnValueWithoutNotifyChanged(bool isOn) => SetColorBlock(isOn);
#endregion // Toggle Extensions

		private void Save()
		{
#if UNITY_EDITOR
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssetIfDirty(this);
#endif // UNITY_EDITOR
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ToggleColorSwapExtensions))]
	public sealed class ToggleColorSwapExtensionsEditor : Editor
	{
		private ToggleColorSwapExtensions _extensions;
		private bool _foldOutOnColor = true;
		private bool _foldOutOffColor = true;
		private void Awake()
		{
			_extensions = target as ToggleColorSwapExtensions;
		}

		public override void OnInspectorGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Set Color From Toggle"))
					_extensions.SetColorFromToggle();
				if(GUILayout.Button("Set Color From Toggle (Flatten)"))
					_extensions.SetColorFromToggleFlat();
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Swap"))
					_extensions.Swap();
				if (GUILayout.Button("Set Toggle Color"))
				{
					if (EditorUtility.DisplayDialog("경고", "기존 색상이 변경됩니다. 계속 하시겠습니까?", "예"))
					{
						if (!_extensions.IsReferenceNull())
							Undo.RecordObject(_extensions.Toggle, "Toggle");
						_extensions.SetToggleColor();
					}
				}
			}

			_extensions.OnColor = DrawColorBlock("On Color", ref _foldOutOnColor, _extensions.OnColor);
			_extensions.OffColor = DrawColorBlock("Off Color", ref _foldOutOffColor, _extensions.OffColor);
		}

		private ColorBlock DrawColorBlock(string label, ref bool foldOut, ColorBlock colorBlock)
		{
			foldOut = EditorGUILayout.Foldout(foldOut, label);
			if (foldOut)
			{
				using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box)))
				{
					colorBlock.normalColor = EditorGUILayout.ColorField(nameof(colorBlock.normalColor), colorBlock.normalColor);
					colorBlock.highlightedColor = EditorGUILayout.ColorField(nameof(colorBlock.highlightedColor), colorBlock.highlightedColor);
					colorBlock.pressedColor = EditorGUILayout.ColorField(nameof(colorBlock.pressedColor), colorBlock.pressedColor);
					colorBlock.selectedColor = EditorGUILayout.ColorField(nameof(colorBlock.selectedColor), colorBlock.selectedColor);
					colorBlock.disabledColor = EditorGUILayout.ColorField(nameof(colorBlock.disabledColor), colorBlock.disabledColor);
					colorBlock.colorMultiplier = EditorGUILayout.FloatField(nameof(colorBlock.colorMultiplier), colorBlock.colorMultiplier);
					colorBlock.fadeDuration = EditorGUILayout.FloatField(nameof(colorBlock.fadeDuration), colorBlock.fadeDuration);
				}
			}

			return colorBlock;
		}
	}
#endif // UNITY_EDITOR
}
