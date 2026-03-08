using System;
using CustomUtils.Runtime.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.Pools.Objects
{
    [PublicAPI]
    public sealed class ComponentPool<TComponent> : Pool<TComponent>
        where TComponent : Component
    {
        public ComponentPool(
            TComponent prefab,
            int defaultPoolSize = InitialDefaultPoolSize,
            int maxPoolSize = InitialMaxPoolSize,
            Transform parent = null,
            IObjectResolver objectResolver = null,
            Action<TComponent> onCreateCallback = null,
            Action<TComponent> onGetCallback = null,
            Action<TComponent> onReleaseCallback = null,
            Action<TComponent> onDestroyCallback = null)
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

        public ComponentPool(
            PoolConfig<TComponent> poolConfig,
            IObjectResolver objectResolver = null,
            Action<TComponent> onCreateCallback = null,
            Action<TComponent> onGetCallback = null,
            Action<TComponent> onReleaseCallback = null,
            Action<TComponent> onDestroyCallback = null)
            : base(
                poolConfig,
                objectResolver,
                onCreateCallback,
                onGetCallback,
                onReleaseCallback,
                onDestroyCallback) { }

        protected override TComponent CreateEntity()
        {
            var entity = objectResolver == null
                ? Object.Instantiate(prefab, parent)
                : objectResolver.Instantiate(prefab, parent);

            onCreateCallback?.Invoke(entity);
            SetActive(entity, false);
            return entity;
        }

        protected override void SetActive(TComponent entity, bool active)
        {
            entity.SetActive(active);
        }
    }
}