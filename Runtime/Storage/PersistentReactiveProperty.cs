using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.Storage
{
    [PublicAPI]
    public sealed class PersistentReactiveProperty<TProperty> : IDisposable
    {
        public ReactiveProperty<TProperty> Property { get; private set; }

        private string _key;
        private IDisposable _subscription;
        private IStorageProvider _provider;
        private bool _savingEnabled;

        public TProperty Value
        {
            get => Property.Value;
            set => Property.Value = value;
        }

        public IDisposable Subscribe<TTarget>(TTarget target, Action<TProperty, TTarget> onNext)
            => Property.Subscribe((target, onNext),
                static (property, tuple) => tuple.onNext(property, tuple.target));

        public IDisposable Subscribe(Action<TProperty> onNext)
            => Property.Subscribe(onNext, static (property, action) => action(property));

        public Observable<TProperty> AsObservable() => Property.AsObservable();

        public async UniTask InitializeAsync(string key, CancellationToken token = default, TProperty defaultValue = default)
        {
            _provider = StorageProvider.Provider;

            _key = key;
            Property = new ReactiveProperty<TProperty>(defaultValue);

            _subscription = Property
                .Where(this, static (_, self) => self._savingEnabled)
                .Subscribe(this, static (_, self) => self.SaveAsync().Forget());

            try
            {
                var loaded = await _provider.LoadAsync<TProperty>(_key, token);
                if (loaded != null)
                    Property.Value = loaded;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("[PersistentReactiveProperty::InitializeAsync] " +
                               $"Failed to load key '{_key}': {e.Message}");
            }
            finally
            {
                _savingEnabled = true;
            }
        }

        public async UniTask SaveAsync() => await _provider.TrySaveAsync(_key, Property.Value);

        public void Dispose()
        {
            _subscription?.Dispose();
            Property?.Dispose();
        }
    }
}