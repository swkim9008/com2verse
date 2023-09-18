/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationInfo.DataModel.cs
* Developer:	jhkim
* Date:			2023-06-22 21:25
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com2Verse.WebApi.Service;
using JetBrains.Annotations;

namespace Com2Verse.Organization
{
#region Model
	public class GroupModel : IGroupModel<GroupModel, Components.Group, Components.Group>
	{
#region Variables
		private Components.Group _group;
#endregion // Variables

#region Properties
		public Components.Group Group => _group;
#endregion // Properties

#region Static Methods
		public static GroupModel Parse(Components.Group sourceData)
		{
			var model = new GroupModel();
			model.ParseInternal(sourceData);
			return model;
		}
#endregion // Static Methods

#region Implements
		private void ParseInternal(Components.Group sourceData) => (this as IGroupModel<GroupModel, Components.Group, Components.Group>).ParseInternal(sourceData);
		GroupModel IGroupModel<GroupModel, Components.Group, Components.Group>.ParseInternal(Components.Group sourceData)
		{
			_group = sourceData;
			return this;
		}
#endregion // Implements

#region Dispose
		public void Dispose()
		{
			_group = null;
		}
#endregion // Dispose
	}

	public class TeamModel : ITeamModel<TeamModel, Components.Team, TeamInfo<Components.Team, Components.Member>>
	{
#region Variables
		private TeamInfo<Components.Team, Components.Member> _teamInfo;
		private long[] _subTeamIds;
		private TeamModel _parent;
		private Components.Group _group;
		private string _affiliationName;
#endregion // Variables

#region Properties
		public TeamInfo<Components.Team, Components.Member> Info => _teamInfo;
		public long[] SubTeamIds => _subTeamIds;
		public string GroupName => _group?.GroupName;
		public string AffiliationName => _affiliationName;
		public bool HasParent => _parent != null;
#endregion // Properties

#region Initialize
		public TeamModel()
		{
			_subTeamIds = Array.Empty<long>();
		}
#endregion // Initialize

#region Static Methods
		public static TeamModel Parse(Components.Team sourceData, Components.Group groupData, TeamModel parent = null)
		{
			var model = new TeamModel();
			model.ParseInternal(sourceData);
			model._group = groupData;
			model._parent = parent;
			model.MakeAffiliationName();
			return model;
		}
#endregion // Static Methods

#region Public Methods
		public string GetTeamStr() => $"{GroupName} {_teamInfo?.TeamName}";
		public Components.Group GetGroupInfo() => _group;
		public override string ToString()
		{
			return _subTeamIds.Length > 0 ? $"{_teamInfo.TeamName}\nSubTeams = [ {_subTeamIds.Select(id => Convert.ToString(id)).Aggregate((l, r) => $"{l}, {r}")} ]" : _teamInfo.TeamName;
		}
#endregion // Public Methods

#region Implements
		private void ParseInternal(Components.Team sourceData) => (this as ITeamModel<TeamModel, Components.Team, TeamInfo<Components.Team, Components.Member>>).ParseInternal(sourceData);
		TeamModel ITeamModel<TeamModel, Components.Team, TeamInfo<Components.Team, Components.Member>>.ParseInternal(Components.Team sourceData)
		{
			_teamInfo = new TeamInfo<Components.Team, Components.Member>
			{
				TeamId = sourceData.TeamId,
				ParentTeamId = sourceData.ParentTeamId,
				TeamName = sourceData.TeamName,
				SpaceId = sourceData.SpaceId,
				TotalCount = sourceData.TotalCount,
				Members = sourceData.Members,
				SubTeams = sourceData.SubTeams,
			};

			_subTeamIds = sourceData.SubTeams.Select(subTeam => subTeam.TeamId).ToArray();
			return this;
		}
#endregion // Implements

		private void MakeAffiliationName()
		{
			if (_teamInfo == null) return;

			if (_parent == null)
				_affiliationName = $"{GroupName} {_teamInfo.TeamName}";
			else
				_affiliationName = $"{_parent._affiliationName} {_teamInfo.TeamName}";
		}
#region Dispose
		public void Dispose()
		{
			_teamInfo = null;
		}
#endregion // Dispose
	}

	public class MemberModel : IMemberModel<MemberModel, Components.Member, Components.Member>
	{
#region Variables
		private Components.Member _member;
		private string _teamName;
		private string _affiliation;
		private static StringBuilder _sb;
#endregion // Variables

#region Properties
		public Components.Member Member => _member;
		public string TeamName => _teamName;
		public string Affiliation => _affiliation;
#endregion // Properties

#region Static Methods
		public static MemberModel Parse(Components.Member sourceData, Components.Team teamData)
		{
			var model = new MemberModel();
			model.ParseInternal(sourceData);
			model._teamName = teamData?.TeamName;
			return model;
		}

		public static MemberModel Parse(Components.Member sourceData, TeamModel teamModel)
		{
			var model = new MemberModel();
			model.ParseInternal(sourceData);
			model._teamName = teamModel?.Info?.TeamName;
			model._affiliation = teamModel.AffiliationName;
			return model;
		}
#endregion // Static Methods

#region Public Methods
		public bool IsMine() => _member != null && (_member.AccountId != 0 && _member.AccountId == DataManager.Instance.UserID);
		public string GetPositionLevelTeamStr() => _member == null ? string.Empty : JobInfoStr(_member.Position, _member.Level, TeamName);
		public string GetPositionLevelStr() => _member == null ? string.Empty : JobInfoStr(_member.Position, _member.Level);
		public string GetTeamStr() => _member == null ? string.Empty : TeamName;

		public static int Compare([NotNull] MemberModel left, [NotNull] MemberModel right) => CompareByName(left, right);
		public static int CompareByName([NotNull] MemberModel left, [NotNull] MemberModel right) => left.Member.MemberName.CompareTo(right.Member.MemberName);

		public string GetFormattedTelNo()
		{
			if (_member       == null) return string.Empty;
			if (_member.TelNo == null) return string.Empty;

			var idx = 2;
			var splitLen = 4;
			var telNoStr = Convert.ToString(_member.TelNo);
			if (!IsValidTelNo(telNoStr)) return telNoStr;

			var telNo = telNoStr.AsSpan();
			if (telNo.Slice(0, 2).SequenceEqual("10"))
			{
				_sb ??= new StringBuilder();
				_sb.Clear();
				_sb.Append('0').Append(telNo.Slice(0, 2));
				var len = telNo.Length - 2;
				var mod = len % splitLen;
				if (mod > 0)
				{
					_sb.Append('-');
					_sb.Append(telNo.Slice(idx, mod));
				}

				while (idx < len)
				{
					_sb.Append('-');
					_sb.Append(telNo.Slice(idx, splitLen));
					idx += splitLen;
				}

				return _sb.ToString();
			}

			return telNo.ToString();

			bool IsValidTelNo(string tel) => tel.Length >= 9; // 10-000-0000 ~ 10-0000-0000
		}

		private static string JobInfoStr(string position, string level, string dept) => $"{position}/{level}/{dept}";
		private static string JobInfoStr(string position, string level) => $"{position}/{level}";
#endregion // Public Methods

#region Implements
		private void ParseInternal(Components.Member sourceData) => (this as IMemberModel<MemberModel, Components.Member, Components.Member>).ParseInternal(sourceData);
		MemberModel IMemberModel<MemberModel, Components.Member, Components.Member>.ParseInternal(Components.Member sourceData)
		{
			_member = sourceData;
			return this;
		}
#endregion // Implements

#region Dispose
		public void Dispose()
		{
			_member = null;
		}
#endregion // Dispose
	}
#endregion // Model
}
