using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Pools.Objects
{
    [PublicAPI]
    public sealed class GameObjectPool : Pool<GameObject>
    {
        public GameObjectPool(PoolParameters<GameObject> poolParameters) : base(poolParameters) { }
        public GameObjectPool(PoolConfig<GameObject> poolParameters) : base(poolParameters) { }

        protected override void SetActive(GameObject entity, bool active)
        {
            entity.SetActive(active);
        }
    }
}