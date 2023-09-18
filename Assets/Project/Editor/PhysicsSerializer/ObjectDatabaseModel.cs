// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ObjectDatabaseModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-09 오후 12:00
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using UnityEngine;

namespace Com2VerseEditor.PhysicsSerializer
{
	public class ObjectDatabaseModel
	{
		public string ObjectId;
		public long BaseObjectId;
		public Vector3 Location;
		public Vector3 Size;
		public Vector3 Rotation;
		public string HexCode = "#000000";
		public int ObjectWeight = 1;
		public int ObjectGravity = 10;
		public string ObjectPath;
	}

	public class ObjectInteractionDatabaseModel
	{
		public string MappingId;
		public long SpaceObjectId;
		public int InteractionNo;
		public long InteractionLink;
		public string InteractionValue;
	}
}
