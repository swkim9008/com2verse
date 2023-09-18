// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ZoneManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-13 오후 7:14
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.UI;
using Protocols.WorldState;
using UnityEngine;
using PacketReceiver = Com2Verse.Network.WorldState.PacketReceiver;

namespace Com2Verse
{
	public class ZoneManager : Singleton<ZoneManager>, IDisposable
	{
		private ZoneManager() { }

		private Transform _zoneParent;
		private Transform ZoneParent
		{
			get
			{
				if (_zoneParent.IsUnityNull()) _zoneParent = new GameObject("Zone").transform;

				return _zoneParent;
			}
		}

		public void Initialize()
		{
			PacketReceiver.Instance.NearZoneNotify += OnZoneData;
		}
		
		private void OnZoneData(NearZoneNotify zoneNotify)
		{
			int leftZone = zoneNotify.ZoneInfos.Count;
			Transform parent = ZoneParent;

			foreach (Transform child in parent)
			{
				if (child.IsUnityNull()) continue;
				
				if (leftZone > 0)
				{
					// process zone (set scale and position)
					var zoneData = zoneNotify.ZoneInfos[zoneNotify.ZoneInfos.Count - leftZone];
					SetZoneData(zoneData, child);
					
					child.gameObject.SetActive(true);
					leftZone--;	
				}
				else
				{
					child.gameObject.SetActive(false);
				}
			}
			
			while (leftZone > 0)
			{
				var zoneData = zoneNotify.ZoneInfos[zoneNotify.ZoneInfos.Count - leftZone];
				
#if UNITY_EDITOR
				GameObject newZone = new GameObject(zoneData.ZoneName);
#else
				GameObject newZone = new GameObject();
#endif
				SetZoneData(zoneData, newZone.transform);
				newZone.transform.SetParent(parent);
				leftZone--;
			}
		}

		private void SetZoneData(ZoneMessage zoneData, Transform target)
		{
#if UNITY_EDITOR
			target.gameObject.name = zoneData.ZoneName;
#endif
			var zoneComponent = target.gameObject.GetOrAddComponent<ServerZone>();
			zoneComponent.ZoneId = zoneData.ZoneId;
			zoneComponent.ZoneName = zoneData.ZoneName;

			zoneComponent.Callback = new ServerZone.ZoneCallback[zoneData.ZoneInteraction.Count];
			for (int i = 0; i < zoneData.ZoneInteraction.Count; i++)
			{
				var zoneInteraction = zoneData.ZoneInteraction[i];
				zoneComponent.Callback[i] = new ServerZone.ZoneCallback()
				{
					LogicType = (eLogicType)zoneInteraction.InteractionId,
					InteractionValue = new List<string>(1) { zoneInteraction.InteractionValue }
				};
			}

			target.SetLocalPositionAndRotation(new Vector3(zoneData.LocationX * 0.01f, zoneData.LocationY * 0.01f, zoneData.LocationZ * 0.01f),
			                                   Quaternion.Euler(zoneData.RotationX * 0.01f, zoneData.RotationY * 0.01f, zoneData.RotationZ * 0.01f));
			target.localScale = new Vector3(zoneData.SizeX * 0.01f, zoneData.SizeY * 0.01f, zoneData.SizeZ * 0.01f);
		}

		public void OnZoneEnter(ServerZone zone)
		{
			for (int callbackIndex = 0; callbackIndex < zone.Callback.Length; callbackIndex++)
			{
				var callback = zone.Callback[callbackIndex];

				var interaction = InteractionManager.Instance.GetActionType(callback.LogicType);
				if (interaction == null) continue;
				
				if (interaction!.TriggerValidationType == eTriggerValidationType.CLIENT)
					NetworkUIManager.Instance.OnZoneEnter(zone, callbackIndex);
			}

			Commander.Instance.ZoneEnter(zone.ZoneId);
		}

		public void OnZoneExit(ServerZone zone)
		{
			for (int callbackIndex = 0; callbackIndex < zone.Callback.Length; callbackIndex++)
			{
				var callback = zone.Callback[callbackIndex];

				var interaction = InteractionManager.Instance.GetActionType(callback.LogicType);
				if (interaction == null) continue;

				if (interaction!.TriggerValidationType == eTriggerValidationType.CLIENT)
					NetworkUIManager.Instance.OnZoneExit(zone, callbackIndex);
			}

			Commander.Instance.ZoneExit(zone.ZoneId);
		}
		
		public void Dispose()
		{
			if (PacketReceiver.InstanceExists)
				PacketReceiver.Instance.NearZoneNotify -= OnZoneData;
		}
	}
}
