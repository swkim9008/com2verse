/*===============================================================
* Product:		Com2Verse
* File Name:	C2VEntryOperationContainer.cs
* Developer:	tlghks1009
* Date:			2023-03-14 18:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections;
using System.Collections.Generic;

namespace Com2VerseEditor.AssetSystem
{
	public class C2VEntryOperationDictionary : IEnumerable<KeyValuePair<string, C2VAddressablesEntryOperationInfo>>
	{
		private readonly Dictionary<string, C2VAddressablesEntryOperationInfo> _entryOperationInfos = new();


		public bool ContainsKey(string key)
		{
			return _entryOperationInfos.ContainsKey(key);
		}


		public void Add(string key, C2VAddressablesEntryOperationInfo entryOperationInfo)
		{
			_entryOperationInfos.Add(key, entryOperationInfo);
		}


		public void Clear()
		{
			_entryOperationInfos.Clear();
		}


		public bool TryGetValue(string key, out C2VAddressablesEntryOperationInfo value)
		{
			return _entryOperationInfos.TryGetValue(key, out value);
		}


		public int Count => _entryOperationInfos.Count;


		public IEnumerator<KeyValuePair<string, C2VAddressablesEntryOperationInfo>> GetEnumerator()
		{
			foreach (var kvp in _entryOperationInfos)
			{
				yield return kvp;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
