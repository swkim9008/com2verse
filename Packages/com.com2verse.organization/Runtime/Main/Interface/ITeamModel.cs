/*===============================================================
* Product:		Com2Verse
* File Name:	ITeamModel.cs
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
	/// 조직도의 팀 (기존 구조에선 부서에 해당) 정보를 구성하는 인터페이스 명세
	/// (Old) Department
	/// (New) Team
	/// </summary>
	/// <typeparam name="TModelType">상속받는 클래스의 타입</typeparam>
	/// <typeparam name="TSrcType">데이터를 파싱하기 위한 원본 타입(팀)</typeparam>
	/// <typeparam name="TInfo">팀 정보에 대한 클래스 타입</typeparam>
	public interface ITeamModel<out TModelType, in TSrcType, out TInfo> : IDisposable
	{
		public TInfo Info { get; }
		internal TModelType ParseInternal(TSrcType sourceData);
	}

	public class TeamInfo<TTeam, TMember>
	{
		public long TeamId;
		public long ParentTeamId;
		public string TeamName;
		public string SpaceId;
		public int TotalCount;
		public TMember[] Members;
		public TTeam[] SubTeams;
	}
}
