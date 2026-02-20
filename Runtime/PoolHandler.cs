using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime
{
    /// <summary>
    /// Generic object pool handler for Unity objects with automatic activation management
    /// </summary>
    /// <typeparam name="TPoolEntity">Type of Unity object to pool (GameObject or Component)</typeparam>
    [PublicAPI]
    public class PoolHandler<TPoolEntity> where TPoolEntity : Object
    {
        private TPoolEntity _prefab;
        private Transform _parent;
        private Action<TPoolEntity> _onCreateCallback;
        private Action<TPoolEntity> _onGetCallback;
        private Action<TPoolEntity> _onReleaseCallback;
        private Action<TPoolEntity> _onDestroyCallback;

        private IObjectPool<TPoolEntity> _pool;

        /// <summary>
        /// Initializes the object pool with specified configuration
        /// </summary>
        /// <param name="prefab">Prefab to instantiate for new pool objects</param>
        /// <param name="defaultPoolSize">Initial number of objects to create in the pool</param>
        /// <param name="maxPoolSize">Maximum number of objects the pool can hold</param>
        /// <param name="onCreateCallback">Called when a new object is created</param>
        /// <param name="onGetCallback">Called when an object is retrieved from the pool</param>
        /// <param name="onReleaseCallback">Called when an object is returned to the pool</param>
        /// <param name="onDestroyCallback">Called when an object is destroyed</param>
        /// <param name="parent">Transform parent for instantiated objects</param>
        public void Init(
            TPoolEntity prefab,
            int defaultPoolSize = 10,
            int maxPoolSize = 100,
            Action<TPoolEntity> onCreateCallback = null,
            Action<TPoolEntity> onGetCallback = null,
            Action<TPoolEntity> onReleaseCallback = null,
            Action<TPoolEntity> onDestroyCallback = null,
            Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            _onCreateCallback = onCreateCallback;
            _onGetCallback = onGetCallback;
            _onReleaseCallback = onReleaseCallback;
            _onDestroyCallback = onDestroyCallback;

            _pool = new ObjectPool<TPoolEntity>(
                CreateEntity,
                OnGet,
                OnRelease,
                OnDestroy,
                false,
                defaultPoolSize,
                maxPoolSize);
        }

        /// <summary>
        /// Creates a new entity instance from the prefab
        /// </summary>
        /// <returns>New entity instance</returns>
        protected virtual TPoolEntity CreateEntity()
        {
            var entity = Object.Instantiate(_prefab, _parent);
            _onCreateCallback?.Invoke(entity);
            SetActive(entity, false);
            return entity;
        }

        /// <summary>
        /// Called when an entity is retrieved from the pool
        /// </summary>
        /// <param name="entity">Entity being retrieved</param>
        protected virtual void OnGet(TPoolEntity entity)
        {
            _onGetCallback?.Invoke(entity);
            SetActive(entity, true);
        }

        /// <summary>
        /// Called when an entity is returned to the pool
        /// </summary>
        /// <param name="entity">Entity being returned</param>
        protected virtual void OnRelease(TPoolEntity entity)
        {
            _onReleaseCallback?.Invoke(entity);
            SetActive(entity, false);
        }

        /// <summary>
        /// Called when an entity is being destroyed
        /// </summary>
        /// <param name="entity">Entity being destroyed</param>
        protected virtual void OnDestroy(TPoolEntity entity)
        {
            _onDestroyCallback?.Invoke(entity);
            Object.Destroy(entity);
        }

        /// <summary>
        /// Gets an object from the pool
        /// </summary>
        /// <returns>Active pooled object</returns>
        public TPoolEntity Get() => _pool.Get();

        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        /// <param name="element">Object to return to the pool</param>
        public void Release(TPoolEntity element) => _pool.Release(element);

        /// <summary>
        /// Clears all objects from the pool
        /// </summary>
        public void Clear() => _pool.Clear();

        private void SetActive(TPoolEntity entity, bool active)
        {
            if (!entity)
            {
                Debug.LogWarning("Attempted to set active state on a destroyed object");
                return;
            }

            switch (entity)
            {
                case GameObject gameObject:
                    gameObject.SetActive(active);
                    break;

                case Component component:
                    component.gameObject.SetActive(active);
                    break;

                default:
                    Debug.LogWarning($"Cannot set active state on {entity.GetType().Name}. " +
                                     "PoolHandler only supports GameObject and Component types.");
                    break;
            }
        }
    }
}