/*===============================================================
* Product:		Com2Verse
* File Name:	LookAtController.cs
* Developer:	eugene9721
* Date:			2023-04-29 14:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.Logger;
using DG.Tweening;
using UnityEngine.Animations.Rigging;

namespace Com2Verse.AvatarAnimation
{
	public sealed class LookAtController : MonoBehaviour
	{
		private bool _isInitialized;

		private Transform? _constrainedTransform;
		private Transform? _lookAtTarget;

		private Transform? _lookAtTargetOverride;

		[Header("LookAt")]
		[field: SerializeField, Range(0, 1f)]
		private float _targetLookAtWeight = 0.844f;

		[field: SerializeField] public Vector2 LookAtLimits { get; set; } = new(-60.0f, 60.0f);

		[SerializeField] private float _lookAtSmoothFactor = 5f;

		[Header("Tween")]
		[SerializeField] private float _onLookAtDuration = 1.4f;

		[SerializeField] private Ease _onLookAtEase  = Ease.OutCubic;
		[SerializeField] private Ease _offLookAtEase = Ease.OutCubic;

		private bool _isLookAt  = true;
		private bool _desiredLookAtState;

		private RigBuilder? _rigBuilder;
		private Rig?        _rig;

		private MultiAimConstraint? _lookAtConstraint;

		private Tweener? _lookAtTween;

		public float TargetLookAtWeight
		{
			get => _targetLookAtWeight;
			set => _targetLookAtWeight = Mathf.Clamp(value, 0, 1);
		}

		public bool IsOnLookAtDelay { get; set; }

		private void Awake()
		{
			_lookAtTargetOverride = new GameObject("HeadLookAtTargetOverride").transform;
			_lookAtTargetOverride.SetParent(transform);
		}

		private void Update()
		{
			if (!_isInitialized) return;
			switch (_desiredLookAtState)
			{
				case true when !_isLookAt:
					EnableLookAt();
					break;
				case false when _isLookAt:
					DisableLookAt();
					break;
			}

			UpdateLookAtTargetPosition();
		}

		private void UpdateLookAtTargetPosition()
		{
			if (_lookAtTarget.IsUnityNull() || _lookAtTargetOverride.IsUnityNull() || IsOnLookAtDelay)
				return;

			_lookAtTargetOverride!.position = Vector3.Lerp(_lookAtTargetOverride.position, _lookAtTarget!.position, Time.deltaTime    * _lookAtSmoothFactor);
			_lookAtTargetOverride.rotation  = Quaternion.Slerp(_lookAtTargetOverride.rotation, _lookAtTarget.rotation, Time.deltaTime * _lookAtSmoothFactor);
		}

#region Initialize
		public void Initialize(Transform constrainedTransform, Transform lookAtTarget)
		{
			_constrainedTransform = constrainedTransform;
			_lookAtTarget         = lookAtTarget;

			RigSetup();
			MultiAimConstraintSetup();

			_isInitialized = true;
		}
#endregion Initialize

		[ContextMenu("EnableLookAt")]
		public void EnableLookAt()
		{
			_desiredLookAtState = true;

			if (!_isInitialized)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "LookAtController is not initialized.");
				return;
			}

			if (_isLookAt) return;

			_isLookAt = true;
			OnLookAtTween(0, 1, _onLookAtDuration, _onLookAtEase);
		}

		[ContextMenu("DisableLookAt")]
		public void DisableLookAt()
		{
			_desiredLookAtState = false;

			if (!_isInitialized)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "LookAtController is not initialized.");
				return;
			}

			if (!_isLookAt) return;

			_isLookAt = false;
			OnLookAtTween(1, 0, _onLookAtDuration, _offLookAtEase);
		}

		private void OnLookAtTween(float start, float end, float duration, Ease ease)
		{
			_lookAtTween?.Kill();

			var weight = start;
			_lookAtTween = DOTween.To(() => weight, x => weight = x, end, duration)
			                      .SetEase(ease)
			                      .OnUpdate(() =>
			                       {
				                       if (_lookAtConstraint.IsUnityNull())
					                       return;

				                       _lookAtConstraint!.weight = weight * _targetLookAtWeight;
			                       })
			                      .OnComplete(OnLookAtComplete);
		}

		private void OnLookAtComplete()
		{
			if (!_rigBuilder.IsUnityNull()) _rigBuilder!.Build();
			_lookAtTween = null;
		}

		private void RigSetup()
		{
			_rigBuilder = gameObject.GetOrAddComponent<RigBuilder>()!;

			// TODO: 다른 컴포넌트에 캐싱해서 사용
			var rigGameObject = new GameObject(AnimationDefine.RigObjectName);
			rigGameObject.transform.SetParent(transform);

			_rig = rigGameObject.AddComponent<Rig>()!;
			_rigBuilder.layers!.Add(new RigLayer(_rig));
		}

		/// <summary>
		/// Constrained Object: Bip001 Head<br/>
		/// AimAxis = Y<br/>
		/// UpAxis = -x<br/>
		/// Constrained Axis: X, Z
		/// </summary>
		private void MultiAimConstraintSetup()
		{
			if (_rig.IsUnityNull() || _rigBuilder.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Rig is null");
				return;
			}

			var constraintObject = new GameObject(AnimationDefine.LookAtConstraintName);
			constraintObject.transform.SetParent(_rig!.transform);

			_lookAtConstraint = constraintObject.AddComponent<MultiAimConstraint>();
			_lookAtConstraint!.Reset();

			_lookAtConstraint!.data.constrainedObject = _constrainedTransform;
			_lookAtConstraint.data.worldUpType        = MultiAimConstraintData.WorldUpType.None;
			_lookAtConstraint.data.aimAxis            = MultiAimConstraintData.Axis.Y;
			_lookAtConstraint.data.upAxis             = MultiAimConstraintData.Axis.X_NEG;

			_lookAtConstraint.data.constrainedXAxis = true;
			_lookAtConstraint.data.constrainedYAxis = true;

			_lookAtConstraint.data.limits = LookAtLimits;

			if (_lookAtTarget.IsUnityNull())
				_lookAtTarget = transform;

			var sources = new WeightedTransformArray(0) { new(_lookAtTargetOverride!, TargetLookAtWeight) };
			_lookAtConstraint.data.sourceObjects = sources;

			_rigBuilder!.Build();
		}

		public void SetLookAtTarget(Transform target)
		{
			_lookAtTarget = target;
		}
	}
}
