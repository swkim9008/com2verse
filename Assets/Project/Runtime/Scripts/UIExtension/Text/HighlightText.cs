/*===============================================================
* Product:		Com2Verse
* File Name:	HighlightText.cs
* Developer:	jhkim
* Date:			2022-09-16 14:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Text.RegularExpressions;
using Com2Verse.Extension;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public sealed class HighlightText : MonoBehaviour
	{
		[SerializeField] private Color _highlightColor = new(0f, 0.5882353f, 0.9843137f, 1f);
		[SerializeField] private bool _ignoreCase = true;

		private TextMeshProUGUI _text;
		private static readonly string ColorTagPattern = "<color=.+>(.*)</color>";
		private static readonly string ColorTagFormat = "<color=#{0}>{1}</color>";
		private string _original;
		private string _highlight;
		private void Start()
		{
			GetTextComponent();
		}


		public string Text
		{
			get => GetTextComponent().text ?? string.Empty;
			set
			{
				GetTextComponent().text = value;
				SetHighlightText();
			}
		}
		public string Highlight
		{
			get => _highlight;
			set
			{
				_highlight = value;
				SetHighlightText();
			}
		}
		public void Clear() => Highlight = string.Empty;

		private TextMeshProUGUI GetTextComponent()
		{
			if (_text.IsReferenceNull())
				_text = GetComponent<TextMeshProUGUI>();
			return _text;
		}

		private void SetHighlightText()
		{
			if (string.IsNullOrWhiteSpace(_highlight)) return;
			if (GetTextComponent().IsReferenceNull()) return;

			_highlight = _highlight.Replace(" ", string.Empty);
			if (string.IsNullOrWhiteSpace(_highlight)) return;

			var plainText = ClearHighlightText();

			var coloredText = string.Format(ColorTagFormat, ColorUtility.ToHtmlStringRGB(_highlightColor), _highlight);
			if (_ignoreCase)
			{
				if (_highlight.ToUpper() == _highlight && _highlight.ToLower() == _highlight)
					_text.text = plainText.Replace(_highlight, coloredText);
				else
				{
					var text = plainText;
					text = ReplaceToColoredText(text, _highlight.ToUpper(), GetColoredText(_highlight.ToUpper())); // ToUpper 먼저 실행할것!! 순서 변경시 버그 발생
					text = ReplaceToColoredText(text, _highlight.ToLower(), GetColoredText(_highlight.ToLower()));
					_text.text = text;
				}
			}
			else
				_text.text = plainText.Replace(_highlight, coloredText);

			string GetColoredText(string highlight) => string.Format(ColorTagFormat, ColorUtility.ToHtmlStringRGB(_highlightColor), highlight);
			string ReplaceToColoredText(string original, string highlight, string colorText) => original.Replace(highlight, colorText);
		}

		private string ClearHighlightText()
		{
			var plainText = GetPlainText();
			_text.text = plainText;
			return plainText;
		}

		private string GetPlainText()
		{
			var plainText = Regex.Replace(_text.text, ColorTagPattern, "$1");
			return plainText;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(HighlightText))]
	public class HighlightTextEditor : Editor
	{
		private HighlightText _target;
		private string _highlight = string.Empty;
		private void Awake()
		{
			_target = target as HighlightText;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_target.IsReferenceNull() || _target.Text == null)
				return;

			var highlight = EditorGUILayout.TextField("Highlight Text", _highlight);
			if (!highlight.Equals(_highlight))
			{
				_highlight = highlight;
				_target.Highlight = _highlight;
			}
			EditorGUILayout.LabelField(_target.Text);
			if(GUILayout.Button("Clear"))
				_target.Clear();
		}
	}
#endif // UNITY_EDITOR
}
