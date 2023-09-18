/*===============================================================
* Product:		Com2Verse
* File Name:	CullingGroupProxy.cs
* Developer:	eugene9721
* Date:			2022-12-22 15:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Logger;
using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public enum eCullingGroupType
	{
		HUD = 0,
	}

	public enum eCullingTargetsUpdateMode
	{
		EVERY_UPDATE = 0,
		MANUAL       = 1,
	}

	[RequireComponent(typeof(MetaverseCamera))]
	public sealed class CullingGroupProxy : MonoBehaviour
	{
#region ConstantFields
		private static readonly int     ArrayMinLength   = 16;
		private static readonly float[] DefaultDistances = { 0, float.PositiveInfinity };

		private readonly List<ICullingTarget>    _cullingTargets       = new();
		private readonly HashSet<ICullingTarget> _targetsToAdd         = new();
		private readonly HashSet<ICullingTarget> _targetsToRemove      = new();
		private readonly List<int>               _dynamicTargetIndices = new();
#endregion ConstantFields

#region InspectorFields
		[SerializeField] [ReadOnly]
		private eCullingGroupType _cullingGroupType = eCullingGroupType.HUD;

		[SerializeField]
		eCullingTargetsUpdateMode _targetsUpdateMode = eCullingTargetsUpdateMode.EVERY_UPDATE;

		[SerializeField]
		private Transform? _distanceReferencePoint = null;

		[SerializeField]
		private float[] _boundingDistances = Array.Empty<float>();
#endregion InspectorFields

#region Fields
		BoundingSphere[] _boundingSpheres = Array.Empty<BoundingSphere>();

		private MetaverseCamera? _owner        = null;
		private CullingGroup?    _cullingGroup = null;
#endregion Fields

#region Properites
		public Transform? DistanceReferencePoint
		{
			get => _distanceReferencePoint;
			set
			{
				_distanceReferencePoint = value;
				_cullingGroup?.SetDistanceReferencePoint(_distanceReferencePoint);
			}
		}

		public float[] BoundingDistances
		{
			get => _boundingDistances;
			set
			{
				_boundingDistances = value;
				_cullingGroup?.SetBoundingDistances(_boundingDistances.Length > 0 ? _boundingDistances : DefaultDistances);
			}
		}

		public IReadOnlyList<ICullingTarget> Targets => _cullingTargets;

		public IReadOnlyList<BoundingSphere> BoundingSpheres => _boundingSpheres;

		public eCullingGroupType CullingGroupType => _cullingGroupType;
#endregion Properites

#region MonoBehaviour
		private void Awake()
		{
			_owner = GetComponent<MetaverseCamera>();
			_cullingGroup = new CullingGroup
			{
				targetCamera   = _owner.Camera,
				onStateChanged = OnStateChanged
			};
		}

		private void Start()
		{
			if (_cullingGroup == null) return;
			_cullingGroup.SetBoundingDistances(_boundingDistances.Length > 0 ? _boundingDistances : DefaultDistances);
			_cullingGroup.SetDistanceReferencePoint(DistanceReferencePoint);
		}

		private void OnEnable()
		{
			if (_cullingGroup == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "CullingGroup is null");
				return;
			}

			_cullingGroup.enabled = true;
		}

		private void OnDisable()
		{
			if (_cullingGroup == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "CullingGroup is null");
				return;
			}

			_cullingGroup.enabled = false;
		}

		private void OnDestroy()
		{
			_dynamicTargetIndices.Clear();
			_cullingGroup?.Dispose();
			_cullingGroup = null;
		}

		private void Update()
		{
			if (_targetsUpdateMode == eCullingTargetsUpdateMode.EVERY_UPDATE)
			{
				UpdateTargets();
			}

			UpdateDynamicBoundingSphereTransforms();
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			DistanceReferencePoint = DistanceReferencePoint;
			BoundingDistances      = BoundingDistances;
		}

#endif
#endregion MonoBehaviour

#region UpdateTarget
		public void UpdateTargets()
		{
			bool hasTargetsToRemove = _targetsToRemove.Count > 0;
			if (hasTargetsToRemove) RemoveTargets();

			bool hasTargetsToAdd = _targetsToAdd.Count > 0;
			if (hasTargetsToAdd) AddTargets();

			if (hasTargetsToAdd || hasTargetsToRemove)
			{
				int targetsCount = _cullingTargets.Count;

				int nextPowerOfTwo = Mathf.NextPowerOfTwo(targetsCount);
				int length         = (nextPowerOfTwo > ArrayMinLength) ? nextPowerOfTwo : ArrayMinLength;
				if (length != _boundingSpheres.Length)
					_boundingSpheres = new BoundingSphere[length];

				_dynamicTargetIndices.Clear();
				for (int i = 0; targetsCount > i; i++)
				{
					var target = _cullingTargets[i];
					if (target == null) continue;
					_boundingSpheres[i] = target.UpdateAndGetBoundingSphere();

					if (target.BoundingSphereUpdateMode == eCullingUpdateMode.DYNAMIC)
						_dynamicTargetIndices.Add(i);
				}

				_cullingGroup!.SetBoundingSphereCount(0);
				_cullingGroup.SetBoundingSpheres(_boundingSpheres);
				_cullingGroup.SetBoundingSphereCount(targetsCount);
			}
		}

		private void RemoveTargets()
		{
			foreach (ICullingTarget target in _targetsToRemove)
				_cullingTargets.Remove(target);
			_targetsToRemove.Clear();
		}

		private void AddTargets()
		{
			_cullingTargets.AddRange(_targetsToAdd);
			_targetsToAdd.Clear();
		}
#endregion UpdateTarget

		public void Add(ICullingTarget target)
		{
			_targetsToAdd.Add(target);
			_targetsToRemove.Remove(target);
		}

		public void Remove(ICullingTarget target)
		{
			_targetsToRemove.Add(target);
			_targetsToAdd.Remove(target);
		}

		public bool IsVisible(ICullingTarget target)
		{
			var index = IndexOf(target);
			if (index == -1)
				return false;

			return _cullingGroup!.IsVisible(index);
		}

		public bool IsContained(ICullingTarget target) => _cullingTargets.Contains(target);

		public int IndexOf(ICullingTarget target) => _cullingTargets.IndexOf(target);

		public void UpdateDynamicBoundingSphereTransforms()
		{
			foreach (var index in _dynamicTargetIndices)
			{
				ICullingTarget target = _cullingTargets[index];
				if (target is UnityEngine.Object)
					_boundingSpheres[index] = target.UpdateAndGetBoundingSphere();
			}
		}

		public void UpdateAllBoundingSphereTransforms()
		{
			for (int i = 0; _cullingTargets.Count > i; i++)
			{
				ICullingTarget target = _cullingTargets[i];
				_boundingSpheres[i] = target.UpdateAndGetBoundingSphere();
			}
		}

		private void OnStateChanged(CullingGroupEvent cullingGroupEvent)
		{
			_cullingTargets[cullingGroupEvent.index].OnHudStateChanged?.Invoke(cullingGroupEvent);
		}

		public void SetCullingGroupType(eCullingGroupType cullingGroupType)
		{
			_cullingGroupType = cullingGroupType;
			var boundingDistances = CameraManager.Instance.GetCullingGroupBoundingDistance(cullingGroupType);
			BoundingDistances = boundingDistances ?? DefaultDistances;
		}
	}
}
