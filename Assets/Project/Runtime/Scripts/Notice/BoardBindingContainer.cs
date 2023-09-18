/*===============================================================
 * Product:		Com2Verse
 * File Name:	BoardBindingContainer.cs
 * Developer:	yangsehoon
 * Date:		2022-12-06 오후 1:33
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Threading;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public class BoardBindingContainer : MonoBehaviour, IBindingContainer
	{
        private CancellationTokenSource _cancellationTokenSource;
        
        public ViewModelContainer ViewModelContainer { get; } = new();
        public Transform GetTransform() => this.transform;

        private Binder[] _binders;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            ViewModelContainer.ClearAll();
            BindAsync().Forget();
        }
        
        private async UniTask BindAsync()
        {
            var boardManager = BoardManager.Instance;

            await UniTask.WaitUntil(() => boardManager.BoardViewModel != null,
                cancellationToken: _cancellationTokenSource.Token);
            ViewModelContainer.AddViewModel(boardManager.BoardViewModel);

            Bind();
        }

        public void Bind()
        {
            ViewModelContainer.InitializeViewModel();

            _binders ??= GetComponentsInChildren<Binder>(true);

            foreach (var binder in _binders)
            {
                if (binder.gameObject == this.gameObject)
                    continue;

                binder.SetViewModelContainer(ViewModelContainer, true);

                binder.Bind();
            }
        }

        public void Unbind()
        {
            foreach (var binder in _binders)
            {
                if (binder.IsUnityNull()) continue;

                if (binder.gameObject == this.gameObject) continue;


                binder.Unbind();
            }

            ViewModelContainer.ClearAll();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;

            _binders = null;
            ViewModelContainer.ClearAll();
        }
    }
}
