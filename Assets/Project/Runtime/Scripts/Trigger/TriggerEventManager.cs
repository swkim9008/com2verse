// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TriggerEventManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-22 오전 10:12
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
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.UI;
using Protocols.GameMechanic;
using UnityEngine;

namespace Com2Verse.EventTrigger
{
	public class TriggerEventManager : Singleton<TriggerEventManager>, ICollisionEventSubscriber
	{
		private TriggerEventManager() { }
		private Dictionary<C2VEventTrigger, List<int>> _triggerSwitch = new Dictionary<C2VEventTrigger, List<int>>();
		private HashSet<ServerZone> _zoneSwitch = new HashSet<ServerZone>();
		public Action<ServerZone> OnZoneAction = null;

		public void Initialize()
		{
			SceneManager.Instance.BeforeSceneChanged += OnBeforeSceneChanged;

			CollisionEventManager.Subscriber = this;
			Network.GameMechanic.PacketReceiver.Instance.CheckCollisionRequest += OnServerResponse;
		}

		private void OnBeforeSceneChanged(SceneBase currentScene, SceneBase newScene)
		{
			Reset();
		}

		public void Reset()
		{
			ExitTriggers();
			ExitZones();
		}

		private void ExitTriggers()
		{
			foreach (var triggerMap in _triggerSwitch)
			{
				foreach (var callbackIndex in triggerMap.Value)
				{
					NetworkUIManager.Instance.OnTriggerExit(new TriggerOutEventParameter()
					{
						SourcePacket = null,
						SourceTrigger = triggerMap.Key,
						CallbackIndex = callbackIndex,
						TriggerIndex = GetTriggerIndex(triggerMap.Key)
					});
				}
			}
			
			_triggerSwitch.Clear();
		}

		private void ExitZones()
		{
			foreach (var zone in _zoneSwitch)
			{
				ZoneManager.Instance.OnZoneExit(zone);
			}
			
			_zoneSwitch.Clear();
		}

		public void OnServerResponse(CheckCollisionRequest response)
		{
			// do nothing
		}

		/// <summary>
		/// 현재 트리거 안에 유저(Me)가 있는지(stay) 확인하는 함수 
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="triggerIndex"></param>
		/// <returns>트리거 안에 있으면 true 아니면 false</returns>
		public bool IsInTrigger(C2VEventTrigger trigger, int triggerIndex)
		{
			if (_triggerSwitch.TryGetValue(trigger, out List<int> indexList))
			{
				if (indexList.Contains(triggerIndex))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsInZone(ServerZone zone)
		{
			return _zoneSwitch.Contains(zone);
		}

		private int GetTriggerIndex(C2VEventTrigger trigger)
		{
			int triggerIndex = -1;
			Transform triggerTransform = trigger.transform;
			Transform parent = triggerTransform.parent;
			int childCount = parent.childCount;
			for (int index = 0; index < childCount; index++)
			{
				if (ReferenceEquals(parent.GetChild(index), triggerTransform))
				{
					triggerIndex = index;
					break;
				}
			}

			return triggerIndex;
		}

		public void OnClick(C2VEventTrigger trigger)
		{
			for (int callbackIndex = 0; callbackIndex < trigger.Callback.Length; callbackIndex++)
			{
				var triggerCallback = trigger.Callback[callbackIndex];
				eLogicType function = (eLogicType)triggerCallback.Function;
				var interaction = InteractionManager.Instance.GetActionType(function);

				if (interaction == null || interaction.ActionType != eActionType.CLICK_ACTION) continue;

				// UnHandled trigger event
				if (triggerCallback.Function == -1) continue;

				NetworkUIManager.Instance.OnTriggerClick(new TriggerEventParameter()
				{
					SourceTrigger = trigger,
					CallbackIndex = callbackIndex,
					TriggerIndex = GetTriggerIndex(trigger)
				});
			}
		}

		private bool CheckIsMe(Collider target)
		{
			BaseMapObject collideTarget = target.GetComponent<BaseMapObject>();
			if (collideTarget.IsReferenceNull() || !collideTarget.IsMine) return false;

			return true;
		}

		public void OnZone(int collisionEventType, ServerZone zone, Collider target)
		{
			if (!CheckIsMe(target)) return;
			
			var type = (OnCollisionEventType)collisionEventType;
			if (type == OnCollisionEventType.OnEnter)
			{
				if (_zoneSwitch.TryAdd(zone))
				{
					ZoneManager.Instance.OnZoneEnter(zone);
					OnZoneAction?.Invoke(zone);
				}
			}
			else if (type == OnCollisionEventType.OnExit)
			{
				if (_zoneSwitch.TryRemove(zone))
				{
					ZoneManager.Instance.OnZoneExit(zone);
				}
			}
		}

		public void OnTrigger(int collisionEventType, C2VEventTrigger trigger, Collider target)
		{
			if (!CheckIsMe(target)) return;
			
			for (int callbackIndex = 0; callbackIndex < trigger.Callback.Length; callbackIndex++)
			{
				var triggerCallback = trigger.Callback[callbackIndex];
				eLogicType function = (eLogicType)triggerCallback.Function;
				var interaction = InteractionManager.Instance.GetActionType(function);

				if (interaction == null || interaction.ActionType == eActionType.CLICK_ACTION) continue;
				
				var type = (OnCollisionEventType)collisionEventType;
				int triggerIndex = GetTriggerIndex(trigger);

				// UnHandled trigger event
				if (triggerCallback.Function == -1) continue;
				
				if (type == OnCollisionEventType.OnEnter)
				{
					// trigger enter validation
					if (!_triggerSwitch.TryGetValue(trigger, out var callbackList))
					{
						callbackList = new List<int>();
						_triggerSwitch.Add(trigger, callbackList);
					}
					
					if (callbackList.TryAdd(callbackIndex))
					{
						NetworkUIManager.Instance.OnTriggerEnter(new TriggerInEventParameter()
						{
							SourcePacket = null,
							SourceTrigger = trigger,
							CallbackIndex = callbackIndex,
							TriggerIndex = triggerIndex
						});
					}
				}
				else if (type == OnCollisionEventType.OnExit)
				{
					if (!_triggerSwitch.TryGetValue(trigger, out var callbackList))
					{
						callbackList = new List<int>();
						_triggerSwitch.Add(trigger, callbackList);
					}
					
					if (callbackList.TryRemove(callbackIndex))
					{
						NetworkUIManager.Instance.OnTriggerExit(new TriggerOutEventParameter()
						{
							SourcePacket = null,
							SourceTrigger = trigger,
							CallbackIndex = callbackIndex,
							TriggerIndex = triggerIndex
						});
					}
				}
			}
		}
	}
}
