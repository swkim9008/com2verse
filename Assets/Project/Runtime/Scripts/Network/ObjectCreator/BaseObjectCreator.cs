/*===============================================================
* Product:		Com2Verse
* File Name:	BaseObjectCreator.cs
* Developer:	haminjeong
* Date:			2023-01-05 15:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Google.Protobuf;
using UnityEngine;

namespace Com2Verse.Network
{
	public abstract class BaseObjectCreator : IObjectCreator
	{
		private Action<long, long, IMessage, Vector3, Action<long, BaseMapObject>> _objectCreator;

		public abstract void Initialize(Func<long, long, bool> checkExist, Func<long, int, BaseMapObject> checkPool, Transform rootTrans);

		protected void SetDelegates(Action<long, long, IMessage, Vector3, Action<long, BaseMapObject>> creator)
		{
			_objectCreator = creator;
		}

		public virtual void ReleaseObject(BaseMapObject mapObject) { }

		Action<long, long, IMessage, Vector3, Action<long, BaseMapObject>> IObjectCreator.ObjectCreator
		{
			get => _objectCreator;
			set => _objectCreator = value;
		}
	}
}
