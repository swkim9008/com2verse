/*===============================================================
* Product:		Com2Verse
* File Name:	KioskObject.cs
* Developer:	ikyoung
* Date:			2023-08-07 17:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Network
{
	public sealed class KioskObject : MonoBehaviour
	{
		private Dictionary<string, object> _tags = new Dictionary<string, object>();
		
		public void UpdateTagValue<T>(object tagValue)
		{
			var type = typeof(T); 
			_tags.Remove(type.Name);
			_tags.Add(type.Name, tagValue);
		}

		public string GetUrl()
		{
			var tagValue = GetTagValue<KioskTag>();
			if (tagValue != null)
			{
				return tagValue.kioskWebUrl;	
			}

			return "";
		}

		public bool HasValidTagValue()
		{
			return HasUrl();
		}

		public bool HasUrl()
		{
			var tagValue = GetTagValue<KioskTag>();
			if (tagValue != null)
			{
				return !string.IsNullOrEmpty(tagValue.kioskWebUrl);
			}
			return false;
		}

		public T GetTagValue<T>()
		{
			if (_tags.TryGetValue(typeof(T).Name, out var res))
			{
				return (T)res;
			}
			return default(T);
		}
		
		public async UniTask LoadAsync()
		{
			string tagKey = KioskTagProcessor.TagKey;
			if (_tags.TryGetValue(tagKey, out object res))
			{
			}
		}
	}
}
