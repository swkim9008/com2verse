/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEffectManager.cs
* Developer:	wlemon
* Date:			2023-07-19 16:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Network;

namespace Com2Verse.Mice
{
	public sealed class MiceEffectManager : MonoBehaviour
	{
		public static MiceEffectManager Instance { get; private set; } = null;

		public class EffectInstance
		{
			public enum eState
			{
				PLAYING,
				STOPPED,
				PAUSED,
			}

			public int    ID;
			public long   TriggerTime;
			public eState State;
			public string ObjectKey;
		}

		private Dictionary<eEffectMICEType, EffectInstance> _activeEffects           = new();
		private Dictionary<string, MiceEffectObject>        _registeredEffectObjects = new();
		private bool                                        _active                  = true;

		public bool IsActive => _active;

		private void Awake()
		{
			Instance = this;

			var effectObjects = GetComponentsInChildren<MiceEffectObject>();
			if (effectObjects != null)
			{
				foreach (var effectObject in effectObjects)
				{
					_registeredEffectObjects.TryAdd(effectObject.gameObject.name, effectObject);
				}
			}
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		public void SetActive(bool active)
		{
			var changed = _active != active;
			_active = active;
			if (changed)
			{
				if (_active) ResumeEffects();
				else PauseEffects();
			}
		}

		public void PlayEffect(int id, long triggerTime)
		{
			var tableData = TableDataManager.Instance.Get<TableMiceEffect>();
			if (tableData == null) return;

			if (!tableData.Datas.TryGetValue(id, out var data)) return;
			if (_activeEffects.TryGetValue(data.EffectMICEType, out var value))
			{
				if (_activeEffects[data.EffectMICEType].ID == id && _activeEffects[data.EffectMICEType].TriggerTime == triggerTime) return;
			}
			else
			{
				_activeEffects.Add(data.EffectMICEType, new EffectInstance());
			}

			var effectInstance = new EffectInstance();
			effectInstance.ID                   = id;
			effectInstance.TriggerTime          = triggerTime;
			effectInstance.State                = _active ? EffectInstance.eState.PLAYING : EffectInstance.eState.PAUSED;
			effectInstance.ObjectKey            = data.ResName;
			_activeEffects[data.EffectMICEType] = effectInstance;

			if (_active) PlayEffectObject(effectInstance.ObjectKey, triggerTime);
		}

		public void StopEffect(int id, long triggerTime)
		{
			var tableData = TableDataManager.Instance.Get<TableMiceEffect>();
			if (tableData == null) return;

			if (!tableData.Datas.TryGetValue(id, out var data)) return;
			if (!_activeEffects.TryGetValue(data.EffectMICEType, out var effectInstance)) return;

			effectInstance.State = EffectInstance.eState.STOPPED;
			StopEffectObject(effectInstance.ObjectKey);
		}

		private void PlayEffectObject(string objectKey, double serverTime)
		{
			if (!_registeredEffectObjects.TryGetValue(objectKey, out var effectObject)) return;

			var triggerDeltaTime = TimeSpan.FromMilliseconds(ServerTime.Time - serverTime).TotalSeconds;
			if (effectObject.CheckIgnoreTimeOffset(triggerDeltaTime)) return;

			effectObject.Play(triggerDeltaTime);
		}

		private void StopEffectObject(string objectKey)
		{
			if (!_registeredEffectObjects.TryGetValue(objectKey, out var effectObject)) return;
			effectObject.Stop();
		}

		private void ResumeEffects()
		{
			foreach (var kvp in _activeEffects)
			{
				var effectInstance = kvp.Value;
				if (effectInstance.State != EffectInstance.eState.PAUSED) continue;

				effectInstance.State = EffectInstance.eState.PLAYING;
				PlayEffectObject(effectInstance.ObjectKey, effectInstance.TriggerTime);
			}
		}

		private void PauseEffects()
		{
			foreach (var kvp in _activeEffects)
			{
				var effectInstance = kvp.Value;
				if (effectInstance.State != EffectInstance.eState.PLAYING) continue;

				effectInstance.State = EffectInstance.eState.PAUSED;
				StopEffectObject(effectInstance.ObjectKey);
			}
		}
	}
}
