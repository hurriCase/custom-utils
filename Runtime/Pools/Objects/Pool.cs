using JetBrains.Annotations;
using UnityEngine.Pool;
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
        protected readonly PoolParameters<TEntity> poolParameters;
        private readonly IObjectPool<TEntity> _pool;

        protected Pool(PoolParameters<TEntity> poolParameters)
        {
            this.poolParameters = poolParameters;
            _pool = new ObjectPool<TEntity>(
                CreateEntity,
                OnGet,
                OnRelease,
                OnDestroy,
                false,
                poolParameters.DefaultPoolSize,
                poolParameters.MaxPoolSize);
        }

        protected Pool(PoolConfig<TEntity> poolParameters)
        {
            this.poolParameters = new PoolParameters<TEntity>(poolParameters);
            _pool = new ObjectPool<TEntity>(
                CreateEntity,
                OnGet,
                OnRelease,
                OnDestroy,
                false,
                poolParameters.DefaultPoolSize,
                poolParameters.MaxPoolSize);
        }

        /// <summary>
        /// Creates a new entity instance from the prefab
        /// </summary>
        /// <returns>New entity instance</returns>
        protected virtual TEntity CreateEntity()
        {
            var entity = Object.Instantiate(poolParameters.Prefab, poolParameters.Parent);
            poolParameters.OnCreateCallback?.Invoke(entity);
            SetActive(entity, false);
            return entity;
        }

        /// <summary>
        /// Called when an entity is retrieved from the pool
        /// </summary>
        /// <param name="entity">Entity being retrieved</param>
        protected virtual void OnGet(TEntity entity)
        {
            poolParameters.OnGetCallback?.Invoke(entity);
            SetActive(entity, true);
        }

        /// <summary>
        /// Called when an entity is returned to the pool
        /// </summary>
        /// <param name="entity">Entity being returned</param>
        protected virtual void OnRelease(TEntity entity)
        {
            poolParameters.OnReleaseCallback?.Invoke(entity);
            SetActive(entity, false);
        }

        /// <summary>
        /// Called when an entity is being destroyed
        /// </summary>
        /// <param name="entity">Entity being destroyed</param>
        protected virtual void OnDestroy(TEntity entity)
        {
            poolParameters.OnDestroyCallback?.Invoke(entity);
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