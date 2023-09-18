/*===============================================================
* Product:		Com2Verse
* File Name:	ITagProcessor.cs
* Developer:	haminjeong
* Date:			2023-05-17 10:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;

namespace Com2Verse.Network
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TagObjectTypeAttribute : Attribute
	{
		public eObjectType ObjectType { get; }

		public TagObjectTypeAttribute(eObjectType type) => ObjectType = type;
	}
	
	public interface ITagProcessor
	{
		protected Dictionary<string, Action<string, BaseMapObject>> TagProcessors { get; }

		/// <summary>
		/// 태그 로직을 담아 초기화하는 함수.
		/// </summary>
		public void Initialize();
		
		/// <summary>
		/// 등록된 태그 로직을 호출합니다.
		/// </summary>
		/// <param name="mapObject">태그가 동작하는 오브젝트</param>
		public void TagProcess(BaseMapObject mapObject)
		{
			if (mapObject.IsUnityNull())
			{
				C2VDebug.LogError($"Tag Object is null!");
				return;
			}

			for (int i = 0; i < mapObject.TagInfos?.Count; ++i)
			{
				if (!TagProcessors.TryGetValue(mapObject.TagInfos[i].key, out var func))
					continue;
				func?.Invoke(mapObject.TagInfos[i].value, mapObject);
			}
		}

		public void UpdateUseObject(BaseMapObject targetObject, bool isUse);
	}
}
