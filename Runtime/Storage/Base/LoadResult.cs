using JetBrains.Annotations;

namespace CustomUtils.Runtime.Storage.Base
{
    /// <summary>
    /// The result of <see cref="IStorageProvider.TryLoadAsync{T}"/>: a status and, when loaded, the data.
    /// </summary>
    /// <typeparam name="TData">The type of the loaded data</typeparam>
    [PublicAPI]
    public readonly struct LoadResult<TData>
    {
        /// <summary>
        /// Gets whether the data was loaded, not found, or failed to load.
        /// </summary>
        public LoadStatus Status { get; }

        /// <summary>
        /// Gets the loaded data. Only meaningful when <see cref="Status"/> is <see cref="LoadStatus.Loaded"/>.
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// A result for a key with no stored data.
        /// </summary>
        public static LoadResult<TData> NotFound => new(LoadStatus.NotFound, default);

        /// <summary>
        /// A result for a key whose stored data couldn't be read.
        /// </summary>
        public static LoadResult<TData> Failed => new(LoadStatus.Failed, default);

        /// <summary>
        /// Creates a result for successfully loaded data.
        /// </summary>
        /// <param name="data">The loaded data, which may be null if null was saved</param>
        /// <returns>A result with <see cref="LoadStatus.Loaded"/> status</returns>
        public static LoadResult<TData> Loaded(TData data) => new(LoadStatus.Loaded, data);

        private LoadResult(LoadStatus status, TData data)
        {
            Status = status;
            Data = data;
        }
    }
}
