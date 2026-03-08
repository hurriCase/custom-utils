using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.Pools.Objects
{
    /// <summary>
    /// Generic object pool handler for Unity objects with automatic activation management
    /// </summary>
    /// <typeparam name="TEntity">Type of Unity object to pool (GameObject or Component)</typeparam>
    [PublicAPI]
    public abstract class Pool<TEntity> where TEntity : Object
    {
        protected const int InitialDefaultPoolSize = 10;
        protected const int InitialMaxPoolSize = 100;

        protected TEntity prefab;
        protected Transform parent;
        protected IObjectResolver objectResolver;
        protected Action<TEntity> onCreateCallback;
        protected Action<TEntity> onGetCallback;
        protected Action<TEntity> onReleaseCallback;
        protected Action<TEntity> onDestroyCallback;

        private readonly IObjectPool<TEntity> _pool;

        protected Pool(
            TEntity prefab,
            int defaultPoolSize = InitialDefaultPoolSize,
            int maxPoolSize = InitialMaxPoolSize,
            Transform parent = null,
            IObjectResolver objectResolver = null,
            Action<TEntity> onCreateCallback = null,
            Action<TEntity> onGetCallback = null,
            Action<TEntity> onReleaseCallback = null,
            Action<TEntity> onDestroyCallback = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.objectResolver = objectResolver;
            this.onCreateCallback = onCreateCallback;
            this.onGetCallback = onGetCallback;
            this.onReleaseCallback = onReleaseCallback;
            this.onDestroyCallback = onDestroyCallback;

            _pool = new ObjectPool<TEntity>(
                CreateEntity,
                OnGet,
                OnRelease,
                OnDestroy,
                false,
                defaultPoolSize,
                maxPoolSize);
        }

        protected Pool(
            PoolConfig<TEntity> poolConfig,
            IObjectResolver objectResolver = null,
            Action<TEntity> onCreateCallback = null,
            Action<TEntity> onGetCallback = null,
            Action<TEntity> onReleaseCallback = null,
            Action<TEntity> onDestroyCallback = null)
        {
            prefab = poolConfig.Prefab;
            parent = poolConfig.Parent;

            this.objectResolver = objectResolver;
            this.onCreateCallback = onCreateCallback;
            this.onGetCallback = onGetCallback;
            this.onReleaseCallback = onReleaseCallback;
            this.onDestroyCallback = onDestroyCallback;

            _pool = new ObjectPool<TEntity>(
                CreateEntity,
                OnGet,
                OnRelease,
                OnDestroy,
                false,
                poolConfig.DefaultPoolSize,
                poolConfig.MaxPoolSize);
        }

        /// <summary>
        /// Creates a new entity instance from the prefab
        /// </summary>
        /// <returns>New entity instance</returns>
        protected virtual TEntity CreateEntity()
        {
            var entity = Object.Instantiate(prefab, parent);
            onCreateCallback?.Invoke(entity);
            SetActive(entity, false);
            return entity;
        }

        /// <summary>
        /// Called when an entity is retrieved from the pool
        /// </summary>
        /// <param name="entity">Entity being retrieved</param>
        protected virtual void OnGet(TEntity entity)
        {
            onGetCallback?.Invoke(entity);
            SetActive(entity, true);
        }

        /// <summary>
        /// Called when an entity is returned to the pool
        /// </summary>
        /// <param name="entity">Entity being returned</param>
        protected virtual void OnRelease(TEntity entity)
        {
            onReleaseCallback?.Invoke(entity);
            SetActive(entity, false);
        }

        /// <summary>
        /// Called when an entity is being destroyed
        /// </summary>
        /// <param name="entity">Entity being destroyed</param>
        protected virtual void OnDestroy(TEntity entity)
        {
            onDestroyCallback?.Invoke(entity);
            Object.Destroy(entity);
        }

        protected abstract void SetActive(TEntity entity, bool active);

        /// <summary>
        /// Gets an object from the pool
        /// </summary>
        /// <returns>Active pooled object</returns>
        public TEntity Get() => _pool.Get();

        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        /// <param name="element">Object to return to the pool</param>
        public void Release(TEntity element) => _pool.Release(element);

        /// <summary>
        /// Clears all objects from the pool
        /// </summary>
        public void Clear() => _pool.Clear();
    }
}