/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_Event_Package.cs
* Developer:	klizzard
* Date:			2023-07-26 16:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Protocols.Mice;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager
	{
		public async UniTask SyncEventPackages(long eventId)
		{
			var result = await MiceWebClient.Event.PackagesGet_EventId(eventId);
			if (result)
			{
				var datas = result.Data;
				foreach (var entry in datas)
				{
					if (EventInfos.TryGetValue(entry.EventId, out var eventInfo))
						eventInfo.AddOrUpdatePackageInfo(entry);
				}
			}

			await UniTask.CompletedTask;
		}
	}
}
