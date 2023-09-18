/*===============================================================
* Product:		Com2Verse
* File Name:	FixedCameraManager.cs
* Developer:	eugene9721
* Date:			2022-06-09 16:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Cinemachine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com2Verse.CameraSystem
{
	public sealed class FixedCameraManager : Singleton<FixedCameraManager>
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private FixedCameraManager() { }

		private readonly List<FixedCameraJig> _cameras               = new();
		private          float                _sequentialCameraTimer = 3f;

		private int _maxPrevIndexListLength = 10;

		private readonly LinkedList<int>  _prevIndexList = new();
		private          CinemachineBrain _brain;

		private int  _currCameraIndex;
		private bool _isOnSequentialCameraSwitcher;

		private bool _isInitialized;

		private TableFixedCamera _tableFixedCamera;

		public int ActivePriority   { get; private set; } = 15;
		public int InactivePriority { get; private set; } = 0;

		private bool IsFixedCameraState => CameraManager.Instance.CurrentState == eCameraState.FIXED_CAMERA;

		public IList<FixedCameraJig> FixedCameraJigs => _cameras.AsReadOnly();

		public string CurrentJigKey
		{
			get
			{
				if (_cameras == null)
					return null;

				_cameras.TryGetAt(_currCameraIndex, out var camera);

				if (camera.IsUnityNull())
					return null;

				return camera!.JigKey;
			}
		}

		private FixedCameraJig _activeCamera;

		public FixedCameraJig ActiveCamera
		{
			get => _activeCamera;
			private set
			{
				if (!_activeCamera.IsReferenceNull())
					_activeCamera.OnInactive();

				_activeCamera = value;

				if (!_activeCamera.IsReferenceNull())
					_activeCamera.OnActive();
			}
		}

		public CinemachineBrain Brain
		{
			get
			{
				if (!_brain.IsReferenceNull()) return _brain;

				var mainCamera = CameraManager.Instance.MainCamera;
				if (mainCamera.IsUnityNull()) return null;

				_brain = mainCamera.GetComponent<CinemachineBrain>();
				return _brain;
			}
		}

		public void Initialize()
		{
			UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloadedEventHandler;
			UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloadedEventHandler;

			if (_tableFixedCamera == null)
			{
				LoadSingleDataTable();
			}
		}

		private void OnSceneUnloadedEventHandler(Scene scene)
		{
			UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloadedEventHandler;
			Disable();
		}

		public void Disable()
		{
			Clear();
		}

		private void Clear()
		{
			_prevIndexList.Clear();
			ActiveCamera = null;
			_brain       = null;
		}

#region TableData
		private void LoadSingleDataTable()
		{
			_tableFixedCamera = TableDataManager.Instance.Get<TableFixedCamera>();
			ApplyTable();
		}

		private void ApplyTable()
		{
			ActivePriority   = _tableFixedCamera.Datas[CameraDefine.DEFAULT_TABLE_INDEX].ActivePriority;
			InactivePriority = _tableFixedCamera.Datas[CameraDefine.DEFAULT_TABLE_INDEX].InactivePriority;
		}
#endregion TableData

#region CameraBlend
		public void SetDefaultBlend(CinemachineBlendDefinition.Style style, float time)
		{
			SetDefaultBlendStyle(style);
			SetDefaultBlendTime(time);
		}

		public void SetDefaultBlendStyle(CinemachineBlendDefinition.Style style)
		{
			if (ReferenceEquals(Brain, null)) return;

			if (style == CinemachineBlendDefinition.Style.Custom && _brain.m_DefaultBlend.m_CustomCurve == null)
			{
				C2VDebug.LogErrorCategory(nameof(CameraSystem), "Custom Blend Type needs CustomCurve");
			}

			_brain.m_DefaultBlend.m_Style = style;
		}

		public void SetDefaultBlendTime(float time)
		{
			if (ReferenceEquals(Brain, null)) return;
			_brain.m_DefaultBlend.m_Time = time;
		}
#endregion CameraBlend

#region SwitchCamera
		public void SwitchNearestCamera([CanBeNull] string key = null)
		{
			var index = -1;
			if (string.IsNullOrEmpty(key))
			{
				index = GetNearestCameraIndex();
			}
			else
			{
				index = GetNearestCameraIndex(key);
			}

			if (index == -1) return;
			SwitchCamera(index);
		}

		public void SwitchCamera(FixedCameraJig sourceCamera, bool isPrev = false)
		{
			if (!IsFixedCameraState) return;

			bool hasSwitchCamera  = false;
			var  prevActiveCamera = ActiveCamera;
			CameraManager.Instance.ChangeTarget(sourceCamera.transform);
			ActiveCamera = sourceCamera;

			for (int index = 0; index < _cameras.Count; ++index)
			{
				if (_cameras[index] != sourceCamera && ActiveCamera == prevActiveCamera)
				{
					SetCameraIndex(index, isPrev);
				}
				else if (_cameras[index] == sourceCamera)
				{
					hasSwitchCamera = true;
				}
			}

			if (!hasSwitchCamera)
			{
				C2VDebug.LogWarningCategory(nameof(CameraSystem), "this camera is not include camera list");
			}
		}

		public void SwitchCamera(int index, bool isPrev = false)
		{
			if (!IsFixedCameraState) return;

			if (index < 0 || index >= _cameras.Count)
			{
				C2VDebug.LogWarningCategory(nameof(CameraSystem), "index out of range");
				return;
			}

			CameraManager.Instance.ChangeTarget(_cameras[index].transform);
			ActiveCamera = _cameras[index];
			SetCameraIndex(index, isPrev);
		}

		public void SwitchNextCamera()
		{
			if (!IsFixedCameraState) return;

			int nextIndex = (_currCameraIndex + 1) % _cameras.Count;
			SwitchCamera(nextIndex);
		}

		public void SwitchPrevCamera()
		{
			if (!IsFixedCameraState) return;

			if (_prevIndexList.Count <= 0)
			{
				C2VDebug.LogWarningCategory(nameof(CameraSystem), "이전 카메라가 없습니다");
				return;
			}

			int prevIndex = _prevIndexList.Last.Value;
			_prevIndexList.RemoveLast();
			SwitchCamera(prevIndex, true);
		}

		private void SetCameraIndex(int index, bool isPrev)
		{
			int prevCameraIndex = _currCameraIndex;
			_currCameraIndex = index;
			if (isPrev) return;

			if (_prevIndexList.Count > _maxPrevIndexListLength)
			{
				_prevIndexList.RemoveFirst();
			}

			_prevIndexList.AddLast(prevCameraIndex);
		}
#endregion SwitchCamera

#region SequentialCameraSwitcher
		public void OnSequentialCameraSwitcher(bool value)
		{
			if (value && !_isOnSequentialCameraSwitcher)
			{
				SequentialCameraSwitcher().Forget();
			}

			_isOnSequentialCameraSwitcher = value;
		}

		private async UniTask SequentialCameraSwitcher()
		{
			do
			{
				if (_cameras == null)
				{
					C2VDebug.LogErrorCategory(nameof(CameraSystem), "카메라 리스트가 필요합니다!");
					return;
				}

				SwitchNextCamera();
				await UniTask.Delay(TimeSpan.FromSeconds(_sequentialCameraTimer), DelayType.Realtime);
				if (!_isOnSequentialCameraSwitcher) return;
			}
			while (_isOnSequentialCameraSwitcher);
		}
#endregion SequentialCameraSwitcher

#region HELPER
		public bool IsActiveCamera(FixedCameraJig sourceCamera) =>
			sourceCamera == ActiveCamera;

		public void Register(FixedCameraJig sourceCamera)
		{
			_cameras.Add(sourceCamera);
		}

		public void Unregister(FixedCameraJig sourceCamera)
		{
			_cameras.Remove(sourceCamera);
		}

		public int GetNearestCameraIndex()
		{
			if (!CameraManager.InstanceExists) return 0;
			var userCharacter = CameraManager.Instance.UserAvatarTarget?.AvatarTarget;
			if (userCharacter.IsUnityNull()) return 0;

			int   nearestUserIndex = 0;
			float nearestDistance  = float.MaxValue;
			for (int index = 0; index < _cameras.Count; ++index)
			{
				var distance = Vector3.SqrMagnitude(_cameras[index].transform.position - userCharacter!.transform.position);
				if (distance < nearestDistance)
				{
					nearestDistance  = distance;
					nearestUserIndex = index;
				}
			}

			return nearestUserIndex;
		}

		public int GetNearestCameraIndex(string key)
		{
			if (!CameraManager.InstanceExists) return 0;
			var userCharacter = CameraManager.Instance.UserAvatarTarget?.AvatarTarget;
			if (userCharacter.IsUnityNull()) return 0;

			int   nearestUserIndex = -1;
			float nearestDistance  = float.MaxValue;
			for (int index = 0; index < _cameras.Count; ++index)
			{
				if (_cameras[index].JigKey != key) continue;
				var distance = Vector3.SqrMagnitude(_cameras[index].transform.position - userCharacter!.transform.position);
				if (distance < nearestDistance)
				{
					nearestDistance  = distance;
					nearestUserIndex = index;
				}
			}

			if (nearestUserIndex == -1)
				C2VDebug.LogWarningCategory(nameof(CameraSystem), "this key is not include camera list");

			return nearestUserIndex;
		}
#endregion HELPER
	}
}
