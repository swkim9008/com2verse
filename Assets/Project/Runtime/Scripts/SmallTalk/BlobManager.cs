/*===============================================================
 * Product:		Com2Verse
 * File Name:	BlobManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-10 11:32
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Rendering.Interactable;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.SmallTalk
{
	public sealed class BlobManager : Singleton<BlobManager>, IDisposable
	{
		private readonly BlobData _blobData = new();

		private readonly Dictionary<long, ConnectionNode> _blobNodes = new();

		private BlobConnection? _blobConnection;

		private CancellationTokenSource? _blobWeightUpdateToken;

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private BlobManager() { }

		public async UniTask TryInitializeAsync()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				_blobData.LoadTable();
				await LoadBlobConnection(_blobData);
			}
			IsInitializing = false;
			IsInitialized  = true;
		}

		private async UniTask LoadBlobConnection(BlobData data)
		{
			var blobPrefab     = await C2VAddressables.LoadAssetAsync<GameObject>(Define.BlobPrefabPath)!.ToUniTask();
			var blobGameObject = Object.Instantiate(blobPrefab, Vector3.zero, Quaternion.identity)!;
			Object.DontDestroyOnLoad(blobGameObject);

			_blobConnection = blobGameObject.GetComponent<BlobConnection>()!;

			_blobConnection._maxHeadRadius        = data.MaxHeadRadius;
			_blobConnection._maxBodyRadius        = data.MaxBodyRadius;
			_blobConnection._softColloidMovement  = data.SoftColloidMovement;
			_blobConnection._softColloidMoveSpeed = data.SoftColloidMoveSpeed;
		}

		public void Dispose()
		{
			Clear();

			_blobConnection.DestroyGameObject();
			_blobConnection = null;
		}

		public void Clear()
		{
			_blobWeightUpdateToken?.Cancel();
			_blobWeightUpdateToken?.Dispose();
			_blobWeightUpdateToken = null;

			if (_blobConnection.IsUnityNull())
				return;

			foreach (var node in _blobNodes.Values)
			{
				_blobConnection!.Disconnect(node);
			}

			_blobNodes.Clear();
		}

		public void Connect(long ownerId)
		{
			if (_blobNodes.ContainsKey(ownerId))
				return;

			if (_blobConnection.IsUnityNull())
				return;

			if (!MapController.InstanceExists)
				return;

			var targetCharacter = MapController.Instance.GetObjectByUserID(ownerId) as ActiveObject;
			if (targetCharacter.IsReferenceNull())
			{
				C2VDebug.LogWarningCategory(nameof(BlobManager), "TargetCharacter is null");
				return;
			}

			var node = _blobConnection!.Connect(targetCharacter!.transform)!;
			_blobNodes.Add(ownerId, node);
			SetNodeWeight(ownerId, node);
		}

		public void Disconnect(long ownerId)
		{
			if (!_blobNodes.TryGetValue(ownerId, out var node))
				return;

			if (_blobConnection.IsUnityNull())
				return;

			_blobNodes.Remove(ownerId);
			_blobConnection!.Disconnect(node!);
		}

		private void SetNodeWeight(long ownerId, ConnectionNode node)
		{
			if (ownerId == User.Instance.CurrentUserData.ID)
			{
				node.SetWeight(1.0f);
			}
			else
			{
				_blobWeightUpdateToken ??= new CancellationTokenSource();
				UpdateNodeWeightAsync(node, _blobWeightUpdateToken).Forget();
			}
		}

		private static async UniTask UpdateNodeWeightAsync(ConnectionNode node, CancellationTokenSource token)
		{
			float weight = 0;

			while (await UniTaskHelper.Delay(Define.BlobNodeWeightUpdateInterval, token))
			{
				weight += Define.BlobNodeWeightIncreaseRate;

				node.SetWeight(weight);

				if (weight >= 1)
					return;
			}
		}
	}
}
