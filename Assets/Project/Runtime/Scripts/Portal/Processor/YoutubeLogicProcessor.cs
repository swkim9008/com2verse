using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.Office;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.YOUTUBE__VIDEO)]
	public sealed class YoutubeLogicProcessor : BaseLogicTypeProcessor
	{
		private static readonly int PlayIdParamIndex = 0;
		public YoutubeLogicProcessor()
		{
			MapController.Instance.OnMapObjectRemove += OnObjectRemoved;
		}

		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;

			base.OnTriggerEnter(triggerInParameter);

			var obj = triggerInParameter.ParentMapObject;
			if (obj.IsUnityNull()) return;

			if (TryGetYoutubeObject(obj, out var youtubeObject))
			{
				var playId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex, PlayIdParamIndex);

				if (string.IsNullOrWhiteSpace(playId)) return;

				if (!youtubeObject.HasController)
					youtubeObject.Create(playId);
				else
					youtubeObject.Resume();
			}
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;

			base.OnTriggerExit(triggerOutParameter);
			var obj = triggerOutParameter.ParentMapObject;

			if (TryGetYoutubeObject(obj, out var youtubeObject))
				youtubeObject.Pause();
		}

		private bool TryGetYoutubeObject(BaseMapObject mapObject, out YoutubeObject youtubeObject)
		{
			youtubeObject = null;
			return !mapObject.IsUnityNull() && mapObject!.TryGetComponent(out youtubeObject);
		}

		private void OnObjectRemoved(BaseMapObject mapObject)
		{
			if (TryGetYoutubeObject(mapObject, out var youtubeObject))
				youtubeObject.Dispose();
		}
	}
}
