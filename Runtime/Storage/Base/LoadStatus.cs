using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Base
{
    /// <summary>
    /// The outcome of <see cref="IStorageProvider.TryLoadAsync{T}"/>.
    /// </summary>
    [PublicAPI]
    public enum LoadStatus
    {
        /// <summary>No data is stored for the key.</summary>
        NotFound = 0,

        /// <summary>Data was stored and loaded successfully. It may still be null if null was saved.</summary>
        Loaded = 1,

        /// <summary>Data may be stored but couldn't be read, e.g. a platform error or corrupted data.</summary>
        Failed = 2
    }
}
