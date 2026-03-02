using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Pools.UI
{
    [PublicAPI]
    public readonly struct UIPoolEvents<TData, TPrefab> where TPrefab : MonoBehaviour
    {
        public Action<TData, TPrefab> OnCreated { get; }
        public Action<TData, TPrefab> OnActivated { get; }
        public Action<TPrefab> OnDeactivated { get; }

        public UIPoolEvents(
            Action<TData, TPrefab> onCreated = null,
            Action<TData, TPrefab> onActivated = null,
            Action<TPrefab> onDeactivated = null)
        {
            OnCreated = onCreated;
            OnActivated = onActivated;
            OnDeactivated = onDeactivated;
        }
    }
}