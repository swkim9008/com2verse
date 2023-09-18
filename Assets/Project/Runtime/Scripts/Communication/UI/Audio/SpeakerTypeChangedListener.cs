/*===============================================================
* Product:		Com2Verse
* File Name:	SpeakerTypeChangedListener.cs
* Developer:	urun4m0r1
* Date:			2022-06-14 11:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Communication;
using Com2Verse.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.Project.Communication.UI
{
	/// <summary>
	/// 화자 상태를 ViewModel 또는 코드 레벨에서 제어해, Filter 조건과 일치하는 경우 UnityEvent 를 발생시키는 클래스.<br/>
	/// 아이콘 표시 등의 UI 제어에 사용할 수 있습니다.
	/// </summary>
	[AddComponentMenu("[Communication]/[Communication] Speaker Type Changed Listener")]
	public sealed class SpeakerTypeChangedListener : MonoBehaviour
	{
#region InspectorFields
		[Header("Filter")]
		[SerializeField] private eFlagMatchType _matchType = eFlagMatchType.EQUALS;
		[SerializeField] private eSpeakerType _filter = eSpeakerType.NONE;

		[Header("Debug Info")]
		[SerializeField, ReadOnly] private bool _isFilterMatch;
		[SerializeField, ReadOnly] private eSpeakerType _type = eSpeakerType.NONE;

		[Header("Events")]
		[SerializeField] private UnityEvent<bool>? _onSpeakerTypeMatch;
		[SerializeField] private UnityEvent? _onSpeakerTypeTrue;
		[SerializeField] private UnityEvent? _onSpeakerTypeFalse;
#endregion // InspectorFields

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public eSpeakerType Type
		{
			get => _type;
			set
			{
				_type = value;
				OnSpeakerTypeChanged();
			}
		}

		/// <summary>
		/// DataBinder 호환성을 위해 int 랩핑 버전 제공 필요
		/// </summary>
		[UsedImplicitly] // Setter used by view model.
		public int TypeInt
		{
			get => Type.CastInt();
			set => Type = value.CastEnum<eSpeakerType>();
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			_isFilterMatch = false;

			_type = eSpeakerType.NONE;

			InvokeFilterMatchEvent();
		}
#endregion // ViewModelProperties

		private void OnEnable()
		{
			OnSpeakerTypeChanged(true);
		}

		private void OnSpeakerTypeChanged(bool forceInvokeEvent = false)
		{
			var isFilterMatch = Type.IsFilterMatch(_filter, _matchType);
			if (_isFilterMatch != isFilterMatch || forceInvokeEvent)
			{
				_isFilterMatch = isFilterMatch;
				InvokeFilterMatchEvent();
			}
		}

		private void InvokeFilterMatchEvent()
		{
			_onSpeakerTypeMatch?.Invoke(_isFilterMatch);

			if (_isFilterMatch)
				_onSpeakerTypeTrue?.Invoke();
			else
				_onSpeakerTypeFalse?.Invoke();
		}
	}
}
