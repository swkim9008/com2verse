/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSeatController.cs
* Developer:	wlemon
* Date:			2023-05-23 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.Rendering;
using Com2Verse.Rendering.Utility;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Protocols;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.Mice
{
	public sealed class MiceSeatController : MonoBehaviour
	{
		public static MiceSeatController Instance { get; private set; } = null;

		[System.Serializable]
		public class Group
		{
			[SerializeField]
			private Transform _root;

			private MiceSeat[]                         _seats;
			private MiceSeat                           _mySeat;
			private List<MiceSeat>                     _candidatesSeats     = new();
			private Dictionary<ActiveObject, MiceSeat> _activeObjectSeatDic = new();

			public MiceSeat   MySeat        => _mySeat;
			public MiceSeat[] Seats         => _seats;
			public int        SeatCount     => _seats.Length;
			public int        LeftSeatCount => _candidatesSeats.Count;

			public void Init()
			{
				_seats = _root.GetComponentsInChildren<MiceSeat>();
				foreach (var seat in _seats)
				{
					seat.SetGroup(this);
					if (seat.IsMySeat)
						_mySeat = seat;
					else
						_candidatesSeats.Add(seat);
				}
			}


			public bool UseSeat(ActiveObject activeObject)
			{
				if (activeObject.IsMine)
				{
					if (_mySeat.IsUnityNull()) return false;

					return UseSeat(activeObject, _mySeat);
				}
				else
				{
					if (_candidatesSeats.Count == 0) return false;

					return UseSeat(activeObject, _candidatesSeats[UnityEngine.Random.Range(0, _candidatesSeats.Count)]);
				}
			}

			public bool ClearSeat(ActiveObject activeObject)
			{
				if (!_activeObjectSeatDic.ContainsKey(activeObject))
					return false;

				var seat = _activeObjectSeatDic[activeObject];
				seat.Clear(activeObject);

				_activeObjectSeatDic.Remove(activeObject);
				_candidatesSeats.Add(seat);
				return true;
			}

			private bool UseSeat(ActiveObject activeObject, MiceSeat seat)
			{
				if (IsUsingSeat(seat))
					return false;

				seat.Use(activeObject);
				_activeObjectSeatDic.Add(activeObject, seat);
				_candidatesSeats.Remove(seat);
				return true;
			}

			private bool IsUsingSeat(MiceSeat seat) => seat.IsUsing;
		}

		[SerializeField]
		private Group[] _groups;

		public int TotalSeatCount
		{
			get
			{
				var count = 0;
				foreach (var group in _groups)
				{
					count += group.SeatCount;
				}

				return count;
			}
		}

		public int LeftSeatCount
		{
			get
			{
				var count = 0;
				foreach (var group in _groups)
				{
					count += group.LeftSeatCount;
				}

				return count;
			}
		}

		private void Awake()
		{
			Instance = this;
			foreach (var group in _groups)
			{
				group.Init();
			}
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}


		public bool UseSeat(ActiveObject activeObject)
		{
			if (_groups == null)
				return false;

			if (activeObject.IsMine)
			{
				foreach (var group in _groups)
				{
					if (group.UseSeat(activeObject))
						return true;
				}
			}
			else
			{
				var targetGroup                      = Array.Find(_groups, (group) => (float)group.LeftSeatCount / (float)group.SeatCount >= 0.5f);
				if (targetGroup == null) targetGroup = Array.Find(_groups, (group) => group.LeftSeatCount > 0);

				if (targetGroup?.UseSeat(activeObject) ?? false) return true;
			}

			return false;
		}

		public bool ClearSeat(ActiveObject activeObject)
		{
			if (_groups == null)
				return false;

			foreach (var group in _groups)
			{
				if (group.ClearSeat(activeObject)) return true;
			}

			return false;
		}

		public Transform FindMySeat()
		{
			foreach (var group in _groups)
			{
				if (!group.MySeat.IsUnityNull()) return group.MySeat.transform;
			}

			return null;
		}

#region Performance Test Dummy
		private GameObject _dummyRoot;

		public void CreateDummyAvatars(float fillRate)
		{
			var fashionItemTableData = TableDataManager.Instance.Get<TableFashionItem>();

			if (!_dummyRoot.IsUnityNull())
			{
				DestroyImmediate(_dummyRoot);
				_dummyRoot = null;
			}

			int value = (int)(fillRate * 100.0f);
			if (value != 0)
			{
				_dummyRoot                  = new GameObject("DummyRoot");
				_dummyRoot.transform.parent = transform;
				_dummyRoot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				_dummyRoot.transform.localScale = Vector3.one;

				var seatTransforms = new List<Transform>();

				foreach (var group in _groups)
				{
					var seats = group.Seats;
					seatTransforms.AddRange(
						from seat in seats
						select seat.transform);
				}

				CreateAll().Forget();

				async UniTask CreateAll()
				{
					foreach (var seatTransform in seatTransforms)
					{
						int pickValue = UnityEngine.Random.Range(1, 101);
						if (pickValue <= value)
						{
							await Create(seatTransform);
						}
					}
				}

				async UniTask Create(Transform seatTransform)
				{
					var avatarInfo = AvatarInfo.CreateTestInfo(true, false);
					var avatar     = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.WORLD, seatTransform.position, (int)Define.eLayer.CHARACTER);
					avatar.gameObject.SetActive(true);
					avatar.transform.parent = _dummyRoot.transform;
					avatar.transform.SetPositionAndRotation(seatTransform.position, seatTransform.rotation);
					avatar.GetComponent<GhostAvatarController>().IsTestAvatar    = true;
					avatar.GetComponent<GhostAvatarController>().ModelTypeString = avatarInfo.AvatarType.ToString();

					avatar.AvatarAnimatorController!.TrySetInteger(AnimationDefine.HashState, (int)Protocols.CharacterState.Sit);
				}
			}
		}
#endregion
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(MiceSeatController))]
	public class MiceSeatManagerEditor : Editor
	{
		private MiceSeatController _seatController;

		public void OnEnable()
		{
			_seatController = target as MiceSeatController;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
#endif
}
