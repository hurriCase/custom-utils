using System;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Encryption
{
    /// <summary>
    /// Provides AES encryption and decryption functionality with password-based key derivation.
    /// </summary>
    [PublicAPI]
    public static class AESDataEncryption
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int IvSize = 16;
        private const int Iterations = 10000;

        /// <summary>
        /// Encrypts the specified data using AES encryption with the provided password.
        /// </summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <param name="password">The password used for key derivation.</param>
        /// <returns>A Base64-encoded string containing the encrypted data with salt and IV.</returns>
        public static string Encrypt(string data, string password)
        {
            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
                random.GetBytes(iv);
            }

            var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
                .GetBytes(KeySize);

            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                var encryptor = aes.CreateEncryptor();
                encrypted = encryptor.TransformFinalBlock(
                    Encoding.UTF8.GetBytes(data), 0, data.Length);
            }

            var result = new byte[SaltSize + IvSize + encrypted.Length];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
            Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
            Buffer.BlockCopy(encrypted, 0, result, SaltSize + IvSize, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts the specified encrypted data using AES decryption with the provided password.
        /// </summary>
        /// <param name="encryptedData">The Base64-encoded encrypted data containing salt, IV, and ciphertext.</param>
        /// <param name="password">The password used for key derivation.</param>
        /// <returns>The decrypted plaintext string.</returns>
        public static string Decrypt(string encryptedData, string password)
        {
            var data = Convert.FromBase64String(encryptedData);

            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            var encrypted = new byte[data.Length - SaltSize - IvSize];

            Buffer.BlockCopy(data, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(data, SaltSize, iv, 0, IvSize);
            Buffer.BlockCopy(data, SaltSize + IvSize, encrypted, 0, encrypted.Length);

            var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
                .GetBytes(KeySize);

            byte[] decrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                var decryptor = aes.CreateDecryptor();
                decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            }

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}