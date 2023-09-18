/*===============================================================
* Product:		Com2Verse
* File Name:	UISortingOrderInfo.cs
* Developer:	tlghks1009
* Date:			2023-05-12 14:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;

namespace Com2Verse.UI
{
	[CreateAssetMenu(fileName = "SystemUIInfo", menuName = "Com2VerseUI/System UI Info")]
	[Serializable]
	public sealed class SystemUIInfo : ScriptableObject
	{
		[Serializable]
		public class SystemUI
		{
			[field: SerializeField] public eSystemViewType SystemViewType;
			[field: SerializeField] public string          ViewName;
			[field: SerializeField] public int             SortingOrder;

			[field: NonSerialized] public GUIView GUIView;
		}

		[field: SerializeField] public List<SystemUI> SystemUIData;

		[NonSerialized] private Dictionary<eSystemViewType, SystemUI> _systemUIDataDictionary;


		public void Parse()
		{
			_systemUIDataDictionary = new Dictionary<eSystemViewType, SystemUI>();

			foreach (var systemUIData in SystemUIData)
			{
				if (!_systemUIDataDictionary.TryAdd(systemUIData.SystemViewType, systemUIData))
				{
					C2VDebug.LogError($"[SystemUI] Duplicate name.. ViewName : {systemUIData.SystemViewType}");
				}
			}
		}


		public IEnumerable<SystemUI> GetSystemUIs() => _systemUIDataDictionary?.Values;


		public bool TryGetValue(eSystemViewType systemViewType, out SystemUI systemUI)
		{
			return _systemUIDataDictionary.TryGetValue(systemViewType, out systemUI);
		}
	}
}
