/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAsyncOperationHandle.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Com2Verse.AssetSystem
{
    public class C2VAsyncOperationHandle<T>
    {
        public event Action<C2VAsyncOperationHandle<T>> OnCompleted;

        public T Result { get; private set; }

        public AsyncOperationHandle<T> Handle { get; }

        public static implicit operator C2VAsyncOperationHandle(C2VAsyncOperationHandle<T> obj) => new C2VAsyncOperationHandle(obj.Handle);

        public static C2VAsyncOperationHandle<T> Convert(AsyncOperationHandle handle) => new C2VAsyncOperationHandle<T>(handle.Convert<T>());


        public C2VAsyncOperationHandle(AsyncOperationHandle<T> handle)
        {
            Handle = handle;
            Handle.Completed += EventHandler;

            IsDone = false;

            void EventHandler(AsyncOperationHandle<T> internalHandle)
            {
                internalHandle.Completed -= EventHandler;

                Result = internalHandle.Result;
                IsDone = true;

                OnCompleted?.Invoke(this);
                OnCompleted = null;
            }
        }

        public UniTask<T> ToUniTask(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default) =>
            Handle.ToUniTask(progress, timing, cancellationToken);

        public bool IsValid() => Handle.IsValid();

        public DownloadStatus DownloadStatus => Handle.GetDownloadStatus();

        public AsyncOperationStatus Status => Handle.Status;

        public bool IsDone { get; private set; }

        public T WaitForCompletion() => Handle.WaitForCompletion();

        public void Release()
        {
            if (IsValid())
            {
                Addressables.Release(Handle);
            }
        }
    }


    public class C2VAsyncOperationHandle
    {
        public event Action<C2VAsyncOperationHandle> OnCompleted;

        public AsyncOperationHandle Handle { get; }


        public C2VAsyncOperationHandle(AsyncOperationHandle handle)
        {
            Handle = handle;
            Handle.Completed += EventHandler;

            IsDone = false;

            void EventHandler(AsyncOperationHandle internalHandle)
            {
                internalHandle.Completed -= EventHandler;
                IsDone = true;

                OnCompleted?.Invoke(this);
                OnCompleted = null;
            }
        }

        public C2VAsyncOperationHandle<T> Convert<T>() => C2VAsyncOperationHandle<T>.Convert(Handle);

        public UniTask ToUniTask(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default) =>
            Handle.ToUniTask(progress, timing, cancellationToken);

        public bool IsValid() => Handle.IsValid();

        public DownloadStatus DownloadStatus => Handle.GetDownloadStatus();

        public AsyncOperationStatus Status => Handle.Status;

        public bool IsDone { get; private set; }

        public void WaitForCompletion() => Handle.WaitForCompletion();

        public void Release()
        {
            if (IsValid())
            {
                Addressables.Release(Handle);
            }
        }
    }
}
