/*===============================================================
* Product:		Com2Verse
* File Name:	TagProcessorManager.cs
* Developer:	haminjeong
* Date:			2023-05-17 14:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Com2Verse.Data;
using Com2Verse.Logger;
using JetBrains.Annotations;

namespace Com2Verse.Network
{
	public sealed class TagProcessorManager : Singleton<TagProcessorManager>
	{
		private readonly Dictionary<eObjectType, ITagProcessor> _tagProcessors;
		private          bool                                   _isAlreadyAddProcessors;

		[UsedImplicitly]
		private TagProcessorManager()
		{
			_tagProcessors          = new();
			_isAlreadyAddProcessors = false;
		}
		
		public void Initialize()
		{
			ObjectUtil.LoadTable();
		}
		
		public void Clear()
		{
			_isAlreadyAddProcessors = false;
			_tagProcessors?.Clear();
		}

		/// <summary>
		/// 외부에서 어셈블리를 조회하여 Processor가 담긴 클래스를 등록시켜 줍니다.
		/// </summary>
		/// <param name="types">ITagProcessor 담긴 클래스의 모음</param>
		public void RegisterTagProcessors(IEnumerable<Type> types)
		{
			if (_isAlreadyAddProcessors) return;
			foreach (var type in types)
			{
				C2VDebug.Log($"-> {type.Name}");
				ITagProcessor tagProcessor = Activator.CreateInstance(type) as ITagProcessor;
				tagProcessor.Initialize();
				_tagProcessors.TryAdd(type.GetCustomAttribute<TagObjectTypeAttribute>().ObjectType, tagProcessor);
				C2VDebug.Log($"-> {type.Name}...DONE");
			}

			_isAlreadyAddProcessors = true;
		}

		/// <summary>
		/// BaseObjectID 값으로 로직 분기를 한다.
		/// </summary>
		/// <param name="mapObject">해당 오브젝트</param>
		public void TagProcess(BaseMapObject mapObject)
		{
			var key = ObjectUtil.GetObjectType(mapObject!.ObjectTypeId);
			if (!_tagProcessors.TryGetValue(key, out var handler))
				return;
			handler?.TagProcess(mapObject);
		}

		/// <summary>
		/// 해당 오브젝트가 사용하고 있는 상호작용 오브젝트를 사용 중인지 여부를 업데이트 시켜준다.
		/// </summary>
		/// <param name="type">업데이트를 요청할 오브젝트 타입</param>
		/// <param name="targetObject">해당 오브젝트</param>
		/// <param name="isUse">사용 중 여부</param>
		public void UpdateUseObjectProcess(eObjectType type, BaseMapObject targetObject, bool isUse)
		{
			if (!_tagProcessors.TryGetValue(type, out var handler))
				return;
			handler?.UpdateUseObject(targetObject, isUse);
		}
	}
}
