/*===============================================================
* Product:		Com2Verse
* File Name:	MessengerAvatarPlacementTool.cs
* Developer:	jhkim
* Date:			2023-08-17 13:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using UnityEngine;
using Random = UnityEngine.Random;

namespace Com2Verse
{
	public sealed class MessengerAvatarPlacementTool : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] private Rect[] _areas;
		[SerializeField] private GameObject _avatarObj;
		[SerializeField] private float _avatarRadius = .5f;

		[Header("Gizmo Options")]
		[SerializeField] private bool _displayGizmo = true;
		[SerializeField] private bool _displayArea = true;
		[SerializeField] private bool _displayAvatarRadius = true;
		[SerializeField] private bool _displayWire;

		[NotNull] private List<GameObject> _avatars = new();

		public IReadOnlyList<GameObject> Avatars => _avatars;

		private void OnDrawGizmos()
		{
			if (!_displayGizmo) return;
			if (_areas == null) return;

			if (_displayArea)
			{
				foreach (var area in _areas)
				{
					var lb = new Vector3(area.xMin, 0, area.yMin);
					var size = new Vector3(area.size.x, 0, area.size.y);
					var center = new Vector3(lb.x + area.width * .5f, 0.01f, lb.z + area.height * .5f);
					Gizmos.color = Color.red;

					if (_displayWire)
						Gizmos.DrawWireCube(center, size);
					else
						Gizmos.DrawCube(center, size);
				}
			}

			if (_displayAvatarRadius && _avatarRadius > 0f)
			{
				foreach (var avatar in _avatars)
				{
					if (avatar == null)
						ValidateAvatars();

					var center = avatar.transform.position;
					Gizmos.color = Color.blue;
					if (_displayWire)
						Gizmos.DrawWireSphere(center, _avatarRadius);
					else
						Gizmos.DrawSphere(center, _avatarRadius);
				}
			}
		}

		private Vector3 GetRandomPosition()
		{
			if (_areas == null || _areas.Length == 0) return Vector3.zero;

			var areaIdx = Random.Range(0, _areas.Length);
			var rect = _areas[areaIdx];
			var x = Random.Range(rect.xMin, rect.xMax);
			var z = Random.Range(rect.yMin, rect.yMax);
			return new Vector3(x, 0, z);
		}
		public void CreateAvatar(int count = 1)
		{
			var positions = CreatePositionsAvoidIntersect(count);

			foreach (var pos in positions)
			{
				var newObj = Instantiate(_avatarObj);
				newObj.name = $"[{pos.x}, {pos.z}]";
				newObj.transform.position = pos;
				_avatars.Add(newObj);
			}
		}

		private Vector3[] CreatePositionsAvoidIntersect(int count)
		{
			var results = new List<Vector3>();
			var checkItems = new List<Vector3>();
			if (_avatars.Count > 0)
				checkItems.AddRange(_avatars.Select(obj => obj.transform.position));

			var retryMax = 10000;
			var retry = 0;
			while (results.Count < count)
			{
				var newPos = GetRandomPosition();
				if (checkItems.Any(pos => IsIntersect(pos, newPos)))
				{
					if (retry >= retryMax) break;

					retry++;
					continue;
				}

				retry = 0;
				results.Add(newPos);
				checkItems.Add(newPos);
			}
			return results.ToArray();

			bool IsIntersect(Vector3 left, Vector3 right)
			{
				var x = left.x - right.x;
				var z = left.z - right.z;
				var dist = Mathf.Sqrt(x * x + z * z);
				return dist <= _avatarRadius * 2;
			}
		}
		public void CopyAreas()
		{
			if (_areas == null || _areas.Length == 0) return;

			var sb = new StringBuilder();
			sb.AppendLine("영역 | Rect");
			sb.AppendLine("--- | ---");

			for (var i = 0; i < _areas.Length; ++i)
			{
				var area = _areas[i];
				sb.AppendLine($"{i + 1} | {area}");
			}

			GUIUtility.systemCopyBuffer = sb.ToString();
			EditorUtility.DisplayDialog("알림", "범위 정보가 클립보드에 저장되었습니다.", "확인");
		}

		public void CopyAvatars()
		{
			if (_avatars.Count == 0) return;

			var sb = new StringBuilder();
			sb.AppendLine("순번 | 좌표");
			sb.AppendLine("--- | ---");

			for (var i = 0; i < _avatars.Count; i++)
			{
				var avatar = _avatars[i];
				if (avatar == null)
				{
					ValidateAvatars();
					continue;
				}

				sb.AppendLine($"{i + 1} | {avatar.transform.position}");
			}

			GUIUtility.systemCopyBuffer = sb.ToString();
			EditorUtility.DisplayDialog("알림", "아바타 배치 정보가 클립보드에 저장되었습니다.", "확인");
		}

		public void ClearAvatar(int idx)
		{
			if (idx < 0) return;

			if (idx < _avatars.Count)
				DestroyImmediate(_avatars[idx]);
		}

		public bool ClearAvatars()
		{
			if (EditorUtility.DisplayDialog("알림", "배치된 아바타를 삭제하시겠습니까?", "예", "아니오"))
			{
				foreach (var avatar in _avatars)
					DestroyImmediate(avatar);
				_avatars.Clear();
				return true;
			}

			return false;
		}

		public void ValidateAvatars()
		{
			for (var i = 0; i < _avatars.Count;)
			{
				if (_avatars[i] == null)
				{
					_avatars.RemoveAt(i);
					continue;
				}

				i++;
			}
		}
#endif // UNITY_EDITOR
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(MessengerAvatarPlacementTool))]
	public class MessengerAvatarPlacementToolEditor : Editor
	{
		private MessengerAvatarPlacementTool _target;
		private SerializedProperty _areas;
		private int _deleteAvatarIdx = -1;
		private void Awake()
		{
			_target = target as MessengerAvatarPlacementTool;
			_areas = serializedObject.FindProperty("_areas");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("한개 추가"))
				{
					_target.CreateAvatar();
					FocusItem(_target.Avatars.Last());
				}

				if (GUILayout.Button("랜덤 30 개 생성"))
				{
					if (_target.ClearAvatars())
						_target.CreateAvatar(30);
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("범위 정보 복사"))
					_target.CopyAreas();
				if (GUILayout.Button("아바타 배치 좌표 복사"))
					_target.CopyAvatars();
			}

			if (GUILayout.Button("아바타 모두 삭제"))
				_target.ClearAvatars();
			// if (GUILayout.Button("아바타 오브젝트 유효성 검증"))
			// 	_target.ValidateAvatars();

			for (var i = 0; i < _target.Avatars.Count; i++)
			{
				var avatar = _target.Avatars[i];
				if (avatar == null)
				{
					_target.ValidateAvatars();
					continue;
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField($"[{i + 1}] {avatar.transform.position}");
					if (GUILayout.Button("P"))
						FocusItem(avatar);

					if (GUILayout.Button("X"))
					{
						if (EditorUtility.DisplayDialog("알림", "배치된 아바타를 삭제하시겠습니까?", "예", "아니오"))
							_deleteAvatarIdx = i;
					}
				}
			}

			if (_deleteAvatarIdx != -1)
			{
				var delete = _deleteAvatarIdx;
				_deleteAvatarIdx = -1;
				_target.ClearAvatar(delete);
			}
		}

		void FocusItem(GameObject targetItem)
		{
			Selection.activeGameObject = targetItem;
			SceneView.lastActiveSceneView.FrameSelected();
		}
	}
#endif // UNITY_EDITOR
}
