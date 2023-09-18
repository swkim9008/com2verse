// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ServerZone.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-01 오전 10:43
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using UnityEngine;

namespace Com2Verse
{
	public class ServerZone : MonoBehaviour
	{
		[Serializable]
		public class ZoneCallback
		{
			public eLogicType LogicType;
			public List<string> InteractionValue;
		}

		public string ZoneName;
		public long SpaceZoneId;
		public ZoneCallback[] Callback;
		
		public long ZoneId { get; set; }

		private void Start()
		{
			var boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = Vector3.one;
			boxCollider.isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			TriggerEventManager.Instance.OnZone(0, this, other);
		}

		private void OnTriggerExit(Collider other)
		{
			TriggerEventManager.Instance.OnZone(1, this, other);
		}
		
#if UNITY_EDITOR
		[NonSerialized] private Vector3[] _boxPoints = new Vector3[8];
		private void OnDrawGizmosSelected()
		{
			Vector3 worldCenter = transform.TransformPoint(Vector3.zero);
			
			for (int i = 0; i < 8; ++i)
			{
				Vector3 dirVector = new Vector3((i & 4) == 0 ? 1 : -1, (i & 2) == 0 ? 1 : -1, (i & 1) == 0 ? 1 : -1) * 0.5f;
				_boxPoints[i] = worldCenter + transform.TransformVector(Vector3.Scale(dirVector, Vector3.one));
			}

			UnityEditor.Handles.color = Color.yellow;
			for (int i = 0; i < 8; ++i)
			{
				int invertX = (i & (7 - 4)) + (4 - (i & 4));
				int invertY = (i & (7 - 2)) + (2 - (i & 2));
				int invertZ = (i & (7 - 1)) + (1 - (i & 1));
				UnityEditor.Handles.DrawLine(_boxPoints[i], _boxPoints[invertX]);
				UnityEditor.Handles.DrawLine(_boxPoints[i], _boxPoints[invertY]);
				UnityEditor.Handles.DrawLine(_boxPoints[i], _boxPoints[invertZ]);
			}
		}
#endif
	}
}
