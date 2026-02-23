using CustomUtils.Runtime.Encryption;
using CustomUtils.Runtime.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Api.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for API configuration with encryption support.
    /// </summary>
    [PublicAPI]
    public abstract class ApiConfigBase : ScriptableObject
    {
        /// <summary>
        /// Gets the interval in seconds for checking API availability.
        /// </summary>
        [field: SerializeField] public float UpdateAvailabilityInterval { get; private set; }

        /// <summary>
        /// Gets the URL used for availability checks.
        /// </summary>
        [field: SerializeField] public string AvailabilityCheckUrl { get; private set; }

        /// <summary>
        /// Gets the expected HTTP response code indicating availability.
        /// </summary>
        [field: SerializeField] public long AvailabilityCode { get; private set; }

        /// <summary>
        /// Gets the environment variable name for the encryption key.
        /// </summary>
        [field: SerializeField] public string EncryptionKeyEnvironmentName { get; private set; }

        [SerializeField] protected string endpointFormat;

        [SerializeField] private string _apiKeyEnvironmentName;

        /// <summary>
        /// Base path for API configuration assets.
        /// </summary>
        protected const string ApiConfigPath = "Api Configs /";

        private string _encryptedApiKey;
        private string _password;

        /// <summary>
        /// Initializes the configuration with an encrypted API key.
        /// </summary>
        /// <param name="encryptedApiKey">The encrypted API key.</param>
        public void Initialize(string encryptedApiKey)
        {
            _encryptedApiKey = encryptedApiKey;
        }

        /// <summary>
        /// Gets the formatted API URL for requests.
        /// </summary>
        /// <returns>The API URL.</returns>
        public abstract string GetApiUrl();

        /// <summary>
        /// Validates the configuration state.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        public virtual bool IsValid() => string.IsNullOrEmpty(endpointFormat) is false && IsEncryptionValid();

        /// <summary>
        /// Sets the password for API key decryption.
        /// </summary>
        /// <param name="password">The decryption password.</param>
        public void SetPassword(string password) => _password = password;

        /// <summary>
        /// Gets the decrypted API key.
        /// </summary>
        /// <returns>The decrypted API key.</returns>
        protected string GetApiKey()
        {
            if (Application.isEditor is false)
                return XORDataEncryption.Decrypt(_encryptedApiKey, _password);

            _apiKeyEnvironmentName.TryGetValueFromEnvironment(out var apiKey);
            return apiKey;
        }

        private bool IsEncryptionValid()
        {
            if (Application.isEditor)
                return true;

            return string.IsNullOrEmpty(_password) is false && string.IsNullOrEmpty(_encryptedApiKey) is false;
        }
    }
}