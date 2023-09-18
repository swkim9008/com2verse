/*===============================================================
* Product:		Com2Verse
* File Name:	FollowCamera.cs
* Developer:	eugene9721
* Date:			2023-01-27 11:29
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Cinemachine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.CameraSystem
{
	public sealed class FollowCamera : CameraBase
	{
#region Fields
		private const float  MinDist            = 10;
		private const float  RotationThreshold  = 20;
		private const string CameraRotationName = "CameraRotation";

		private IAvatarTarget? _avatarTarget;
		private float          _height = 1.5f;

		private float _targetZoom;

		// Cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

		private Cinemachine3rdPersonFollow? _cinemachineThirdPersonFollow;
		private CinemachineComposer?        _cinemachineComposer;

		// Cinemachine Helper
		private FollowCameraTargetProxy? _cameraTargetProxy;
		private Transform?               _cinemachineCameraTarget;

		// player
		private float _rotationVelocity;
		private float _rotationVelocityPitch;
		private float _rotationVelocityYaw;

		// input
		private Vector2 _inputMove;
		private Vector2 _inputLook;

		private Rect      _frustumRect    = new Rect(0, 0, 1, 1);
		private Vector3[] _frustumCorners = new Vector3[4];
		private bool      _desiredSetCameraColliderMinSize;

		// WayPoint
		private Vector3 _lastPos;
		private float   _progress;

		private float _targetRotationYawFromWaypoint;

		private float _moveMaintainTimer;
		private float _rotationFromKeyboardWeight;

		private float _moveSpeedAccelerationFactor     = 3f;
		private float _moveSpeedAccelerationTimeFactor = 7f;

		private readonly List<float>       _relativeDist = new();
		private readonly List<Vector3>     _wayPointList = new(100);
		private readonly List<BezierPoint> _bezierPoints = new(100);

		private readonly List<ValueTuple<Vector3, float>> _bezierCopy = new(100);

		private readonly BezierPosGenerator _bezierPosGenerator = new();

		// Settings
		public readonly CameraTargetProxySetting CameraTargetProxySettingValue = new();
		public readonly CameraObstacleData       CameraObstacleDataValue       = new();
		public readonly ZoomSettingData          ZoomSettingDataValue          = new();
		public readonly RotationBaseSettingData  RotationBaseSettingDataValue  = new();
		public readonly RotationLimitSettingData RotationLimitSettingDataValue = new();
		public readonly CinemachineSettingData   CinemachineSettingDataValue   = new();

		private bool  _isPreventZoom;

		private CinemachineBrain? _brain;
#endregion Fields

		public static FollowCamera Create(CinemachineVirtualCamera? virtualCamera, Camera? camera)
		{
			var followCamera = new FollowCamera(virtualCamera, camera);
			followCamera.LoadTable();
			return followCamera;
		}

		private FollowCamera(CinemachineVirtualCamera? virtualCamera, Camera? camera) : base(virtualCamera, camera, true)
		{
			if (!camera.IsUnityNull())
				_brain = camera!.GetComponent<CinemachineBrain>();
		}

#region Abstract Method Override
		public override void OnStateEnter()
		{
			if (!_isMainCamera)
			{
				C2VDebug.LogWarning(nameof(CameraSystem), "Current state can only enter main camera");
				return;
			}

			if (!CheckHasVirtualCamera()) return;

			InitializeCameraState();

			var cameraManager = CameraManager.Instance;
			cameraManager.OnChangeFov           += SetCameraColliderMinSize;
			cameraManager.OnChangeNearClipPlane += SetCameraColliderMinSize;
			cameraManager.OnChangeAspectRatio   += SetCameraColliderMinSize;
		}

		private void SetCameraColliderMinSize(float _)
		{
			_desiredSetCameraColliderMinSize = true;
		}

		/// <summary>
		/// 카메라의 Collider의 사이즈 = Max(테이블 데이터의 cameraRadius, 카메라와 near프러스텀의 한 버텍스와의 거리)
		/// Virtual Camera간 블랜딩이 진행중인 상황에서는 카메라 Collider의 사이즈를 변경하지 않는다.
		/// (자연스러운 블랜딩이 방해된다)
		/// </summary>
		private void UpdateCameraColliderMinSize()
		{
			if (_camera.IsUnityNull()) return;
			if (_cinemachineThirdPersonFollow.IsUnityNull()) return;
			if (_brain.IsUnityNull() || _brain!.IsBlending) return;

			_desiredSetCameraColliderMinSize = false;

			_camera!.CalculateFrustumCorners(_frustumRect, _camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners);
			_cinemachineThirdPersonFollow!.CameraRadius = Mathf.Max(CameraObstacleDataValue.CameraRadius, _frustumCorners[0].magnitude);
		}

		public override void OnStateExit()
		{
			_avatarTarget = null;
			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager == null) return;

			cameraManager.OnChangeFov           -= SetCameraColliderMinSize;
			cameraManager.OnChangeNearClipPlane -= SetCameraColliderMinSize;
			cameraManager.OnChangeAspectRatio   -= SetCameraColliderMinSize;

			if (!_isMainCamera) return;
			var        mainCamera = cameraManager.MainCamera;
			Vector3    prevPosition;
			Quaternion prevRotation;
			bool       hasMainCamera = !mainCamera.IsUnityNull();

			if (hasMainCamera)
			{
				var prevTransform = mainCamera!.transform;
				prevPosition = prevTransform.position;
				prevRotation = prevTransform.rotation;
			}
			else
			{
				prevPosition = Vector3.zero;
				prevRotation = Quaternion.identity;
			}

			if (!_cameraTargetProxy.IsUnityNull())
				_cameraTargetProxy!.SetTarget(null);

			if (!CheckHasVirtualCamera()) return;

			_virtualCamera!.DestroyCinemachineComponent<CinemachineComponentBase>();

			if (_virtualCamera.gameObject.TryGetComponent(out CinemachineCollider collider))
				_virtualCamera.RemoveExtension(collider);

			if (hasMainCamera)
				_virtualCamera.transform.SetPositionAndRotation(prevPosition, prevRotation);
		}

		public override void Dispose()
		{
			if (!_cameraTargetProxy.IsUnityNull())
				Object.Destroy(_cameraTargetProxy!.gameObject);
		}
#endregion Abstract Method Override

#region Virtual Method Override
		public override void OnChangeTarget(Transform? cameraTarget)
		{
			if (!_isMainCamera)
			{
				base.OnChangeTarget(cameraTarget);
				return;
			}

			if (cameraTarget.IsUnityNull())
			{
				if (!_cameraTargetProxy.IsUnityNull())
					_cameraTargetProxy!.SetTarget(null);
				if (!CheckHasVirtualCamera()) return;

				_virtualCamera!.Follow = null;
				_virtualCamera.LookAt  = null;

				// 씬 변경시 에러나서 임시코드로 막음
				base.OnChangeTarget(cameraTarget);
				return;
			}

			base.OnChangeTarget(cameraTarget);
			InitializeTarget();
		}

		public override void OnUpdate()
		{
			if (!_isMainCamera) return;

			if (_desiredSetCameraColliderMinSize)
				UpdateCameraColliderMinSize();
		}

		public override void OnLateUpdate()
		{
			if (!_isMainCamera) return;
			if (TargetObject.IsReferenceNull()) return;

			UpdateAvatarHeight();
			CameraZoom();
			CameraRotation();
		}

		private void UpdateAvatarHeight()
		{
			if (_avatarTarget == null) return;
			_height = _avatarTarget.Height;
			_height = 1.5f;
		}
#endregion Virtual Method Override

#region Initialize
		private void InitializeCameraState()
		{
			InitializeVirtualCamera();
			AddThirdPersonFollow();
			AddComposer();
			InitializeCameraObstacle();
			InitializeCinemachineSetting();
		}

		private void InitializeVirtualCamera()
		{
			_virtualCamera!.m_Lens.FieldOfView = CinemachineSettingDataValue.ThirdFieldOfViewDefault;
		}

		private void AddThirdPersonFollow()
		{
			_cinemachineThirdPersonFollow = _virtualCamera!.AddCinemachineComponent<Cinemachine3rdPersonFollow>()!;

			_cinemachineThirdPersonFollow.CameraDistance = CinemachineSettingDataValue.ThirdCameraDistanceDefault;

			_targetZoom = CinemachineSettingDataValue.ThirdCameraDistanceDefault;
		}

		private void AddComposer()
		{
			_cinemachineComposer = _virtualCamera!.AddCinemachineComponent<CinemachineComposer>()!;

			_cinemachineComposer.m_HorizontalDamping   = 0;
			_cinemachineComposer.m_VerticalDamping     = 0;
			_cinemachineComposer.m_SoftZoneHeight      = 0;
			_cinemachineComposer.m_SoftZoneWidth       = 0;
			_cinemachineComposer.m_TrackedObjectOffset = Vector3.up * _height;
		}

		private void InitializeCameraObstacle()
		{
			if (_virtualCamera!.TryGetComponent(out CinemachineCollider collider))
				collider!.enabled = false;
			_cinemachineThirdPersonFollow!.CameraCollisionFilter = 0;
			InitializeThirdPersonObstacle();
		}

		private void InitializeThirdPersonObstacle()
		{
			if (_cinemachineThirdPersonFollow.IsUnityNull()) return;
			_cinemachineThirdPersonFollow!.CameraCollisionFilter = CameraManager.Instance.CameraCollisionFilter;
			_cinemachineThirdPersonFollow.CameraRadius           = CameraObstacleDataValue.CameraRadius;
			_cinemachineThirdPersonFollow.DampingIntoCollision   = CameraObstacleDataValue.ObstacleDampingWhenOccluded;
			_cinemachineThirdPersonFollow.DampingFromCollision   = CameraObstacleDataValue.ObstacleDamping;
			UpdateCameraColliderMinSize();
		}

		private void InitializeCinemachineSetting()
		{
			_targetZoom = CinemachineSettingDataValue.ThirdCameraDistanceDefault;

			if (_virtualCamera.IsUnityNull()) return;

			_virtualCamera!.m_Lens.FieldOfView = CinemachineSettingDataValue.ThirdFieldOfViewDefault;

			if (_cinemachineThirdPersonFollow.IsUnityNull()) return;

			_cinemachineThirdPersonFollow!.CameraDistance   = CinemachineSettingDataValue.ThirdCameraDistanceDefault;
			_cinemachineThirdPersonFollow.ShoulderOffset    = CinemachineSettingDataValue.ShoulderOffset;
			_cinemachineThirdPersonFollow.VerticalArmLength = CinemachineSettingDataValue.VerticalArmLength;
		}

		private void InitializeTarget()
		{
			if (!CheckHasVirtualCamera()) return;

			InitializeAvatarTarget();
			_cinemachineCameraTarget = GetCinemachineTarget();

			InitializeVirtualCameraTarget();
			InitializeCameraObstacle();
		}

		private void InitializeAvatarTarget()
		{
			if (TargetObject.IsReferenceNull()) return;

			_height = 0f;
			if (TargetObject!.TryGetComponent(out IAvatarTarget avatarTarget))
			{
				_avatarTarget = avatarTarget;
				_height       = _avatarTarget!.Height;
			}

			_height = 1.5f;
		}

		private void InitializeVirtualCameraTarget()
		{
			_virtualCamera!.Follow = _cinemachineCameraTarget;

			var eulerAngles = TargetObject!.transform.eulerAngles;
			_cinemachineTargetPitch = eulerAngles.x + RotationBaseSettingDataValue.PitchCinemachineTargetDefault;
		}

		private Transform GetCinemachineTarget()
		{
			if (!_cameraTargetProxy.IsUnityNull())
			{
				_cameraTargetProxy!.SetTarget(TargetObject!.transform);
				_cameraTargetProxy.SetTableData(CameraTargetProxySettingValue);

				return _cameraTargetProxy.transform.Find(CameraRotationName)!;
			}

			_cameraTargetProxy = new GameObject { name = $"@{nameof(FollowCameraTargetProxy)}" }.AddComponent<FollowCameraTargetProxy>()!;
			var cameraRotation = new GameObject { name = CameraRotationName }.transform;

			_cameraTargetProxy.SetTarget(TargetObject!.transform, _height);
			_cameraTargetProxy.SetTableData(CameraTargetProxySettingValue);

			var proxyTransform = _cameraTargetProxy.transform;
			cameraRotation.SetParent(proxyTransform);
			cameraRotation.SetPositionAndRotation(proxyTransform.position, proxyTransform.rotation);
			Object.DontDestroyOnLoad(_cameraTargetProxy.gameObject);

			return cameraRotation;
		}

		public override void InitializeValue()
		{
			var eulerAngles = TargetObject!.transform.eulerAngles;

			_targetZoom             = CinemachineSettingDataValue.ThirdCameraDistanceDefault;
			_cinemachineTargetYaw   = eulerAngles.y + RotationBaseSettingDataValue.YawCinemachineTargetDefault;
			_cinemachineTargetPitch = eulerAngles.x + RotationBaseSettingDataValue.PitchCinemachineTargetDefault;

			var targetRotation = Quaternion.Euler(_cinemachineTargetPitch + RotationLimitSettingDataValue.CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
			if (_cinemachineCameraTarget.IsUnityNull()) return;

			var currentRotationVector3 = targetRotation.eulerAngles;
			_cinemachineCameraTarget!.rotation = Quaternion.Euler(currentRotationVector3.x, currentRotationVector3.y, 0f);
		}
#endregion Initialize

#region Process Camera
		private void CameraZoom()
		{
			if (_cinemachineThirdPersonFollow.IsUnityNull()) return;
			if (Mathf.Abs(_cinemachineThirdPersonFollow!.CameraDistance - _targetZoom) < ZoomSettingDataValue.ZoomThreshold) return;

			_cinemachineThirdPersonFollow.CameraDistance =
				Mathf.Lerp(_cinemachineThirdPersonFollow.CameraDistance, _targetZoom, ZoomSettingDataValue.ZoomSmoothFactor * Time.deltaTime);
		}

		private void CameraRotation()
		{
			var eulerAngles = TargetObject!.transform.eulerAngles;

			ProcessRotate();

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw   = MathUtil.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = MathUtil.ClampAngle(_cinemachineTargetPitch, RotationLimitSettingDataValue.BottomClamp, RotationLimitSettingDataValue.TopClamp);

			float smoothFactor           = RotationBaseSettingDataValue.CameraRotationSmoothFactor;
			var   targetRotation         = Quaternion.Euler(_cinemachineTargetPitch + RotationLimitSettingDataValue.CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
			if (_cinemachineCameraTarget.IsUnityNull()) return;
			var   currentRotation        = Quaternion.Slerp(_cinemachineCameraTarget!.rotation, targetRotation, smoothFactor * Time.deltaTime);
			var   currentRotationVector3 = currentRotation.eulerAngles;
			_cinemachineCameraTarget.rotation = Quaternion.Euler(currentRotationVector3.x, currentRotationVector3.y, 0f);
		}

		private void ProcessRotate()
		{
			if (_cameraTargetProxy.IsReferenceNull()) return;

			RotateFromMouse();
			RotateFromKeyboard();
			CalculateRotationFromWaypoint();
			RotateFromWayPoint();
		}

		private void RotateFromMouse()
		{
			_cinemachineTargetYaw   += _inputLook.x * RotationBaseSettingDataValue.ScaleFactorMouseRotation;
			_cinemachineTargetPitch += _inputLook.y * RotationBaseSettingDataValue.ScaleFactorMouseRotation;
		}

		private void RotateFromKeyboard()
		{
			if (_inputMove == Vector2.zero)
			{
				_rotationFromKeyboardWeight = 0f;
				_moveMaintainTimer          = 0f;
				return;
			}

			_rotationFromKeyboardWeight =  _moveMaintainTimer >= 0.9f ? 1f : Mathf.Min(1f, Mathf.Pow(_moveMaintainTimer, _moveSpeedAccelerationFactor));
			_moveMaintainTimer          += Time.deltaTime * _moveSpeedAccelerationTimeFactor;

			var inputDirection = _inputMove.normalized;
			var weight         = Vector3.Dot(Vector2.right, inputDirection) * _rotationFromKeyboardWeight;
			_cinemachineTargetYaw += weight * RotationBaseSettingDataValue.RotationVelocityYawByKeyboard * Time.deltaTime;
		}

		private void CalculateRotationFromWaypoint()
		{
			if (Mathf.Approximately(_progress, 1)) return;
			if (_inputMove != Vector2.zero) return;

			var newPos = GetBezierPos(_progress, eBezierCurveType.CUBIC);
			var newDir = newPos - _lastPos;
			if (newDir == Vector3.zero) return;

			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager == null) return;

			var mainCamera = cameraManager.MainCamera;
			if (mainCamera.IsReferenceNull()) return;

			_lastPos = newPos;

			var targetDirProj = Vector3.ProjectOnPlane(newDir, Vector3.up);
			var fixDir        = mainCamera!.transform.forward;
			var fixDirProj    = Vector3.ProjectOnPlane(fixDir, Vector3.up);
			var angle         = Vector3.Angle(targetDirProj, fixDirProj);

			if (Mathf.Abs(RotationThreshold) <= Mathf.Abs(angle))
			{
				var rotateAroundDestDir = newDir;
				var targetPlayerPos     = _cameraTargetProxy!.transform.position;
				var camToTargetDir      = (targetPlayerPos - mainCamera.transform.position).normalized;
				var offsetY = MathUtil.GetAxisAngle(camToTargetDir, rotateAroundDestDir, Vector3.up);
				_targetRotationYawFromWaypoint += offsetY * RotationBaseSettingDataValue.RotationYawWeightByWaypoint;
			}
		}

		private void RotateFromWayPoint()
		{
			var delta = RotationBaseSettingDataValue.RotationPerSecondDueToWaypoint * Time.deltaTime;
			switch (_targetRotationYawFromWaypoint)
			{
				case > 0.1f:
					_targetRotationYawFromWaypoint -= delta;
					_cinemachineTargetYaw          += delta;
					break;
				case < -0.1f:
					_targetRotationYawFromWaypoint += delta;
					_cinemachineTargetYaw          -= delta;
					break;
				default:
					_targetRotationYawFromWaypoint = 0f;
					break;
			}
		}
#endregion Process Camera

#region InputEvents
		public void OnClickMoveStart() { }

		public void OnClickMoveEnd() { }

		public void OnLookStart() { }

		public void OnLookEnd() { }

		public void OnZoom(float value)
		{
			if (TargetObject.IsReferenceNull()) return;
			if (_cinemachineThirdPersonFollow.IsReferenceNull()) return;

			_targetZoom = Mathf.Clamp(
				_cinemachineThirdPersonFollow!.CameraDistance + (value * ZoomSettingDataValue.ScaleFactorCameraDistance),
				ZoomSettingDataValue.MinCameraDistance,
				ZoomSettingDataValue.MaxCameraDistance
			);
		}

		public void OnMove(Vector2 value)
		{
			_inputMove = value;
		}

		public void OnLook(Vector2 value)
		{
			_inputLook = value;
		}
#endregion InputEvents

#region WayPointHandle
		public void UpdateBezierPoint(IReadOnlyList<Vector3> wayPoints)
		{
			_targetRotationYawFromWaypoint = 0f;

			if (wayPoints.Count == 0)
			{
				return;
			}

			_lastPos = wayPoints[0];
			SelectWayPoints(wayPoints);

			_relativeDist.Clear();
			float relativeDist = 0;
			float maxDist      = 0;
			for (var i = 1; i < _wayPointList.Count; ++i)
			{
				relativeDist += (_wayPointList[i] - _wayPointList[i - 1]).magnitude;
				maxDist      =  relativeDist;
				_relativeDist.Add(relativeDist);
			}

			if (maxDist == 0)
				return;

			_bezierPoints.Clear();
			_bezierPoints.Add(new BezierPoint(_wayPointList[0], 0));
			for (var i = 1; i < _wayPointList.Count; ++i)
				_bezierPoints.Add(new BezierPoint(_wayPointList[i], _relativeDist[i - 1] / maxDist));

			//베지어를 균일한 영역으로 쪼갬
			var   divideUnit           = maxDist >= 2 ? 2 : maxDist;
			var   dividedNum           = (maxDist) / divideUnit;
			var   normalizedDivideUnit = 1f / dividedNum;
			float currentNormal        = 0;
			_bezierCopy.Clear();
			while (currentNormal <= 1)
			{
				currentNormal += normalizedDivideUnit;
				var revisedNormal = Mathf.Clamp01(currentNormal);
				var pos           = GetBezierPos(revisedNormal, eBezierCurveType.QUADRATIC);
				_bezierCopy.Add((pos, revisedNormal));
			}

			_bezierPoints.Clear();
			for (var i = 0; i < _bezierCopy.Count; i++)
				_bezierPoints.Add(new BezierPoint(_bezierCopy[i].Item1, _bezierCopy[i].Item2));
		}

		private Vector3 GetBezierPos(float delta, eBezierCurveType type)
		{
			return _bezierPosGenerator.GetBezierPos(_bezierPoints, delta, type);
		}

		private void SelectWayPoints(IReadOnlyList<Vector3> wayPoints)
		{
			_wayPointList.Clear();
			_wayPointList.Add(_lastPos);

			for (int i = 1; i < wayPoints.Count; i++)
			{
				int j = i - 1;
				if (i == wayPoints.Count - 1)
					_wayPointList.Add(wayPoints[i]);
				else
				{
					while (j <= wayPoints.Count - 2)
					{
						if ((wayPoints[i] - wayPoints[j]).sqrMagnitude - MinDist * MinDist >= 0)
						{
							_wayPointList.Add(wayPoints[j + 1]);
							i = j + 1;
							break;
						}
						j++;
					}
				}
			}
		}

		public void UpdateMoveProgress(float progress)
		{
			_progress = progress;
			if (TargetObject.IsReferenceNull()) return;
			if (_cinemachineThirdPersonFollow.IsReferenceNull()) return;
		}
#endregion WayPointHandle

#region TableData
		private TableFollowCamera?                  _tableFollowCamera;
		private TableFollowCameraObstacleDetection? _tableFollowCameraObstacle;
		private TableFollowCameraTargetProxy?       _tableFollowCameraTargetProxy;
		private TableAvatarControl?                 _tableAvatarControl;

		private void LoadTable()
		{
			_tableFollowCamera            = TableDataManager.Instance.Get<TableFollowCamera>();
			_tableFollowCameraObstacle    = TableDataManager.Instance.Get<TableFollowCameraObstacleDetection>();
			_tableFollowCameraTargetProxy = TableDataManager.Instance.Get<TableFollowCameraTargetProxy>();
			_tableAvatarControl           = TableDataManager.Instance.Get<TableAvatarControl>();

			SetCameraObstacleTable(_tableFollowCameraObstacle);
			SetCameraTargetProxyTable(_tableFollowCameraTargetProxy);
			SetFollowCameraTable(_tableFollowCamera);
			SetAvatarControlTable(_tableAvatarControl);
			SetFollowCameraTable(CameraDefine.DEFAULT_TABLE_INDEX);
		}

		private void SetCameraTargetProxyTable(TableFollowCameraTargetProxy? table)
		{
			if (table == null)
			{
				C2VDebug.LogErrorCategory(nameof(FollowCamera), "Table is null");
				return;
			}

			var data = table.Datas[CameraDefine.DEFAULT_TABLE_INDEX];

			CameraTargetProxySettingValue.SetData(data.SmoothFactor, data.TeleportDistance, data.SkipDistance);
			if (!_cameraTargetProxy.IsUnityNull())
				_cameraTargetProxy!.SetTableData(data);
		}

		public void SetCameraTargetProxySetting(CameraTargetProxySetting data)
		{
			CameraTargetProxySettingValue.SetData(data);
			if (!_cameraTargetProxy.IsUnityNull())
				_cameraTargetProxy!.SetTableData(data);
		}

		private void SetCameraObstacleTable(TableFollowCameraObstacleDetection? table)
		{
			if (table == null)
			{
				C2VDebug.LogErrorCategory(nameof(FollowCamera), "Table is null");
				return;
			}

			var data = table.Datas[CameraDefine.DEFAULT_TABLE_INDEX];

			CameraObstacleDataValue.SetData(data.CameraRadius, data.Damping, data.DampingWhenOccluded);
		}

		public void SetCameraObstacleSetting(float cameraRadius, float damping, float dampingWhenOccluded)
		{
			CameraObstacleDataValue.SetData(cameraRadius, damping, dampingWhenOccluded);
		}

		public void SetCameraObstacleSetting(CameraObstacleData data)
		{
			CameraObstacleDataValue.SetData(data);
			InitializeThirdPersonObstacle();
		}

		private void SetFollowCameraTable(TableFollowCamera? table)
		{
			if (table == null)
			{
				C2VDebug.LogErrorCategory(nameof(FollowCamera), "Table is null");
				return;
			}

			var data = table.Datas[CameraDefine.DEFAULT_TABLE_INDEX];

			ZoomSettingDataValue.SetData(data.ScaleFactorCameraDistance, data.ZoomSmoothFactor, data.ZoomThreshold, data.MinCameraDistance, data.MaxCameraDistance);
			RotationBaseSettingDataValue.SetData(data.ScaleFactorMouseRotation, data.CameraRotationSmoothFactor, data.YawCinemachineTargetDefault, data.PitchCinemachineTargetDefault,
			                                     data.RotationVelocityYawByKeyboard, data.RotationYawWeightByWaypoint, data.RotationPerSecondDueToWaypoint);
			RotationLimitSettingDataValue.SetData(data.TopClamp, data.BottomClamp, data.CameraAngleOverride);
		}

		public void SetFollowCameraTable(int index)
		{
			if (_tableFollowCamera == null) return;
			if (!_tableFollowCamera.Datas!.TryGetValue(index, out var data)) return;

			ZoomSettingDataValue.SetData(data.ScaleFactorCameraDistance, data.ZoomSmoothFactor, data.ZoomThreshold, data.MinCameraDistance, data.MaxCameraDistance);
			RotationBaseSettingDataValue.SetData(data.ScaleFactorMouseRotation, data.CameraRotationSmoothFactor, data.YawCinemachineTargetDefault, data.PitchCinemachineTargetDefault,
			                                     data.RotationVelocityYawByKeyboard, data.RotationYawWeightByWaypoint, data.RotationPerSecondDueToWaypoint);
			RotationLimitSettingDataValue.SetData(data.TopClamp, data.BottomClamp, data.CameraAngleOverride);
			CinemachineSettingDataValue.SetData(data.ThirdFieldOfViewDefault, data.ThirdCameraDistanceDefault, data.ShoulderOffset, data.VerticalArmLength);
		}

		public void SetCameraZoomSetting(float scaleFactorCameraDistance, float zoomSmoothFactor, float zoomThreshold, float minCameraDistance, float maxCameraDistance)
		{
			ZoomSettingDataValue.SetData(scaleFactorCameraDistance, zoomSmoothFactor, zoomThreshold, minCameraDistance, maxCameraDistance);
		}

		public void SetCameraZoomSetting(ZoomSettingData data)
		{
			ZoomSettingDataValue.SetData(data);
		}

		public void SetCameraRotationBaseSetting(float scaleFactorMouseRotation, float cameraRotationSmoothFactor, float yawCinemachineTargetDefault, float pitchCinemachineTargetDefault,
		                                         float rotationVelocityYawByKeyboard, float rotationYawWeightByWaypoint, float rotationPerSecondDueToWaypoint)
		{
			RotationBaseSettingDataValue.SetData(scaleFactorMouseRotation, cameraRotationSmoothFactor, yawCinemachineTargetDefault, pitchCinemachineTargetDefault,
			                                     rotationVelocityYawByKeyboard, rotationYawWeightByWaypoint, rotationPerSecondDueToWaypoint);
		}

		public void SetCameraRotationBaseSetting(RotationBaseSettingData data)
		{
			RotationBaseSettingDataValue.SetData(data);
		}

		public void SetCameraRotationLimitSetting(float topClamp, float bottomClamp, float cameraAngleOverride)
		{
			RotationLimitSettingDataValue.SetData(topClamp, bottomClamp, cameraAngleOverride);
		}

		public void SetCameraRotationLimitSetting(RotationLimitSettingData data)
		{
			RotationLimitSettingDataValue.SetData(data);
		}

		public void SetCameraCinemachineSetting(float thirdFieldOfViewDefault, float thirdCameraDistanceDefault, Vector3 shoulderOffset, float verticalArmLength)
		{
			CinemachineSettingDataValue.SetData(thirdFieldOfViewDefault, thirdCameraDistanceDefault, shoulderOffset, verticalArmLength);
			InitializeCinemachineSetting();
		}

		public void SetCameraCinemachineSetting(CinemachineSettingData data)
		{
			CinemachineSettingDataValue.SetData(data);
			InitializeCinemachineSetting();
		}

		public void SetAvatarControlTable(TableAvatarControl? table)
		{
			if (table == null)
			{
				C2VDebug.LogErrorCategory(nameof(FollowCamera), "Table is null");
				return;
			}

			var data = table.Datas[CameraDefine.DEFAULT_TABLE_INDEX];

			_moveSpeedAccelerationFactor     = data.MoveSpeedAccelerationFactor;
			_moveSpeedAccelerationTimeFactor = data.MoveSpeedAccelerationTimeFactor;
		}
#endregion TableData

#region InternalClass
		[Serializable]
		public class CameraTargetProxySetting
		{
			[field: SerializeField]
			public float SmoothFactor     { get; set; } = 15f;

			[field: SerializeField]
			public float TeleportDistance { get; set; } = 10f;

			[field: SerializeField]
			public float SkipDistance     { get; set; } = 0.1f;

			public void SetData(CameraTargetProxySetting data)
			{
				SmoothFactor     = data.SmoothFactor;
				TeleportDistance = data.TeleportDistance;
				SkipDistance     = data.SkipDistance;
			}

			public void SetData(float smoothFactor, float teleportDistance, float skipDistance)
			{
				SmoothFactor     = smoothFactor;
				TeleportDistance = teleportDistance;
				SkipDistance     = skipDistance;
			}
		}

		[Serializable]
		public class CameraObstacleData
		{
			[field: SerializeField]
			public float CameraRadius { get; set; } = 0.08f;

			[field: SerializeField]
			public float ObstacleDamping { get; set; } = 0.23f;

			[field: SerializeField]
			public float ObstacleDampingWhenOccluded { get; set; } = 0.61f;

			public void SetData(CameraObstacleData data)
			{
				CameraRadius                = data.CameraRadius;
				ObstacleDamping             = data.ObstacleDamping;
				ObstacleDampingWhenOccluded = data.ObstacleDampingWhenOccluded;
			}

			public void SetData(float cameraRadius, float obstacleDamping, float obstacleDampingWhenOccluded)
			{
				CameraRadius                = cameraRadius;
				ObstacleDamping             = obstacleDamping;
				ObstacleDampingWhenOccluded = obstacleDampingWhenOccluded;
			}
		}

		[Serializable]
		public class ZoomSettingData
		{
			[field: SerializeField]
			public float ScaleFactorCameraDistance { get; set; } = 1f;

			[field: SerializeField]
			public float ZoomSmoothFactor { get; set; } = 0.14f;

			[field: SerializeField]
			public float ZoomThreshold { get; set; } = 0.1f;

			[field: SerializeField]
			public float MinCameraDistance { get; set; } = 3.739271f;

			[field: SerializeField]
			public float MaxCameraDistance { get; set; } = 12f;

			public void SetData(ZoomSettingData data)
			{
				ScaleFactorCameraDistance = data.ScaleFactorCameraDistance;
				ZoomSmoothFactor          = data.ZoomSmoothFactor;
				ZoomThreshold             = data.ZoomThreshold;
				MinCameraDistance         = data.MinCameraDistance;
				MaxCameraDistance         = data.MaxCameraDistance;
			}

			public void SetData(float scaleFactorCameraDistance, float zoomSmoothFactor, float zoomThreshold, float minCameraDistance, float maxCameraDistance)
			{
				ScaleFactorCameraDistance = scaleFactorCameraDistance;
				ZoomSmoothFactor          = zoomSmoothFactor;
				ZoomThreshold             = zoomThreshold;
				MinCameraDistance         = minCameraDistance;
				MaxCameraDistance         = maxCameraDistance;
			}
		}

		[Serializable]
		public class RotationBaseSettingData
		{
			[field: SerializeField]
			public float ScaleFactorMouseRotation { get; set; } = 1f;

			[field: SerializeField]
			public float CameraRotationSmoothFactor { get; set; } = 15f;

			[field: SerializeField]
			public float YawCinemachineTargetDefault { get; set; } = 0f;

			[field: SerializeField]
			public float PitchCinemachineTargetDefault { get; set; } = 7f;

			[field: SerializeField]
			public float RotationVelocityYawByKeyboard { get; set; } = 10f;

			[field: SerializeField]
			public float RotationYawWeightByWaypoint { get; set; } = 0.01f;

			[field: SerializeField]
			public float RotationPerSecondDueToWaypoint { get; set; } = 7f;

			public void SetData(RotationBaseSettingData data)
			{
				ScaleFactorMouseRotation       = data.ScaleFactorMouseRotation;
				CameraRotationSmoothFactor     = data.CameraRotationSmoothFactor;
				YawCinemachineTargetDefault    = data.YawCinemachineTargetDefault;
				PitchCinemachineTargetDefault  = data.PitchCinemachineTargetDefault;
				RotationVelocityYawByKeyboard  = data.RotationVelocityYawByKeyboard;
				RotationYawWeightByWaypoint    = data.RotationYawWeightByWaypoint;
				RotationPerSecondDueToWaypoint = data.RotationPerSecondDueToWaypoint;
			}

			public void SetData(float scaleFactorMouseRotation, float cameraRotationSmoothFactor, float yawCinemachineTargetDefault, float pitchCinemachineTargetDefault, float rotationVelocityYawByKeyboard, float rotationYawWeightByWaypoint, float rotationPerSecondDueToWaypoint)
			{
				ScaleFactorMouseRotation       = scaleFactorMouseRotation;
				CameraRotationSmoothFactor     = cameraRotationSmoothFactor;
				YawCinemachineTargetDefault    = yawCinemachineTargetDefault;
				PitchCinemachineTargetDefault  = pitchCinemachineTargetDefault;
				RotationVelocityYawByKeyboard  = rotationVelocityYawByKeyboard;
				RotationYawWeightByWaypoint    = rotationYawWeightByWaypoint;
				RotationPerSecondDueToWaypoint = rotationPerSecondDueToWaypoint;
			}
		}

		[Serializable]
		public class RotationLimitSettingData
		{
			[field: SerializeField]
			public float TopClamp { get; set; } = 70f;

			[field: SerializeField]
			public float BottomClamp { get; set; } = -30f;

			[field: SerializeField]
			public float CameraAngleOverride { get; set; } = 0;

			public void SetData(RotationLimitSettingData data)
			{
				TopClamp            = data.TopClamp;
				BottomClamp         = data.BottomClamp;
				CameraAngleOverride = data.CameraAngleOverride;
			}

			public void SetData(float topClamp, float bottomClamp, float cameraAngleOverride)
			{
				TopClamp            = topClamp;
				BottomClamp         = bottomClamp;
				CameraAngleOverride = cameraAngleOverride;
			}
		}

		[Serializable]
		public class CinemachineSettingData
		{
			[field: SerializeField]
			public float ThirdFieldOfViewDefault { get; set; } = 40f;

			[field: SerializeField]
			public float ThirdCameraDistanceDefault { get; set; } = 6.575283f;

			[field: SerializeField]
			public Vector3 ShoulderOffset { get; set; } = new Vector3(0, 0, 0);

			[field: SerializeField]
			public float VerticalArmLength { get; set; } = -0.4f;

			public void SetData(CinemachineSettingData data)
			{
				ThirdFieldOfViewDefault    = data.ThirdFieldOfViewDefault;
				ThirdCameraDistanceDefault = data.ThirdCameraDistanceDefault;
				ShoulderOffset             = data.ShoulderOffset;
				VerticalArmLength          = data.VerticalArmLength;
			}

			public void SetData(float thirdFieldOfViewDefault, float thirdCameraDistanceDefault, Vector3 shoulderOffset, float verticalArmLength)
			{
				ThirdFieldOfViewDefault    = thirdFieldOfViewDefault;
				ThirdCameraDistanceDefault = thirdCameraDistanceDefault;
				ShoulderOffset             = shoulderOffset;
				VerticalArmLength          = verticalArmLength;
			}
		}
#endregion InternalClass

#region Util
		public void SetPreventZoom(bool isPrevent)
		{
			_isPreventZoom = isPrevent;
		}

		public void SetClampZoomFactor()
		{
			_targetZoom = Mathf.Clamp(_targetZoom, ZoomSettingDataValue.MinCameraDistance, ZoomSettingDataValue.MaxCameraDistance);
		}
#endregion Util
	}
}
