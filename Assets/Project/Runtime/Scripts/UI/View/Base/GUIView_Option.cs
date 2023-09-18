/*===============================================================
* Product:		Com2Verse
* File Name:	GUIView_Option.cs
* Developer:	tlghks1009
* Date:			2022-08-19 14:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	public abstract partial class GUIView : MonoBehaviour
	{
		[Header("Optional")]
		[Tooltip("중복 생성 허용 설정")] [SerializeField] private bool _allowDuplicate = false;
		[Tooltip("항상 Binding 되는 설정")] [SerializeField] private bool _alwaysBinded = false;
		[Tooltip("파괴되지 않는 View")] [SerializeField]     private bool _dontDestroyOnLoad;
		[Tooltip("SystemView 체크")] [SerializeField]    private bool _isSystemView;
		[Tooltip("DimmedPopup 설정")] [SerializeField]   private bool _needDimmedPopup = false;
		[Tooltip("Focus 이벤트 사용 유무")] [SerializeField]  private bool _useFocusEvent   = true;

		public bool WillChangeInputSystem {get; set;} = true;

		public     bool IsSystemView      => _isSystemView;
		public new bool DontDestroyOnLoad => _dontDestroyOnLoad;
		public     bool AllowDuplicate
		{
			get => _allowDuplicate;
			set => _allowDuplicate = value;
		}

		public bool NeedDimmedPopup
		{
			get => _needDimmedPopup;
			set => _needDimmedPopup = value;
		}

		public bool UseFocusEvent
		{
			get => _useFocusEvent;
			set => _useFocusEvent = value;
		}
	}
}
