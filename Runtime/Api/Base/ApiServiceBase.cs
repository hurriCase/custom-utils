using System;
using System.Threading;
using CustomUtils.Runtime.Api.Interfaces;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine.Networking;

namespace CustomUtils.Runtime.Api.Base
{
    /// <summary>
    /// Base class for API services with availability monitoring and request handling.
    /// </summary>
    /// <typeparam name="TConfig">The configuration type derived from ApiConfigBase.</typeparam>
    [PublicAPI]
    public abstract class ApiServiceBase<TConfig> : IDisposable where TConfig : ApiConfigBase
    {
        /// <summary>
        /// Gets the current availability status of the API.
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsAvailable => _isAvailable;
        private readonly ReactiveProperty<bool> _isAvailable = new();

        private readonly IApiAvailabilityChecker _apiAvailabilityChecker;
        private readonly IApiClient _apiClient;
        private readonly TConfig _config;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly IDisposable _disposable;

        /// <summary>
        /// Initializes a new instance of the ApiServiceBase class.
        /// </summary>
        /// <param name="apiAvailabilityChecker">The availability checker.</param>
        /// <param name="apiClient">The HTTP client.</param>
        /// <param name="config">The API configuration.</param>
        protected ApiServiceBase(IApiAvailabilityChecker apiAvailabilityChecker, IApiClient apiClient, TConfig config)
        {
            _apiAvailabilityChecker = apiAvailabilityChecker;
            _apiClient = apiClient;
            _config = config;

            _disposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(_config.UpdateAvailabilityInterval))
                .AsObservable()
                .Subscribe(this, static (_, self) => self.UpdateAvailable(self._cancellationTokenSource.Token)
                    .Forget());
        }

        /// <summary>
        /// Sends a request and retrieves a response.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="request">The request data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The response result containing data and success status.</returns>
        public async UniTask<ResponseResult<TResponse>> GetResponse<TRequest, TResponse>(
            TRequest request,
            CancellationToken token)
            where TRequest : class
            where TResponse : class, IValidatable
        {
            if (!_config.IsValid())
                return new ResponseResult<TResponse>(false);

            var url = _config.GetApiUrl();
            var response = await _apiClient.PostAsync<TRequest, TResponse>(request, url, token);
            var success = response != null && response.IsValid();
            return new ResponseResult<TResponse>(response, success);
        }

        /// <summary>
        /// Sends a request with custom headers and retrieves a response.
        /// </summary>
        /// <typeparam name="TSource">The source type for header configuration.</typeparam>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="source">The source object for header configuration.</param>
        /// <param name="request">The request data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="additionalHeaders">Callback to configure additional headers.</param>
        /// <returns>The response result containing data and success status.</returns>
        public async UniTask<ResponseResult<TResponse>> GetResponse<TSource, TRequest, TResponse>(
            TSource source,
            TRequest request,
            CancellationToken token,
            Action<TSource, UnityWebRequest> additionalHeaders)
            where TRequest : class
            where TResponse : class, IValidatable
        {
            if (!_config.IsValid())
                return new ResponseResult<TResponse>(false);

            var url = _config.GetApiUrl();
            var response = await _apiClient.PostAsync<TSource, TRequest, TResponse>(
                source,
                request,
                url,
                token,
                additionalHeaders);
            var success = response != null && response.IsValid();
            return new ResponseResult<TResponse>(response, success);
        }

        /// <summary>
        /// Updates the availability status by checking the API endpoint.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        public async UniTask UpdateAvailable(CancellationToken token)
        {
            _isAvailable.Value = await _apiAvailabilityChecker.IsAvailable(
                _config.AvailabilityCheckUrl, _config.AvailabilityCode, token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
            _isAvailable.Dispose();
            _disposable.Dispose();
        }
    }
}