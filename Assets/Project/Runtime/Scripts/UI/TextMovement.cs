/*===============================================================
* Product:		Com2Verse
* File Name:	TextMovement.cs
* Developer:	ksw
* Date:			2023-05-23 16:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;
using Com2Verse.UI;
using TMPro;

namespace Com2Verse
{
	[RequireComponent(typeof(RectTransform))]
	public sealed class TextMovement : MonoBehaviour
	{
		private          float _speed           = 100f;
		// 애니메이션 플레이 시간 때문에 앞 뒤 길이 보정
		private readonly float _correctionValue = 30f;

		private RectTransform _textRectTransform;
		private TMP_Text      _text;

		private bool _isLoop;
		private bool _isPlay;

		private float _bgWidth;
		private float _textWidth;
		private float _endPos;

		public float Speed
		{
			get => _speed;
			set => _speed = value;
		}

		public bool IsLoop
		{
			get => _isLoop;
			set => _isLoop = value;
		}

		public bool IsPlay
		{
			get => _isPlay;
			set
			{
				if (value)
					SetTextPosition();
				_isPlay = value;
			}
		}

		private void Awake()
		{
			_text          = GetComponentInChildren<TMP_Text>();
			_textRectTransform = _text.gameObject.GetComponent<RectTransform>();
			_bgWidth       = GetComponent<RectTransform>()!.rect.width;
			var textRectTransformExtensions = _textRectTransform.gameObject.GetComponent<RectTransformPropertyExtensions>();
			if (textRectTransformExtensions == null)
			{
				C2VDebug.LogError("System Announcement RectTransformExtensions is null!");
				return;
			}
			textRectTransformExtensions._onChangeWidth.AddListener(ChangeWidth);
		}

		private void Update()
		{
			if (!_isPlay)
				return;

			_textRectTransform.anchoredPosition -= new Vector2(_speed * Time.deltaTime, 0f);
			if (_textRectTransform.anchoredPosition.x < _endPos)
			{
				if (_isLoop)
				{
					SetTextPosition();
				}
				else
				{
					UIManager.Instance.HideAnnouncement();
				}
			}
		}

		private void SetTextPosition()
		{
			var rectTransformAnchoredPosition = _textRectTransform.anchoredPosition;
			rectTransformAnchoredPosition.x     = (_bgWidth / 2) + (_textWidth / 2) + _correctionValue;
			_textRectTransform.anchoredPosition = rectTransformAnchoredPosition;
			_endPos                             = -rectTransformAnchoredPosition.x;
		}

		private void ChangeWidth(float width)
		{
			_textWidth = width;
			SetTextPosition();
		}
	}
}
