/*===============================================================
* Product:		Com2Verse
* File Name:	LocalizationUI.cs
* Developer:	haminjeong
* Date:			2022-07-08 11:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	/// <summary>
	/// 에셋번들 다운로드 후 사용가능
	/// </summary>
	public sealed class LocalizationUI : MonoBehaviour, ILocalizationUI
	{
		public string TextKey;
		private TMP_Text _targetText;
		private float _startTime;
		private static readonly float DelayForInitialize = 2f;

#region Mono
		private void Start()
		{
			(this as ILocalizationUI).InitializeLocalization();

			OnLanguageChanged();
		}
#endregion // Mono
	#region Parameter Properties
		private string _parameter0;
		
		public string Parameter0
		{
			get => _parameter0;
			set
			{
				_parameter0 = value;
				if (_targetText.IsUnityNull()) return;
				_targetText.text = Localization.Instance.GetString(TextKey, _parameter0);
			}
		}
	#endregion Parameter Properties

		private void OnDestroy()
		{
			(this as ILocalizationUI).ReleaseLocalization();
		}

		public void OnLanguageChanged()
		{
			if (!TryGetComponent(out _targetText)) return;

			_targetText.text = Localization.Instance.GetString(TextKey, _parameter0);
		}

#if UNITY_EDITOR
		public void SetText(string text)
		{
			if (_targetText.IsReferenceNull())
			{
				_targetText = GetComponent<TMP_Text>();
			}

			_targetText.text = text;
		}
#endif
	}
}
