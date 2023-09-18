/*===============================================================
* Product:		Com2Verse
* File Name:	BaseTagProcessor.cs
* Developer:	haminjeong
* Date:			2023-05-17 10:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;

namespace Com2Verse.Network
{
	public abstract class BaseTagProcessor : ITagProcessor
	{
		private readonly Dictionary<string, Action<string, BaseMapObject>> _tagProcessors = new();

		public abstract void Initialize();
		
		protected void SetDelegates(string key, Action<string, BaseMapObject> handler)
		{
			if (string.IsNullOrEmpty(key)) return;
			_tagProcessors?.Add(key, handler);
		}
		
		public virtual void UpdateUseObject(BaseMapObject targetObject, bool isUse) { }

		Dictionary<string, Action<string, BaseMapObject>> ITagProcessor.TagProcessors => _tagProcessors;
	}
}

