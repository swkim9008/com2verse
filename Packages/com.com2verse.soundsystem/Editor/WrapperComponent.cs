/*===============================================================
* Product:    Com2Verse
* File Name:  WrapperComponent.cs
* Developer:  yangsehoon
* Date:       2022-04-12 10:17
* History:    
* Documents:  Wrapper component custom editor. (Hides origin Component T(T, U), destroy origin Component T(T, U) when Component U(V) destroyed)
*               Supports up to 2 base components (T, U)
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor.Sound
{
    public abstract class MetaverseMediaSourceEditor : Editor
    {
        public virtual bool TryGetComponent<T>(out T component) where T : Object
        {
            component = default(T);
            return true;
        }

        public virtual void SetComponent<T>(T component) { }
    }

    public class WrapperComponent<T, U> : MetaverseMediaSourceEditor where T : Component where U : MonoBehaviour
    {
        private GameObject _GameObject = null;
        private T _Source = null;

        void OnEnable()
        {
            U _gameObject = (U) target;
            if (!TryGetComponent<T>(out _))
            {
                SetComponent(ObjectFactory.AddComponent<T>(_gameObject.gameObject));
            }
            
            _GameObject = _gameObject.gameObject;
            if (TryGetComponent<T>(out _Source))
            {
                _Source.hideFlags = HideFlags.HideInInspector;
            }
        }

        void OnDisable()
        {
            // Component removed
            if (_GameObject != null && !_GameObject.TryGetComponent<U>(out _))
            {
                DestroyImmediate(_Source, true);
            }
        }
    }

    public class WrapperComponent<T, U, V> : MetaverseMediaSourceEditor where T : Component where U : Component where V : MonoBehaviour
    {
        private GameObject _GameObject = null;
        private T _Source1 = null;
        private U _Source2 = null;
        
        void OnEnable()
        {
            V _gameObject = (V)target;

            if (!TryGetComponent<T>(out _))
            {
                SetComponent(ObjectFactory.AddComponent<T>(_gameObject.gameObject));
            }
            if (!TryGetComponent<U>(out _))
            {
                SetComponent(ObjectFactory.AddComponent<U>(_gameObject.gameObject));
            }
            
            _GameObject = _gameObject.gameObject;
            if (TryGetComponent<T>(out _Source1))
            {
                _Source1.hideFlags = HideFlags.HideInInspector;
            }
            if (TryGetComponent<U>(out _Source2))
            {
                _Source2.hideFlags = HideFlags.HideInInspector;
            }
        }

        void OnDisable()
        {
            // Component removed
            if (_GameObject != null && !_GameObject.TryGetComponent<V>(out _))
            {
                DestroyImmediate(_Source1, true);
                DestroyImmediate(_Source2, true);
            }
        }
    }
}
#endif
