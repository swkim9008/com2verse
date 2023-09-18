/*===============================================================
* Product:		Com2Verse
* File Name:	MonoSingleton.cs
* Developer:	urun4m0r1
* Date:			2022-10-20 11:03
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse
{
    /// <inheritdoc cref="Metaverse.IMonoSingleton" />
    public class MonoSingleton<T> : MonoBehaviour, IMonoSingleton where T : MonoBehaviour
    {
        protected static GenericValue<T, bool> IsDestroyable { get; set; } = new(false);

        private static Lazy<T>? _lazyInstance;

        private static GenericValue<T, GameObject> _instanceGameObject;

        private static string GenericTypeName => SingletonHelper<T>.GenericTypeName;
        private static string ClassName       => SingletonHelper<T>.ClassName;
        private static string TypeName        => SingletonHelper<T>.TypeName;

        private static string GameObjectName => $"[{GenericTypeName}] ({TypeName})";

        private static string GetGameObjectPath(GameObject gameObject) => $"\"{gameObject.name}\" in scene \"{gameObject.scene.name}\"";

#region InstanceAccess
        public static bool InstanceExists => _lazyInstance?.IsValueCreated ?? false;

        public static T? InstanceOrNull => InstanceExists ? _lazyInstance?.Value : null;

        public static T Instance => GetOrCreateInstance();

        public static void ForceCreateInstance()
        {
            if (InstanceExists)
            {
                SingletonHelper<T>.LogInstanceAlreadyCreated();
                DestroySingletonInstance();
            }

            GetOrCreateInstance();
        }

        public static void CreateInstance()
        {
            if (InstanceExists)
            {
                throw SingletonHelper<T>.InstanceAlreadyCreatedException;
            }

            GetOrCreateInstance();
        }

        public static bool TryCreateInstance()
        {
            if (InstanceExists)
            {
                return false;
            }

            GetOrCreateInstance();
            return true;
        }

        public bool IsSingleton => ReferenceEquals(this, InstanceOrNull!) || ReferenceEquals(gameObject, _instanceGameObject.Value!);
#endregion // InstanceAccess

#region InstanceGeneration
        private static T GetOrCreateInstance()
        {
            _lazyInstance ??= GenerateLazyInstance();

            var instance = _lazyInstance.Value;
            if (instance == null)
            {
                ResetInstanceReferences();
                throw SingletonHelper<T>.InstanceNullOrDestroyedException;
            }

            return instance;
        }

        private static Lazy<T> GenerateLazyInstance()
        {
#if UNITY_INCLUDE_TESTS
            if (IsTesting)
            {
                return SingletonHelper<T>.GenerateThreadSafeLazyInstance(GenerateInstance);
            }
#endif // UNITY_INCLUDE_TESTS

            SingletonHelper<T>.EnsureApplicationIsPlaying();
            return SingletonHelper<T>.GenerateThreadSafeLazyInstance(GenerateInstance);
        }

        private static T GenerateInstance()
        {
            var instances = FindObjectsOfType<T>(true);
            if (instances == null || instances.Length == 0)
            {
                return CreateNewInstance();
            }

            if (instances.Length == 1)
            {
                return GetExistingInstance(instances[0]);
            }

            ResetInstanceReferences();
            throw new InvalidOperationException($"{ClassName} cannot be created because there are already {instances.Length} instances exists.");
        }

        private static T CreateNewInstance()
        {
            var gameObject = new GameObject(GameObjectName)
            {
                hideFlags = HideFlags.DontSave,
            };

            _instanceGameObject.Value = gameObject;

            var instance = gameObject.AddComponent<T>();
            if (instance == null)
            {
                ResetInstanceReferences();
                DestroyTarget(gameObject);
                throw new MissingComponentException($"{ClassName} failed to add component to {GetGameObjectPath(gameObject)}.");
            }

            if (!IsDestroyable.Value)
                SetDontDestroyOnLoad(instance);

            SingletonHelper<T>.LogInstanceCreated();
            return instance;
        }

        private static T GetExistingInstance(T instance)
        {
            var gameObject = instance.gameObject;

            _instanceGameObject.Value = gameObject;

            if (!IsDestroyable.Value)
                SetDontDestroyOnLoad(gameObject);

            C2VDebug.LogCategory(GenericTypeName, $"<{TypeName}> instance assigned to existing {GetGameObjectPath(gameObject)}.");
            return instance;
        }
#endregion // InstanceGeneration

#region InstanceDestruction
        public static void DestroySingletonInstance()
        {
            var instance = InstanceOrNull;

            // ReSharper disable once SuspiciousTypeConversion.Global
            (instance as IDisposable)?.Dispose();
            ResetInstanceReferences();

            if (instance != null)
            {
                DestroyTarget(instance.gameObject);
                SingletonHelper<T>.LogInstanceDestroyed();
            }
        }

        private void DestroyDuplicatedInstance()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            (this as IDisposable)?.Dispose();
            DestroyTarget(this);
            C2VDebug.LogWarningCategory(GenericTypeName, $"<{TypeName}> instance already exists. Destroying duplicated instance attached to {GetGameObjectPath(gameObject)}.");
        }

        private static void ResetInstanceReferences()
        {
            _lazyInstance             = null;
            _instanceGameObject.Value = null;
        }
#endregion // InstanceDestruction

#region MonoBehaviour
        private bool _isApplicationQuitting;

        private void Awake()
        {
            if (_instanceGameObject.Value == null)
            {
                CreateInstance();
            }
            else if (!IsSingleton)
            {
                DestroyDuplicatedInstance();
            }

            AwakeInvoked();
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;

            OnApplicationQuitInvoked();

            if (IsSingleton)
            {
                DestroySingletonInstance();
            }
        }

        private void OnDestroy()
        {
            OnDestroyInvoked();

            if (IsSingleton)
            {
                DestroySingletonInstance();

                if (!IsDestroyable.Value && !_isApplicationQuitting)
                {
                    C2VDebug.LogErrorCategory(GenericTypeName, $"<{TypeName}> instance should not be destroyed manually. Singleton integrity is compromised.");
                }
            }
        }

        protected virtual void AwakeInvoked()             { }
        protected virtual void OnApplicationQuitInvoked() { }
        protected virtual void OnDestroyInvoked()         { }
#endregion // MonoBehaviour

#region Utils
        private static void DestroyTarget(UnityEngine.Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static void SetDontDestroyOnLoad(UnityEngine.Object target)
        {
            if (Application.isPlaying)
                DontDestroyOnLoad(target);
        }
#endregion // Utils

#if UNITY_INCLUDE_TESTS
        private static Lazy<T>? _previousInstance;

        private static GenericValue<T, GameObject> _previousGameObject;

        private static GenericValue<T, bool> _isTesting;

        public static bool IsTesting => _isTesting.Value;

        public static void SetupForTests()
        {
            _isTesting.Value = true;

            _previousInstance   = _lazyInstance;
            _previousGameObject = _instanceGameObject;
            _lazyInstance       = null;
            _instanceGameObject = default;
        }

        public static void TearDownForTests()
        {
            DestroySingletonInstance();

            _lazyInstance       = _previousInstance;
            _instanceGameObject = _previousGameObject;
            _previousInstance   = null;
            _previousGameObject = default;

            _isTesting.Value = false;
        }
#endif // UNITY_INCLUDE_TESTS
    }
}
