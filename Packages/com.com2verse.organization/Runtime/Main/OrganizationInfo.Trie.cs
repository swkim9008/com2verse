/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationInfo.Trie.cs
* Developer:	jhkim
* Date:			2023-06-22 17:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using EmployeeNoType = System.String;
using Com2Verse.Trie;
using Com2Verse.WebApi.Service;
using Sentry;
using TeamIdType = System.Int64;
using MemberIdType = System.Int64;

namespace Com2Verse.Organization
{
	// Trie
	public partial class DataManager
	{
#region Public - Trie
		public GroupModel GetGroupModel() => _trieData?.Group;
		public TeamModel GetTeam(TeamIdType teamId) => _trieData?.GetTeam(teamId);
		public async UniTask<MemberModel> GetMemberAsync(MemberIdType memberId)
		{
			if (_trieData == null)
				return null;

			return await _trieData.GetMemberAsync(memberId);
		}
		public MemberModel GetMember(MemberIdType memberId)
		{
			if (_trieData == null) return null;

			return _trieData.TryGetMember(memberId, out var member) ? member : null;
		}
		public MemberModel[] GetMembers(IEnumerable<MemberIdType> memberIds) => _trieData?.GetMembers(memberIds);
		public List<MemberModel> FindMemberByName(string name) => _trieData?.FindMemberByName(name);
		public IReadOnlyList<MemberIdType> GetMemberIdsFromTeam(TeamIdType teamId) => _trieData?.GetMemberIdsFromTeam(teamId);
#endregion // Public - Trie

#region Trie Data
		private class TrieData : IDisposable
		{
#region Variables
			private GroupModel _groupModel;
			private TeamModel _groupTeam;
			private Dictionary<TeamIdType, TeamModel> _teamMap;
			private Dictionary<MemberIdType, MemberModel> _accountIdMemberMap;
			private Dictionary<TeamIdType, List<MemberIdType>> _teamMemberMap;
			private Trie<MemberModel> _memberTrie;
#endregion // Variables

#region Properties
			public GroupModel Group => _groupModel;
			public TeamModel GroupTeam => _groupTeam;
#endregion // Properties

#region Initialization
			private TrieData()
			{
				_teamMap = new();
				_accountIdMemberMap = new();
				_teamMemberMap = new();
			}
			public static TrieData ParseNew(Components.Group group)
			{
				var newData = new TrieData();
				newData.Parse(group);
				return newData;
			}

			private TrieData Parse(Components.Group group)
			{
				Dispose();

				_groupModel = GroupModel.Parse(group);

				var groupTeam = new Components.Team
				{
					Members = Array.Empty<Components.Member>(),
					TeamId = group.GroupId,
					SubTeams = group.Teams,
					TeamName = group.GroupName,
				};
				_groupTeam = TeamModel.Parse(groupTeam, group);

				_memberTrie = Trie<MemberModel>.CreateNew(Trie<MemberModel>.TrieSettings.Default);

				foreach (var team in group.Teams)
					FillInfos(team);

				return this;

				void FillInfos(Components.Team team, TeamModel parent = null)
				{
					var teamModel = TeamModel.Parse(team, group, parent);
					_teamMap.Add(teamModel.Info.TeamId, teamModel);
					foreach (var member in team.Members)
					{
						var memberModel = MemberModel.Parse(member, teamModel);
						_accountIdMemberMap.TryAdd(member.AccountId, memberModel);
						_memberTrie.Insert(new Trie<MemberModel>.Pair {Key = member.MemberName, Value = memberModel});

						FillTeamMemberMap(member);
					}

					foreach (var subTeam in team.SubTeams)
						FillInfos(subTeam, teamModel);

					void FillTeamMemberMap(Components.Member member)
					{
						if (_teamMemberMap.TryGetValue(member.TeamId, out var memberList))
						{
							memberList.TryAdd(member.AccountId);
						}
						else
						{
							var memberIds = new List<MemberIdType> {member.AccountId};
							_teamMemberMap.Add(member.TeamId, memberIds);
						}
					}
				}
			}
#endregion // Initialization

#region Public Methods
			public TeamModel GetTeam(TeamIdType teamId)
			{
				if (!IsValidTeamId(teamId)) return null;

				if (_groupTeam.Info.TeamId == teamId) return _groupTeam;

				return _teamMap.TryGetValue(teamId, out var find) ? find : null;
			}

			public async UniTask<MemberModel> GetMemberAsync(MemberIdType memberId)
			{
				if (!_accountIdMemberMap.ContainsKey(memberId))
				{
					var groupId = _groupModel?.Group?.GroupId ?? -1;
					if (groupId != -1)
						await RequestOrganizationChartAsync(groupId);

					return !Instance._trieData._accountIdMemberMap.ContainsKey(memberId) ? null : Instance._trieData._accountIdMemberMap[memberId];
				}

				return _accountIdMemberMap[memberId];
			}

			public MemberModel[] GetMembers(IEnumerable<MemberIdType> memberNos)
			{
				var result = new List<MemberModel>();

				foreach (var memberNo in memberNos)
				{
					var member = _accountIdMemberMap.ContainsKey(memberNo) ? _accountIdMemberMap[memberNo] : null;
					if (member != null)
						result.Add(member);
				}
				return result.ToArray();
			}
			public List<MemberModel> FindMemberByName(string name)
			{
				var result = _memberTrie.FindAll(name) ?? new();
				return result;
			}

			public IReadOnlyList<MemberIdType> GetMemberIdsFromTeam(TeamIdType teamId)
			{
				if (_teamMemberMap.TryGetValue(teamId, out var result))
					return result;

				return Array.Empty<MemberIdType>();
			}

			public bool TryGetMember(MemberIdType memberId, out MemberModel memberModel) => _accountIdMemberMap.TryGetValue(memberId, out memberModel);
			private bool IsValidTeamId(TeamIdType teamId) => teamId >= 0;
#endregion // Public Methods

#region Dispose
			public void Dispose()
			{
				_groupModel?.Dispose();
				_groupModel = null;

				if (_teamMap != null)
				{
					foreach (var (_, value) in _teamMap)
						value?.Dispose();
					_teamMap.Clear();
				}

				_accountIdMemberMap?.Clear();
				if (_teamMemberMap != null)
				{
					foreach (var (_, value) in _teamMemberMap)
						value?.Clear();
					_teamMemberMap.Clear();
				}

				_memberTrie?.Dispose();
			}
#endregion // Dispose
		}
#endregion // Trie Data
	}
}
