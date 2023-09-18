/*===============================================================
* Product:		Com2Verse
* File Name:	WarpSpaceItemController.cs
* Developer:	jhkim
* Date:			2023-06-30 21:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Office.WarpSpace
{
	public sealed class WarpSpaceItemController
	{
		private WarpSpaceItemModel _model;

		public delegate void OnStateChanged(WarpSpaceItemModel.eState prevState, WarpSpaceItemModel.eState newState);
		private readonly OnStateChanged _onStateChanged;

		public WarpSpaceItemModel.eState State => _model.State;
		public int Pid => _model.Pid;
		public WarpSpaceItemController(WarpSpaceItemModel model, OnStateChanged onStateChanged = null)
		{
			_model = model;
			_onStateChanged = onStateChanged;
		}

		public void SetState(WarpSpaceItemModel.eState state)
		{
			if (state == _model.State) return;

			_model.State = state;
			_onStateChanged?.Invoke(state, _model.State);
		}
	}

#region Model
	/* Work Space (groupId, teamId)
	 * Rest Space (groupId, spaceId)
	 * Lobby Space ()
	 */
	public struct WarpSpaceItemModel
	{
		public enum eType
		{
			WORK,
			REST,
			LOBBY,
		}
		public enum eState
		{
			DESELECTED,
			SELECTED,
			DISABLED,
		}

		public int Pid { get; init; }
		public eType Type { get; init; }
		public eState State;
		public string Label { get; init; }
		public long GroupID { get; init; }
		public long TeamID { get; init; }
		public string SpaceID { get; init; }
	}
#endregion // Model
}
