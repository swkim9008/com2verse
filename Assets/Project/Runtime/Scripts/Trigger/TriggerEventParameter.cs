// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TriggerEventParameter.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-19 오후 4:27
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Network;
using Com2Verse.PhysicsAssetSerialization;
using Protocols.GameLogic;

namespace Com2Verse.EventTrigger
{
	public class TriggerEventParameter
	{
		public C2VEventTrigger SourceTrigger { get; set; }
		public int CallbackIndex { get; set; }
		public int TriggerIndex { get; set; }

		public virtual MapObject ParentMapObject
		{
			get
			{
				if (SourceTrigger != null)
				{
					// (TODO) World는 hierarchy 상위에 MapObject 컴포넌트가 없음
					return SourceTrigger.transform.GetComponentInParent<MapObject>();
				}

				return null;
			}
		}
	}

	public class TriggerInEventParameter : TriggerEventParameter
	{
		public StandInTriggerNotify SourcePacket { get; set; }
		
		public override MapObject ParentMapObject
		{
			get
			{
				if (SourcePacket != null)
				{
					return MapController.Instance.GetStaticObjectByID(SourcePacket.ObjectId) as MapObject;
				}
				else
				{
					return base.ParentMapObject;
				}
			}
		}
	}

	public class TriggerOutEventParameter : TriggerEventParameter
	{
		public GetOffTriggerNotify SourcePacket { get; set; }

		public override MapObject ParentMapObject
		{
			get
			{
				if (SourcePacket != null)
				{
					return MapController.Instance.GetStaticObjectByID(SourcePacket.ObjectId) as MapObject;
				}
				else
				{
					return base.ParentMapObject;
				}
			}
		}
	}
}
