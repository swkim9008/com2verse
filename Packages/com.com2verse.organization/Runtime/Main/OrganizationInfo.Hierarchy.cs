/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationInfo.Hierarchy.cs
* Developer:	jhkim
* Date:			2023-06-22 17:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.WebApi.Service;
using OrganizationTreeModel = Com2Verse.Organization.HierarchyTreeModel<long>;

namespace Com2Verse.Organization
{
	// Hierarchy
	public partial class DataManager
	{

#region Public - Hierarchy
		public HierarchyTree<OrganizationTreeModel>[] GetHierarchyRoot() => _hierarchyData?.RootItems;
#endregion // Public - Hierarchy

#region Hierarchy Data
		private class HierarchyData : IDisposable
		{
#region Variables
			private HierarchyTree<OrganizationTreeModel>[] _rootItems;
#endregion // Variables

#region Properties
			public HierarchyTree<OrganizationTreeModel>[] RootItems => _rootItems;
#endregion // Properties

#region Initialization
			private HierarchyData()
			{
				_rootItems = Array.Empty<HierarchyTree<OrganizationTreeModel>>();
			}

			public static HierarchyData ParseNew(Components.Group group)
			{
				var newData = new HierarchyData();
				var groupLength = 1; // TODO : 그룹이 여러개인 경우 수정
				newData._rootItems = new HierarchyTree<OrganizationTreeModel>[groupLength];

				var groupModel = GroupModel.Parse(group);
				var treeModel = new OrganizationTreeModel
				{
					Name = group.GroupName,
					ID = group.GroupId,
				};
				var tree = HierarchyTree<OrganizationTreeModel>.CreateNew(treeModel);
				FillTreeGroup(groupModel, tree);
				tree.SetItemIndex();
				newData._rootItems[0] = tree;
				return newData;

				void FillTreeGroup(GroupModel model, HierarchyTree<OrganizationTreeModel> tree)
				{
					var group = model.Group;
					for (var j = 0; j < group.Teams.Length; j++)
					{
						var team = group.Teams[j];
						tree.AddChildren(new OrganizationTreeModel
						{
							Name = team.TeamName,
							ID = team.TeamId,
						});

						var teamModel = TeamModel.Parse(team, group);
						FillTreeTeam(teamModel, tree[j]);
					}
				}
				void FillTreeTeam(TeamModel teamModel, HierarchyTree<OrganizationTreeModel> tree)
				{
					for (var i = 0; i < teamModel.Info.SubTeams.Length; i++)
					{
						var subTeam = teamModel.Info.SubTeams[i];
						tree.AddChildren(new OrganizationTreeModel
						{
							Name = subTeam.TeamName,
							ID = subTeam.TeamId,
						});

						var subTeamModel = TeamModel.Parse(subTeam, group, teamModel);
						FillTreeTeam(subTeamModel, tree[i]);
					}
				}
			}
#endregion // Initialization

#region Dispose
			public void Dispose()
			{
				if (_rootItems != null)
				{
					foreach (var hierarchyTree in _rootItems)
						hierarchyTree?.Dispose();
					_rootItems = Array.Empty<HierarchyTree<OrganizationTreeModel>>();
				}
			}
#endregion // Dispose
		}
#endregion // Hierarchy Data
	}
}
