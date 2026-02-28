using System;
using JetBrains.Annotations;
using R3;

namespace CustomUtils.Editor.Scripts.PersistentEditor
{
    /// <inheritdoc />
    /// <summary>
    /// Reactive property that automatically persists to EditorPrefs
    /// </summary>
    /// <typeparam name="TProperty">Type of the property value</typeparam>
    [PublicAPI]
    public sealed class PersistentEditorProperty<TProperty> : IDisposable
    {
        private readonly string _key;
        private readonly IDisposable _subscription;
        private bool _savingEnabled = true;

        private readonly ReactiveProperty<TProperty> _property;

        /// <summary>
        /// Gets or sets the current value of the property
        /// </summary>

        public TProperty Value
        {
            get => _property.Value;
            set => _property.Value = value;
        }

        /// <summary>
        /// Creates a new persistent editor property
        /// </summary>
        /// <param name="key">Unique key for EditorPrefs storage</param>
        /// <param name="defaultValue">Default value if no saved value exists</param>
        public PersistentEditorProperty(string key, TProperty defaultValue = default)
        {
            _key = key;
            _property = new ReactiveProperty<TProperty>(defaultValue);

            _subscription = _property.Subscribe(this, static (_, state) =>
            {
                if (state._savingEnabled)
                    state.Save();
            });

            Initialize();
        }

        /// <summary>
        /// Subscribes to value changes
        /// </summary>
        /// <typeparam name="TTarget">Type of target object</typeparam>
        /// <param name="target">Target object to pass to callback</param>
        /// <param name="onNext">Action to execute when value changes</param>
        /// <returns>Disposable subscription</returns>
        public IDisposable Subscribe<TTarget>(TTarget target, Action<TTarget, TProperty> onNext) where TTarget : class
            => _property.Subscribe(
                (target, onNext),
                static (value, tuple) => tuple.onNext(tuple.target, value));

        /// <summary>
        /// Manually saves the current value to EditorPrefs
        /// </summary>
        public void Save() => EditorPrefsHelper.SetValue(_key, _property.Value);

        private void Initialize()
        {
            try
            {
                _savingEnabled = false;

                if (!EditorPrefsHelper.HasKey(_key))
                    return;

                var loaded = EditorPrefsHelper.GetValue<TProperty>(_key);
                _property.Value = loaded;
            }
            finally
            {
                _savingEnabled = true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes the property and stops persistence
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
            _property.Dispose();
        }
    }
}