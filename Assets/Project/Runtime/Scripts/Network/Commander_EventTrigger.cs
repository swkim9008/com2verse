using Com2Verse.Logger;
using Com2Verse.UI;
using Protocols.GameMechanic;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
		public void ChairCommand(long chairObjectId, long userId)
		{
			Protocols.CommonLogic.UseChairRequest chairCommand = new()
			{
				TargetChairId = chairObjectId,
				UserId        = userId
			};
			C2VDebug.Log($"Sending ChairCommand. TargetChairID : {chairObjectId}, UserID : {userId}");
			NetworkManager.Instance.Send(chairCommand, Protocols.CommonLogic.MessageTypes.UseChairRequest);
		}

		public void ChairDisuseCommand(long chairObjectId, long userId)
		{
			Protocols.CommonLogic.DisuseChairRequest chairCommand = new()
			{
				TargetChairId = chairObjectId,
				UserId        = userId
			};
			C2VDebug.Log($"Sending DisuseChairCommand. TargetChairID : {chairObjectId}, UserID : {userId}");
			NetworkManager.Instance.Send(chairCommand, Protocols.CommonLogic.MessageTypes.DisuseChairRequest);
		}

		public void RequestTrigger(OnCollisionEventType type, long source, long target, int triggerId)
		{
			CheckCollisionRequest collisionRequest = new CheckCollisionRequest()
			{
				CollisionEventType = type,
				SourceObjectId = source,
				TargetObjectId = target,
				EventColliderId = triggerId
			};
			C2VDebug.Log($"Sending RequestTrigger. OnCollisionEventType : {type}, source : {source}, target : {target}, triggerId : {triggerId}");
			NetworkManager.Instance.Send(collisionRequest, Protocols.GameMechanic.MessageTypes.CheckCollisionRequest);
		}
	}
}
