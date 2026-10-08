using System.Threading;
using CustomUtils.Runtime.Serializer;
using CustomUtils.Runtime.Storage.Base;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.PersistentEditor
{
    /// <inheritdoc />
    /// <summary>
    /// Stores data using Unity's <see cref="EditorPrefs"/>. Editor only; every operation completes synchronously.
    /// </summary>
    [PublicAPI]
    public sealed class EditorPrefsProvider : StorageProviderBase<string>
    {
        private readonly IStringSerializer _serializer;

        /// <summary>
        /// Creates a provider that serializes values with <paramref name="serializer"/>.
        /// </summary>
        /// <param name="serializer">Serializer used to convert values to and from strings</param>
        public EditorPrefsProvider(IStringSerializer serializer)
        {
            _serializer = serializer;
        }

        protected override string Serialize<TData>(TData data) => _serializer.SerializeToString(data);
        protected override TData Deserialize<TData>(string raw) => _serializer.DeserializeFromString<TData>(raw);

        protected override UniTask PlatformSaveAsync(string key, string data)
        {
            EditorPrefs.SetString(key, data);
            return UniTask.CompletedTask;
        }

        protected override UniTask<string> PlatformLoadAsync(string key, CancellationToken token)
            => UniTask.FromResult(EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key, null) : null);

        protected override UniTask<bool> PlatformHasKeyAsync(string key, CancellationToken token)
            => UniTask.FromResult(EditorPrefs.HasKey(key));

        protected override UniTask PlatformDeleteKeyAsync(string key, CancellationToken token)
        {
            EditorPrefs.DeleteKey(key);
            return UniTask.CompletedTask;
        }

        // EditorPrefs are shared by every project on this machine, so deleting all of them isn't supported
        protected override UniTask<bool> PlatformTryDeleteAllAsync(CancellationToken token)
            => UniTask.FromResult(false);
    }
}
