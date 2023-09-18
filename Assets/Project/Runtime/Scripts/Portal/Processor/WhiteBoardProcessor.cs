/*===============================================================
* Product:		Com2Verse
* File Name:	WhiteBoardProcessor.cs
* Developer:	jhkim
* Date:			2023-05-19 12:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.Office;
using Com2Verse.UI;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.WHITE_BOARD)]
	public sealed class WhiteBoardProcessor : BaseLogicTypeProcessor
	{
		private static readonly int MinWhiteBoardIdLength = 5;

#region Initialize
		public WhiteBoardProcessor()
		{
			MapController.Instance.OnMapObjectRemove += OnObjectRemoved;
		}
#endregion // Initialize

#region Trigger Events
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;

			base.OnTriggerEnter(triggerInParameter);
		}

		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			var boardId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex, 0);
			ShowWhiteBoardEditPopup(boardId);
		}
#endregion // Trigger Events

#region MapObject Event
		private void OnObjectRemoved(BaseMapObject mapObject)
		{
			if (TryGetWhiteBoardViewObject(mapObject, out var whiteBoardViewObject))
				whiteBoardViewObject.Dispose();
		}
#endregion // MapObject Event

		private void ShowWhiteBoardEditPopup(string boardId)
		{
			boardId = RemoveUrl(boardId);
			if (!IsValidBoardId(boardId)) return;

			WhiteBoardWebView.Show(boardId);
		}
		private string RemoveUrl(string boardId)
		{
			if (string.IsNullOrWhiteSpace(boardId)) return boardId;

			var idx = boardId.LastIndexOf("/");

			return idx == -1 ? boardId : boardId.Substring(idx);
		}
		private bool IsValidBoardId(string boardId)
		{
			if (string.IsNullOrWhiteSpace(boardId)) return false;
			if (boardId.Length < MinWhiteBoardIdLength) return false;

			return true;
		}
		private bool TryGetWhiteBoardViewObject(BaseMapObject mapObject, out WhiteBoardViewObject whiteBoardViewObject)
		{
			whiteBoardViewObject = null;
			return !mapObject.IsReferenceNull() && mapObject.TryGetComponent(out whiteBoardViewObject);
		}
	}
}
