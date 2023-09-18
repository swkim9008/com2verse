/*===============================================================
* Product:		Com2Verse
* File Name:	StateMachineBehaviour.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.AssetSystem
{
    public class StateMachineBehaviour : MonoBehaviour, IDisposable
    {
        public event Action OnUpdateListener;

        public event Action OnLateUpdateListener;

        public event Action OnFixedUpdateListener;


        private void Awake() => DontDestroyOnLoad(this.gameObject);

        private void Update() => OnUpdateListener?.Invoke();


        private void FixedUpdate() => OnFixedUpdateListener?.Invoke();


        private void LateUpdate() => OnLateUpdateListener?.Invoke();


        public void Dispose()
        {
            OnUpdateListener = null;
            OnLateUpdateListener = null;
            OnFixedUpdateListener = null;

            GameObject.Destroy(this.gameObject);
        }
    }
}
