using System;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;

namespace CustomUtils.Runtime.Pools.Objects
{
    [PublicAPI]
    public sealed class GameObjectPool : Pool<GameObject>
    {
        public GameObjectPool(
            GameObject prefab,
            int defaultPoolSize = InitialDefaultPoolSize,
            int maxPoolSize = InitialMaxPoolSize,
            Transform parent = null,
            IObjectResolver objectResolver = null,
            Action<GameObject> onCreateCallback = null,
            Action<GameObject> onGetCallback = null,
            Action<GameObject> onReleaseCallback = null,
            Action<GameObject> onDestroyCallback = null)
            : base(
                prefab,
                defaultPoolSize,
                maxPoolSize,
                parent,
                objectResolver,
                onCreateCallback,
                onGetCallback,
                onReleaseCallback,
                onDestroyCallback) { }

        public GameObjectPool(
            PoolConfig<GameObject> poolConfig,
            IObjectResolver objectResolver = null,
            Action<GameObject> onCreateCallback = null,
            Action<GameObject> onGetCallback = null,
            Action<GameObject> onReleaseCallback = null,
            Action<GameObject> onDestroyCallback = null)
            : base(
                poolConfig,
                objectResolver,
                onCreateCallback,
                onGetCallback,
                onReleaseCallback,
                onDestroyCallback) { }

        protected override void SetActive(GameObject entity, bool active)
        {
            entity.SetActive(active);
        }
    }
}