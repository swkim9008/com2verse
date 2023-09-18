// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderAction.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오후 2:08
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using UnityEngine;

namespace Com2Verse.Builder
{
	public abstract class BuilderAction : ReversibleAction
	{
		public GameObject TargetObject { get; set; }

		public BuilderAction(GameObject targetObject)
		{
			TargetObject = targetObject;
		}
	}

	public class MoveObjectAction : BuilderAction
	{
		public Vector3 From { get; set; }
		public Vector3 To { get; set; }
		public Quaternion FromRotation { get; set; }
		public Quaternion ToRotation { get; set; }
		
		public Transform FromParent { get; set; }
		public Transform ToParent { get; set; }

		private BuilderObject _targetBuilderObject = null;

		public MoveObjectAction(BuilderObject targetObject, Vector3 from, Vector3 to, Quaternion fromRotation, Quaternion toRotation, Transform fromParent, Transform toParent) : base(targetObject.gameObject)
		{
			From = from;
			To = to;
			FromParent = fromParent;
			ToParent = toParent;
			FromRotation = fromRotation;
			ToRotation = toRotation;
			_targetBuilderObject = targetObject;
		}

		public override void Undo()
		{
			base.Undo();
			Transform targetTransform = TargetObject.transform;
			targetTransform.SetParent(FromParent, true);
			targetTransform.SetPositionAndRotation(From, FromRotation);
			targetTransform.RoundTransform();
		}

		public override void Do(bool isInitial = true)
		{
			base.Do(isInitial);
			Transform targetTransform = TargetObject.transform;
			targetTransform.SetParent(ToParent, true);
			targetTransform.SetPositionAndRotation(To, ToRotation);
			targetTransform.RoundTransform();
		}
	}

	public abstract class AddOrRemoveObjectAction : BuilderAction
	{
		protected Transform Parent;
		protected Transform Trashcan;

		public long ObjectId { get; set; }

		public AddOrRemoveObjectAction(GameObject targetObject, Transform parent, Transform trashcan) : base(targetObject)
		{
			Parent = parent;
			Trashcan = trashcan;
		}

		public override void Clean()
		{
			Object.Destroy(TargetObject);
		}
	}

	public class AddObjectAction : AddOrRemoveObjectAction
	{
		public AddObjectAction(GameObject targetObject, Transform parent, Transform trashcan) : base(targetObject, parent, trashcan) {}
		
		public override void Undo()
		{
			base.Undo();
			Transform targetTransform = TargetObject.transform;
			targetTransform.SetParent(Trashcan, true);
			targetTransform.RoundTransform();
		}

		public override void Do(bool isInitial = true)
		{
			base.Do(isInitial);
			Transform targetTransform = TargetObject.transform;
			targetTransform.SetParent(Parent, true);
			targetTransform.RoundTransform();
		}
	}

	public class RemoveObjectAction : AddOrRemoveObjectAction
	{
		public RemoveObjectAction(GameObject targetObject, Transform parent, Transform trashcan) : base(targetObject, parent, trashcan) { }

		public override void Undo()
		{
			base.Undo();
			Transform targetTransform = TargetObject.transform;
			targetTransform.SetParent(Parent, true);
			targetTransform.RoundTransform();
		}

		public override void Do(bool isInitial = true)
		{
			base.Do(isInitial);
			Transform targetTransform = TargetObject.transform;
			targetTransform.SetParent(Trashcan, true);
			targetTransform.RoundTransform();
		}

		public override void Clean()
		{
			
		}
	}

	public class ChangeMaterialAction : BuilderAction
	{
		private Renderer _targetRenderer;
		public Material From { get; set; }
		public Material To { get; set; }

		private long _lastTextureId;
		private long _targetTextureId;
		private BaseWallObject _object;

		public ChangeMaterialAction(GameObject targetObject, Renderer targetRenderer, Material from, Material to, long objectId) : base(targetObject)
		{
			From = from;
			To = to;
			_targetRenderer = targetRenderer;
			_object = targetObject.GetComponentInParent<BaseWallObject>(true);
			_targetTextureId = objectId;
		}

		public override void Undo()
		{
			base.Undo();
			_targetRenderer.sharedMaterial = From;
			_object.AppliedTextureId = _lastTextureId;
			_object.AssignedMaterial = From;
		}

		public override void Do(bool isInitial = true)
		{
			base.Do(isInitial);
			_targetRenderer.sharedMaterial = To;
			_lastTextureId = _object.AppliedTextureId;
			_object.AppliedTextureId = _targetTextureId;
			_object.AssignedMaterial = To;
		}
	}
}
