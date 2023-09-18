/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCreatorQueue.cs
* Developer:	NGSG
* Date:			2023-08-01 18:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Avatar;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using Utils;

namespace Com2Verse
{
	public class AvatarLoadInfo : IComparable <AvatarLoadInfo>
	{
		 public AvatarController _avatarController;
		 public int _distance;
		 public Func<AvatarController, Action<GameObject>, CancellationTokenSource, UniTask> _onProcess;
		 //public Action<long, BaseMapObject> _onCompleted;
		 public Action<GameObject> _onCompleted;

		 public AvatarLoadInfo(AvatarController avatarController, int distance, Func<AvatarController, Action<GameObject>, CancellationTokenSource, UniTask> onProcess, Action<GameObject> onCompleted)
		 {
			 _avatarController = avatarController;
			 _distance = distance;
			 _onProcess = onProcess;
			 _onCompleted = onCompleted;
		 }

		 public int CompareTo(AvatarLoadInfo other)
		 {
			 // 같으면 거리별로 가까운거 부터 로딩한다
			 float a = this._distance;
			 float b = other._distance;
			 if (a > b)
				 return 1;
			 else if (a < b)
				 return -1;
			 else
				 return 0;
		 }
	}
	
	public sealed class AvatarCreatorQueue : Singleton<AvatarCreatorQueue>
	{
		//private PriorityQueue<AvatarLoadInfo, int> _InstantiateWorkQueue = new(Comparer<int>.Create((x, y) => x.CompareTo(y)));
		private PriorityQueueRemovable<AvatarLoadInfo> _InstantiateWorkQueue = new PriorityQueueRemovable<AvatarLoadInfo>();

		private GameObject _myAvatar;
		public Vector3 MyAvatarPosition => _myAvatar == null ? Vector3.zero : _myAvatar.transform.position;

		public AvatarLoadInfo CurrentAvatarLoadInfo { private set; get; }

		private CancellationTokenSource? _tokenSource;
		
		private AvatarCreatorQueue()
		{
			_tokenSource ??= new CancellationTokenSource();

			Application.quitting -= Dispose;
			Application.quitting += Dispose;
			
			Process();
		}

		public void SetMyAvatar(GameObject obj)
		{
			_myAvatar = obj;
		}
		
		public void Clear()
		{
			CurrentAvatarLoadInfo = null;
			_myAvatar = null;
			
			_InstantiateWorkQueue.Clear();
		}

		public void Enqueue(AvatarLoadInfo avatarLoadInfo)
		{
			if(_InstantiateWorkQueue == null || avatarLoadInfo == null)
				return;

			//_InstantiateWorkQueue.Enqueue(avatarLoadInfo, avatarLoadInfo._distance);
			_InstantiateWorkQueue.Enqueue(avatarLoadInfo);
		}

		public AvatarLoadInfo DeQueue()
		{
			if (_InstantiateWorkQueue == null)
				return null;

			if (_InstantiateWorkQueue.Count() == 0)
				return null;

			// if(_InstantiateWorkQueue.TryDequeue(out var avatarLoadInfo, out var priority))
			// 	return avatarLoadInfo;
			AvatarLoadInfo avatarLoadInfo = _InstantiateWorkQueue.Dequeue();
			return avatarLoadInfo;
		}

		public async UniTask Process()
		{
			try
			{
				while (_tokenSource is { IsCancellationRequested: false })
				{
					CurrentAvatarLoadInfo = DeQueue();
					if (IsValid(CurrentAvatarLoadInfo))
					{
						var isCancelled = await CreateInstance(CurrentAvatarLoadInfo).SuppressCancellationThrow();
					}

					await UniTask.Yield(PlayerLoopTiming.Update, _tokenSource.Token);
				}
			}
			catch (Exception e)
			{
				if (e is OperationCanceledException)
					C2VDebug.LogCategory("[AvatarLoading]", "Cancel Process");
				// else	// 종료할때 에러나서 삭제함
				// 	C2VDebug.LogErrorCategory("[AvatarLoading]", $"Error Process - {e.Message}");
				throw;
			}
		}

		public async UniTask CreateInstance(AvatarLoadInfo avatarLoadInfo)
		{
			try
			{
				await avatarLoadInfo._onProcess(avatarLoadInfo._avatarController, avatarLoadInfo._onCompleted, avatarLoadInfo._avatarController.LoadingCancellationTokenSource);
				//avatarLoadInfo._onCompleted?.Invoke(avatarLoadInfo._avatarController.gameObject);
			}
			catch (Exception e)
			{
				if (e is OperationCanceledException)
					C2VDebug.LogCategory("[AvatarLoading]", "Cancel CreateInstance");
				else
					C2VDebug.LogErrorCategory("[AvatarLoading]", $"Error CreateInstance - {e.Message}");
				throw;
			}
		}

		private bool IsValid(AvatarLoadInfo avatarLoadInfo)
		{
			if (avatarLoadInfo == null || avatarLoadInfo._avatarController.IsReferenceNull())
				return false;

			if (avatarLoadInfo._avatarController.transform.parent.gameObject.activeInHierarchy == false)
				return false;

			return true;
		}

		private void Dispose()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;

			Application.quitting -= Dispose;
		}

		public void RemoveAvatarLoadInfo(AvatarController avatarController)
		{
			AvatarLoadInfo avatarLoadInfo = _InstantiateWorkQueue.Find(info => info._avatarController == avatarController);
			if (avatarLoadInfo != null)
			{
				// C2VDebug.LogCategory("[AvatarLoading]", $"로딩전에 삭제됨- {0}", avatarLoadInfo._avatarController.Info.AvatarId);
				_InstantiateWorkQueue.Remove(avatarLoadInfo);
			}
		}
		
		// public void Test_CancelCurrentAvatar()
		// {
		// 	if (CurrentAvatarLoadInfo == null || CurrentAvatarLoadInfo._avatarController == null)
		// 		return;
		//
		// 	CurrentAvatarLoadInfo._avatarController.DeleteLoadingCancellationTokenSource();
		// 	CurrentAvatarLoadInfo._avatarController.RemoveAllFashionItem();
		// }
	}
}

