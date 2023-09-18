/*===============================================================
* Product:		Com2Verse
* File Name:	IGroupModel.cs
* Developer:	jhkim
* Date:			2023-06-22 21:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;

namespace Com2Verse.Organization
{
	/// <summary>
	/// 조직도의 그룹 (기존 구조에선 회사에 해당) 정보를 구성하는 인터페이스 명세
	/// (Old) Company
	/// (New) Group
	/// </summary>
	/// <typeparam name="TModelType">상속받는 클래스의 타입</typeparam>
	/// <typeparam name="TDstType">변환된 데이터의 타입</typeparam>
	/// <typeparam name="TSrcType">데이터를 파싱하기 위한 원본 타입(그룹)</typeparam>
	public interface IGroupModel<out TModelType, out TDstType, in TSrcType> : IDisposable where TModelType : IGroupModel<TModelType, TDstType, TSrcType>
	{
		public TDstType Group { get; }
		internal TModelType ParseInternal(TSrcType sourceData);
	}
}
