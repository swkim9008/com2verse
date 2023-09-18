/*===============================================================
* Product:		Com2Verse
* File Name:	InputSystemManagerHelper.cs
* Developer:	eugene9721
* Date:			2023-01-02 14:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Utils;

namespace Com2Verse.Project.InputSystem
{
	public static class InputSystemManagerHelper
	{
		private static eInputSystemState _currentState;

		public static void Initialize()
		{
			_currentState = eInputSystemState.UI;
			var inputActionCollection = new Com2VerseAction();
			InputSystemManager.Instance.Initialize(
				inputActionCollection,
				inputActionCollection.asset,
				Define.LayerMask(Define.eLayer.GROUND),
				Define.LayerMask(Define.eLayer.CHARACTER) | Define.LayerMask(Define.eLayer.OBJECT),
				"ActionMap.json");

			ChangeState(_currentState, true);
		}

		/// <summary>
		/// 프로젝트에서 InputSystemManager의 ActionMap 변환을 쉽게 하기 위한 메서드
		/// </summary>
		/// <param name="nextState">기획데이터에서 정의한 테이블 타입</param>
		/// <param name="forceChange">state값이 같을 경우에도 ChangeActionMap을 수행할지 체크</param>
		public static void ChangeState(eInputSystemState nextState, bool forceChange = false)
		{
			if (!forceChange && _currentState == nextState) return;
			var inputSystemManager = InputSystemManager.InstanceOrNull;
			if (inputSystemManager == null) return;

			_currentState = nextState;
			switch (nextState)
			{
				case eInputSystemState.CHARACTER_CONTROL:
					inputSystemManager.ChangeActionMap<ActionMapCharacterControl>();
					break;
				case eInputSystemState.UI:
					inputSystemManager.ChangeActionMap<ActionMapUIControl>();
					break;
				case eInputSystemState.WEB_VIEW_UI:
					inputSystemManager.ChangeActionMap<ActionMapWebViewUIControl>();
					break;
				case eInputSystemState.WAITING_BLOCK:
					inputSystemManager.ChangeActionMap<ActionMapWaitingBlockControl>();
					break;
			}
			C2VDebug.LogCategory("InputSystemManager", $"Change ActionMap to ({_currentState.ToString()})");
		}
	}
}
