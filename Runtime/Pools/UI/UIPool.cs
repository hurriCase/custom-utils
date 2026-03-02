using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace CustomUtils.Runtime.Pools.UI
{
    [PublicAPI]
    public sealed class UIPool<TPrefab> where TPrefab : Component
    {
        public IReadOnlyList<TPrefab> ActiveItems => _activeItems;

        private readonly List<TPrefab> _activeItems = new();
        private readonly Queue<TPrefab> _inactiveItems = new();

        private readonly TPrefab _prefab;
        private readonly RectTransform _container;
        private readonly IObjectResolver _objectResolver;

        public UIPool(
            [NotNull] TPrefab prefab,
            [NotNull] RectTransform container,
            IObjectResolver objectResolver = null)
        {
            _prefab = prefab;
            _container = container;
            _objectResolver = objectResolver;
        }

        private void AddElement()
        {
            if (_inactiveItems.TryDequeue(out var item))
                item.gameObject.SetActive(true);
            else
                item = _objectResolver == null
                    ? Object.Instantiate(_prefab, _container)
                    : _objectResolver.Instantiate(_prefab, _container);

            _activeItems.Add(item);
        }

        private void RemoveElement(TPrefab item)
        {
            if (_activeItems.Remove(item) is false)
                return;

            item.gameObject.SetActive(false);
            _inactiveItems.Enqueue(item);
        }

        public void EnsureCount(int desiredCount)
        {
            while (_activeItems.Count > desiredCount)
            {
                RemoveElement(_activeItems[^1]);
            }

            while (_activeItems.Count < desiredCount)
            {
                AddElement();
            }
        }
    }
}