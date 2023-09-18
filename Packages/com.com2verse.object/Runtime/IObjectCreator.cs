/*===============================================================
* Product:		Com2Verse
* File Name:	IObjectCreator.cs
* Developer:	haminjeong
* Date:			2022-12-28 17:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Google.Protobuf;
using UnityEngine;

namespace Com2Verse.Network
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DefinitionAttribute : Attribute
	{
		public long Definition { get; }

		public DefinitionAttribute(long definition) => Definition = definition;
	}
	
	public interface IObjectCreator
	{
		protected Action<long, long, IMessage, Vector3, Action<long, BaseMapObject>> ObjectCreator { get; set; }

		/// <summary>
		/// 오브젝트를 생성하는 로직을 담아 초기화하는 함수. 
		/// </summary>
		/// <param name="checkExist">생성 시 이미 오브젝트가 있는지 체크</param>
		/// <param name="checkPool">생성 시 풀에서 가져올 수 있는지 체크</param>
		/// <param name="rootTrans">생성 시 기준이 되는 부모 Transform</param>
		public void Initialize(Func<long, long, bool> checkExist, Func<long, int, BaseMapObject> checkPool,
			Transform rootTrans);

		/// <summary>
		/// 등록된 생성자를 호출합니다.
		/// </summary>
		/// <param name="serial">Serial ID</param>
		/// <param name="definition">오브젝트 정의</param>
		/// <param name="data">오브젝트 정보</param>
		/// <param name="initialPosition">생성된 직후 초기 위치</param>
		/// <param name="onCompleted">생성 후 콜백</param>
		public void CreateObject(long serial, long definition, IMessage data, Vector3 initialPosition, Action<long, BaseMapObject> onCompleted)
		{
			ObjectCreator?.Invoke(serial, definition, data, initialPosition, onCompleted);
		}

		/// <summary>
		/// 오브젝트를 삭제 시 함께 처리할 로직입니다.
		/// </summary>
		/// <param name="mapObject">삭제될 BaseMapObject</param>
		public void ReleaseObject(BaseMapObject mapObject);
	}
}
