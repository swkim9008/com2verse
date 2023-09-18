/*===============================================================
* Product:		Com2Verse
* File Name:	AudioStateChangedListener.cs
* Developer:	urun4m0r1
* Date:			2022-06-14 11:53
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.Project.Communication.UI
{
	/// <summary>
	/// 오디오 상태를 ViewModel 또는 코드 레벨에서 제어해, Filter 조건과 일치하는 경우 UnityEvent 를 발생시키는 클래스.<br/>
	/// 아이콘 표시 등의 UI 제어에 사용할 수 있습니다.
	/// </summary>
	[AddComponentMenu("[Communication]/[Communication] Audio State Changed Listener")]
	public sealed class AudioStateChangedListener : MonoBehaviour
	{
#region InspectorFields
		[Header("Filter")]
		[SerializeField] private eBooleanFilterType _isInputAudibleFilter;
		[SerializeField] private eBooleanFilterType _isOutputAudibleFilter;
		[SerializeField] private eBooleanFilterType _isSpeakingFilter;

		[Header("Debug Info")]
		[SerializeField, ReadOnly] private bool _isFilterMatch;
		[SerializeField, ReadOnly] private bool _isInputAudible;
		[SerializeField, ReadOnly] private bool _isOutputAudible;
		[SerializeField, ReadOnly] private bool _isSpeaking;

		[Header("Events")]
		[SerializeField] private UnityEvent<bool>? _onAudioStateMatch;
		[SerializeField] private UnityEvent? _onAudioStateTrue;
		[SerializeField] private UnityEvent? _onAudioStateFalse;
#endregion // InspectorFields

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public bool IsInputAudible
		{
			get => _isInputAudible;
			set
			{
				_isInputAudible = value;
				OnAudioStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public bool IsOutputAudible
		{
			get => _isOutputAudible;
			set
			{
				_isOutputAudible = value;
				OnAudioStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public bool IsSpeaking
		{
			get => _isSpeaking;
			set
			{
				_isSpeaking = value;
				OnAudioStateChanged();
			}
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			_isFilterMatch = false;

			_isInputAudible  = false;
			_isOutputAudible = false;
			_isSpeaking      = false;

			InvokeFilterMatchEvent();
		}
#endregion // ViewModelProperties

		private void OnEnable()
		{
			OnAudioStateChanged(true);
		}

		private void OnAudioStateChanged(bool forceInvokeEvent = false)
		{
			var isFilterMatch = IsFilterMatch(IsInputAudible, IsOutputAudible, IsSpeaking);
			if (_isFilterMatch != isFilterMatch || forceInvokeEvent)
			{
				_isFilterMatch = isFilterMatch;
				InvokeFilterMatchEvent();
			}
		}

		private bool IsFilterMatch(bool isInputAudible, bool isOutputAudible, bool isSpeaking)
		{
			var isInputAudibleMatch  = BooleanFilter.Apply(isInputAudible,  _isInputAudibleFilter);
			var isOutputAudibleMatch = BooleanFilter.Apply(isOutputAudible, _isOutputAudibleFilter);
			var isSpeakingMatch      = BooleanFilter.Apply(isSpeaking,      _isSpeakingFilter);

			return isInputAudibleMatch && isOutputAudibleMatch && isSpeakingMatch;
		}

		private void InvokeFilterMatchEvent()
		{
			_onAudioStateMatch?.Invoke(_isFilterMatch);

			if (_isFilterMatch)
				_onAudioStateTrue?.Invoke();
			else
				_onAudioStateFalse?.Invoke();
		}
	}
}
