using System;
using System.Text;
using System.Threading;
using CustomUtils.Runtime.Api.Interfaces;
using CustomUtils.Runtime.Formatter;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomUtils.Runtime.Api
{
    [PublicAPI]
    public sealed class ApiClient : IApiClient
    {
        public async UniTask<TResponse> PostAsync<TRequest, TResponse>(
            TRequest request,
            string url,
            CancellationToken token)
            where TRequest : class
            where TResponse : class
        {
            var response = await SendRequestAsync<object, TRequest>(null, request, url, token);
            var parsedResponse = ParseResponse<TResponse>(response);
            return parsedResponse;
        }

        public async UniTask<TResponse> PostAsync<TSource, TRequest, TResponse>(
            TSource source,
            TRequest request,
            string url,
            CancellationToken token,
            Action<TSource, UnityWebRequest> additionalHeaders)
            where TRequest : class
            where TResponse : class
        {
            var response = await SendRequestAsync(source, request, url, token, additionalHeaders);
            var parsedResponse = ParseResponse<TResponse>(response);
            return parsedResponse;
        }

        private async UniTask<string> SendRequestAsync<TSource, TData>(
            TSource source,
            TData data,
            string url,
            CancellationToken token,
            Action<TSource, UnityWebRequest> additionalHeaders = null)
            where TData : class
        {
            using var request = new UnityWebRequest(url, "POST");
            var jsonData = JsonConvert.SerializeObject(data);
            var bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            additionalHeaders?.Invoke(source, request);

            try
            {
                await request.SendWebRequest().WithCancellation(token);

                if (request.result == UnityWebRequest.Result.Success)
                    return request.downloadHandler.text;

                var message = StringFormatter.Format("[AppHelper::SendRequestAsync] " +
                                                     "Request failed: {0} " +
                                                     "Response Code: {1}" +
                                                     "Response Headers: {2} " +
                                                     "Response Body: {3} With Json Data: {4}",
                    request.responseCode, request.GetResponseHeaders(), request.downloadHandler?.text, jsonData);

                Debug.LogError(message);
                return null;
            }
            catch (Exception e)
            {
                var message = StringFormatter.Format(
                    "[AppHelper::SendRequestAsync] Request failed with exception: {0}" +
                    "Stack trace: {1}" +
                    "Request failed: {2}" +
                    "With Json Data: {3}",
                    e, e.StackTrace, e.Message, jsonData);

                Debug.LogError(message);
                return null;
            }
        }

        private TResponse ParseResponse<TResponse>(string responseJson)
            where TResponse : class
        {
            try
            {
                var response = JsonConvert.DeserializeObject<TResponse>(responseJson);

                if (response != null)
                    return response;

                Debug.LogWarning($"[AppHelper::ParseResponse] Failed to parse response with {responseJson} json");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AppHelper::ParseResponse] Failed to parse response: {e.Message}");
                return null;
            }
        }
    }
}