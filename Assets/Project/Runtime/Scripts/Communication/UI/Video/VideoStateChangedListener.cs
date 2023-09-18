/*===============================================================
* Product:		Com2Verse
* File Name:	VideoStateChangedListener.cs
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
	/// 비디오 상태를 ViewModel 또는 코드 레벨에서 제어해, Filter 조건과 일치하는 경우 UnityEvent 를 발생시키는 클래스.<br/>
	/// 아이콘 표시 등의 UI 제어에 사용할 수 있습니다.
	/// </summary>
	[AddComponentMenu("[Communication]/[Communication] Video State Changed Listener")]
	public sealed class VideoStateChangedListener : MonoBehaviour
	{
#region InspectorFields
		[Header("Filter")]
		[SerializeField] public eBooleanFilterType _isInputRunningFilter;
		[SerializeField] public eBooleanFilterType _isOutputRunningFilter;

		[Header("Debug Info")]
		[SerializeField, ReadOnly] private bool _isFilterMatch;
		[SerializeField, ReadOnly] private bool _isInputRunning;
		[SerializeField, ReadOnly] private bool _isOutputRunning;

		[Header("Events")]
		[SerializeField] private UnityEvent<bool>? _onVideoStateMatch;
		[SerializeField] private UnityEvent? _onVideoStateTrue;
		[SerializeField] private UnityEvent? _onVideoStateFalse;
#endregion // InspectorFields

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public bool IsInputRunning
		{
			get => _isInputRunning;
			set
			{
				_isInputRunning = value;
				OnVideoStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public bool IsOutputRunning
		{
			get => _isOutputRunning;
			set
			{
				_isOutputRunning = value;
				OnVideoStateChanged();
			}
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			_isFilterMatch = false;

			_isInputRunning  = false;
			_isOutputRunning = false;

			InvokeFilterMatchEvent();
		}
#endregion // ViewModelProperties

		private void OnEnable()
		{
			OnVideoStateChanged(true);
		}

		private void OnVideoStateChanged(bool forceInvokeEvent = false)
		{
			var isFilterMatch = IsFilterMatch(IsInputRunning, IsOutputRunning);
			if (_isFilterMatch != isFilterMatch || forceInvokeEvent)
			{
				_isFilterMatch = isFilterMatch;
				InvokeFilterMatchEvent();
			}
		}

		private bool IsFilterMatch(bool isInputRunning, bool isOutputRunning)
		{
			var isInputMatch  = BooleanFilter.Apply(isInputRunning,  _isInputRunningFilter);
			var isOutputMatch = BooleanFilter.Apply(isOutputRunning, _isOutputRunningFilter);

			return isInputMatch && isOutputMatch;
		}

		private void InvokeFilterMatchEvent()
		{
			_onVideoStateMatch?.Invoke(_isFilterMatch);

			if (_isFilterMatch)
				_onVideoStateTrue?.Invoke();
			else
				_onVideoStateFalse?.Invoke();
		}
	}
}
