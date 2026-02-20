using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Encryption
{
    /// <summary>
    /// Provides XOR encryption and decryption functionality with SHA256-based key generation.
    /// </summary>
    [PublicAPI]
    public static class XORDataEncryption
    {
        private static byte[] _xorKey;

        /// <summary>
        /// Encrypts the specified data using XOR encryption with the provided password.
        /// </summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <param name="password">The password used for key generation.</param>
        /// <returns>A Base64-encoded string containing the encrypted data.</returns>
        public static string Encrypt(string data, string password)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var encrypted = GenerateKey(dataBytes, password);

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts the specified encrypted data using XOR decryption with the provided password.
        /// </summary>
        /// <param name="encryptedData">The Base64-encoded encrypted data.</param>
        /// <param name="password">The password used for key generation.</param>
        /// <returns>The decrypted plaintext string.</returns>
        public static string Decrypt(string encryptedData, string password)
        {
            var data = Convert.FromBase64String(encryptedData);
            var decrypted = GenerateKey(data, password);
            return Encoding.UTF8.GetString(decrypted);
        }

        private static byte[] GetXorKey(string password)
        {
            if (_xorKey is not null)
                return _xorKey;

            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
            _xorKey = new byte[32];
            Array.Copy(hash, _xorKey, 32);
            return _xorKey;
        }

        private static byte[] GenerateKey(IReadOnlyList<byte> data, string password)
        {
            var key = GetXorKey(password);
            var decrypted = new byte[data.Count];

            for (var i = 0; i < data.Count; i++)
                decrypted[i] = (byte)(data[i] ^ key[i % key.Length]);

            return decrypted;
        }
    }
}