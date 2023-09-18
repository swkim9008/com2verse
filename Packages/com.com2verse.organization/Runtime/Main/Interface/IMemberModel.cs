/*===============================================================
* Product:		Com2Verse
* File Name:	IMemberModel.cs
* Developer:	jhkim
* Date:			2023-06-22 21:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.Organization
{
	/// <summary>
	/// 조직도의 멤버 (기존 구조에선 직원에 해당) 정보를 구성하는 인터페이스 명세
	/// (Old) Employee
	/// (New) Member
	/// </summary>
	/// <typeparam name="TModelType">상속받는 클래스의 타입</typeparam>
	/// <typeparam name="TSrcType">데이터를 파싱하기 위한 원본 타입(멤버)</typeparam>
	/// <typeparam name="TInfo">멤버 정보에 대한 클래스 타입</typeparam>
	public interface IMemberModel<out TModelType, in TSrcType, out TInfo> : IDisposable
	{
		public TInfo Member { get; }
		internal TModelType ParseInternal(TSrcType sourceData);
	}
}
