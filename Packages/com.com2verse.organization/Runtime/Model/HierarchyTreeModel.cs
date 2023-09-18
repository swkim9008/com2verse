/*===============================================================
* Product:		Com2Verse
* File Name:	HierarchyTreeModel.cs
* Developer:	jhkim
* Date:			2022-07-19 11:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Organization
{
	public struct HierarchyTreeModel<T>
	{
		public int Index;
		public int GroupIndex;
		public string Name;
		public T ID;
		public override string ToString() => Name;
	}
}
