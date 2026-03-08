using CustomUtils.Runtime.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using VContainer.Unity;

namespace CustomUtils.Runtime.Pools.Objects
{
    [PublicAPI]
    public sealed class ComponentPool<TComponent> : Pool<TComponent>
        where TComponent : Component
    {
        public ComponentPool(PoolParameters<TComponent> poolParameters) : base(poolParameters) { }
        public ComponentPool(PoolConfig<TComponent> poolParameters) : base(poolParameters) { }

        protected override TComponent CreateEntity()
        {
            var entity = poolParameters.ObjectResolver == null
                ? Object.Instantiate(poolParameters.Prefab, poolParameters.Parent)
                : poolParameters.ObjectResolver.Instantiate(poolParameters.Prefab, poolParameters.Parent);

            poolParameters.OnCreateCallback?.Invoke(entity);
            SetActive(entity, false);
            return entity;
        }

        protected override void SetActive(TComponent entity, bool active)
        {
            entity.SetActive(active);
        }
    }
}