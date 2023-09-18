/*===============================================================
* Product:		Com2Verse
* File Name:	BaseObjectStatePublisher.cs
* Developer:	haminjeong
* Date:			2023-02-03 10:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Diagnostics.CodeAnalysis;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using UnityEngine;
using Cysharp.Text;

namespace Com2Verse.Network
{
	public class BaseObjectStatePublisher
	{
		protected BaseMapObject                             MyObject     { get; set; }
		protected Protocols.GameMechanic.ObjectStateRequest CurrentState { get; set; }

		private static readonly int   CheckLoopTime     = 200;
		private static readonly float VelocityThreshold = 10f;

		private long _checkTime = 0;

#region ObjectState
		protected virtual Protocols.GameMechanic.ObjectStateRequest GetStateFromObject()
		{
			Vector3    prevPos          = Vector3.negativeInfinity;
			Vector3    currentPos       = MyObject.transform.position;
			Vector3    delta            = Vector3.zero;
			Vector3    offset           = MapObjectUtil.GetCellOffset(MyObject.CellIndex);
			Vector2Int deltaOfCellIndex = Vector2Int.zero;
			Vector3    currentCellPos   = currentPos - offset;
			if (CurrentState is { PhysicsState: not null })
			{
				prevPos = new Vector3(CurrentState.PhysicsState.Position.X * 0.01f,
				                      CurrentState.PhysicsState.Position.Y * 0.01f,
				                      CurrentState.PhysicsState.Position.Z * 0.01f) + offset;

				delta = (currentPos - prevPos) * 1000;

				var prevVelocity = new Vector3(CurrentState.PhysicsState.Velocity.X,
				                               CurrentState.PhysicsState.Velocity.Y,
				                               CurrentState.PhysicsState.Velocity.Z);
				delta = delta.sqrMagnitude > VelocityThreshold ? delta : prevVelocity;

				deltaOfCellIndex = new Vector2Int(Mathf.FloorToInt(currentCellPos.x / MapController.Instance.CellWidth),
				                                  Mathf.FloorToInt(currentCellPos.z / MapController.Instance.CellHeight));
				if (!deltaOfCellIndex.Equals(Vector2Int.zero)) // 셀을 벗어남
				{
					MyObject.CellIndex += deltaOfCellIndex;
					offset             =  MapObjectUtil.GetCellOffset(MyObject.CellIndex);
					currentCellPos     =  currentPos - offset;
				}
			}

			Protocols.GameMechanic.ObjectStateRequest state = new()
			{
				Serial = MyObject.ObjectID,
				PhysicsState = new()
				{
					Position = new()
					{
						X = (int)(currentCellPos.x * 100),
						Y = (int)(currentCellPos.y * 100),
						Z = (int)(currentCellPos.z * 100),
					},
					Rotation = new()
					{
						X = (int)(MyObject.transform.rotation.x * 200),
						Y = (int)(MyObject.transform.rotation.y * 200),
						Z = (int)(MyObject.transform.rotation.z * 200),
						W = (int)(MyObject.transform.rotation.w * 200),
					},
					Velocity = new()
					{
						X = (int)delta.x,
						Y = (int)delta.y,
						Z = (int)delta.z,
					},
					CellIndex = new()
					{
						X = (int)MyObject.CellIndex.x,
						Y = (int)MyObject.CellIndex.y,
					},
				},
			};

			return state;
		}

		[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
		protected virtual Protocols.Changed IsEqualToCurrentState(ref Protocols.GameMechanic.ObjectStateRequest targetState)
		{
			Protocols.Changed result = Protocols.Changed.None;

			var physicsState = CurrentState?.PhysicsState;
			if (physicsState == null)
				return result;

			if (!physicsState.Position.Equals(targetState.PhysicsState.Position))
			{
				result |= Protocols.Changed.Position;
				if (!physicsState.Velocity.Equals(targetState.PhysicsState.Velocity))
					result |= Protocols.Changed.Velocity;
			}

			if (!physicsState.Rotation.Equals(targetState.PhysicsState.Rotation))
				result |= Protocols.Changed.Rotation;

			if (!physicsState.CellIndex.Equals(targetState.PhysicsState.CellIndex) || result.HasFlag(Protocols.Changed.Position))
				result |= Protocols.Changed.Cell;

			return result;
		}

		// ReSharper disable once InconsistentNaming
		private Protocols.GameMechanic.ObjectStateRequest CreateOSR() => CurrentState;

		/// <summary>
		/// OSR을 클라이언트에서 서버로 업데이트 해야 하는 상태가 되었을 때, 이전까지 서버에서 업데이트 되던 상태를 업데이트 합니다
		/// </summary>
		public void UpdateCurrentState()
		{
			if (MyObject.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(GetType().Name, "MyObject is null");
				return;
			}

			var state = MyObject.UpdatedState;

			if (CurrentState == null)
			{
				CurrentState = new()
				{
					Serial = MyObject.ObjectID,
					Changed = 0,
				};
				return;
			}

			var offset  = MapObjectUtil.GetCellOffset(MyObject.CellIndex);
			var position = state.Position - offset;
			CurrentState.PhysicsState = new Protocols.PhysicsState()
			{
				Position = new()
				{
					X = (int)(position.x * 100),
					Y = (int)(position.y * 100),
					Z = (int)(position.z * 100),
				},
				Rotation = new()
				{
					X = (int)(state.Orientation.x * 200),
					Y = (int)(state.Orientation.y * 200),
					Z = (int)(state.Orientation.z * 200),
					W = (int)(state.Orientation.w * 200),
				},
				Velocity = new()
				{
					X = (int)state.Velocity.x * 1000,
					Y = (int)state.Velocity.y * 1000,
					Z = (int)state.Velocity.z * 1000,
				},
				CellIndex = new()
				{
					X = (int)MyObject.CellIndex.x,
					Y = (int)MyObject.CellIndex.y,
				},
			};
			CurrentState.CharacterState = (Protocols.CharacterState)MyObject.CharacterState;
		}
#endregion

		public void SetMyObject(BaseMapObject obj)
		{
			MyObject     = obj;
			UpdateCurrentState();
			CurrentState = GetStateFromObject();
		}

		public void ClearMyObject()
		{
			MyObject       = null;
			CurrentState   = null;
		}

		public void CheckStateDirty(bool isImmediately = false)
		{
			if (MyObject.IsReferenceNull() || CurrentState == null) return;
			if (MyObject.IsNavigating) return;

			if (!isImmediately && ServerTime.Time < _checkTime) return;
			_checkTime = ServerTime.Time + CheckLoopTime;

			var target = GetStateFromObject();
			target.Changed = IsEqualToCurrentState(ref target);
			if (target.Changed == Protocols.Changed.None) return;

			CurrentState = target;
			// _publishVersion++;
			var osr = CreateOSR();

			if (osr == null)
			{
				C2VDebug.LogErrorCategory(nameof(BaseObjectStatePublisher), "osr is null");
				return;
			}

			// C2VDebug.LogCategory(nameof(BaseObjectStatePublisher), ToStringOsr(osr));
			NetworkManager.Instance.Send(osr, Protocols.GameMechanic.MessageTypes.ObjectStateRequest);
		}

#if UNITY_EDITOR && !METAVERSE_RELEASE
		private string ToStringOsr(Protocols.GameMechanic.ObjectStateRequest osr)
		{
			var sb = ZString.CreateStringBuilder();
			if (osr.Changed.HasFlag(Protocols.Changed.Cell))
			{
				sb.Append("changed Cell\n");
				sb.Append(osr.PhysicsState.CellIndex);
				sb.Append("\n");
			}

			if (osr.Changed.HasFlag(Protocols.Changed.Position))
			{
				sb.Append("changed Position\n");
				sb.Append(osr.PhysicsState.Position);
				sb.Append("\n");
			}

			if (osr.Changed.HasFlag(Protocols.Changed.Velocity))
			{
				sb.Append("changed Velocity\n");
				sb.Append(osr.PhysicsState.Velocity);
				sb.Append("\n");
			}

			if (osr.Changed.HasFlag(Protocols.Changed.Rotation))
			{
				sb.Append("changed Rotation\n");
				sb.Append(osr.PhysicsState.Rotation);
				sb.Append("\n");
			}

			if (osr.Changed.HasFlag(Protocols.Changed.CharacterState))
			{
				sb.Append("changed CharacterState ");
				sb.Append(osr.CharacterState);
				sb.Append("\n");
			}

			return sb.ToString();
		}
#endif
	}
}
