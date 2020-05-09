using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PriendWeb.Data.Entity
{
    /// <summary>
    /// A class represents account
    /// </summary>
    public sealed class Account : IJsonConvertible
    {
        private const string JsonKeyId = "id";
        private const string JsonKeyName = "name";
        private const string JsonKeyEmail = "email";
        private const string JsonKeyAuthenticationToken = "authToken";
        private const string JsonKeySettings = "settings";

        /// <summary>
        /// Get the ID of account
        /// </summary>
        public long Id { get; private set; } = -1;

        /// <summary>
        /// Get or set the name of account
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Get the email of account
        /// </summary>
        public string Email { get; private set; } = null;

        /// <summary>
        /// Get the authentication token of account
        /// </summary>
        public string AuthenticationToken { get; private set; } = null;

        /// <summary>
        /// Get the account settings
        /// </summary>
        public Settings Settings { get; private set; } = default;

        /// <summary>
        /// Create instance with data
        /// </summary>
        /// <param name="id">Id of the account</param>
        /// <param name="name">Name of the owner of the account</param>
        /// <param name="email">Email addresss of the account</param>
        /// <param name="authToken">Authentication token of the account</param>
        /// <param name="settings">Setting informations of the account</param>
        public Account(long id, string name, string email, string authToken, Settings settings)
        {
            Id = id;
            Name = name;
            Email = email;
            AuthenticationToken = authToken;
            Settings = settings;

        }

        public JObject ToJson()
        {
            JObject json = new JObject();

            json.Add(JsonKeyId, Id);
            json.Add(JsonKeyName, Name);
            json.Add(JsonKeyEmail, Email);
            json.Add(JsonKeyAuthenticationToken, AuthenticationToken);
            json.Add(JsonKeySettings, Settings.ToJson());

            return json;
        }

        public bool ReadJson(JObject json)
        {
            long? id = null;
            string name = null;
            string email = null;
            string authenticationToken = null;
            JObject settingsJson = null;
            Settings? settings = null;

            JToken token;

            if (json.TryGetValue(JsonKeyId, out token))
            {
                id = token.ToObject<long>();
            }
            if (json.TryGetValue(JsonKeyName, out token))
            {
                name = token.ToObject<string>();
            }
            if (json.TryGetValue(JsonKeyEmail, out token))
            {
                email = token.ToObject<string>();
            }
            if (json.TryGetValue(JsonKeyAuthenticationToken, out token))
            {
                authenticationToken = token.ToObject<string>();
            }
            if (json.TryGetValue(JsonKeySettings, out token))
            {
                settingsJson = token.ToObject<JObject>();

                settings = new Settings();
                settings.Value.ReadJson(settingsJson);
            }

            if (new object[] { id, name, email, authenticationToken, settings }.All(o => o != null))
            {
                Id = id.Value;
                Name = name;
                Email = email;
                AuthenticationToken = authenticationToken;
                Settings = settings.Value;

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A class that contains SHA-256 hashing methods
        /// </summary>
        internal static class Hash
        {
            /// <summary>
            /// Hash password
            /// </summary>
            /// <param name="password">Password string to hash</param>
            /// <returns>Hashed password string</returns>
            public static string Password(string password)
            {
                return ToHexString(HashString(password + "rice-burger"));
            }

            /// <summary>
            /// Hash verification key
            /// </summary>
            /// <param name="email">Email of an account to verify</param>
            /// <returns>Hashed verification key</returns>
            public static string VerificationKey(string email)
            {
                return ToHexString(HashString(email + "waffle"));
            }

            /// <summary>
            /// Hash reset password key
            /// </summary>
            /// <param name="email">Email of an account to reset password</param>
            /// <param name="due">The expiration date of key</param>
            /// <returns>Hashed reset password key</returns>
            public static string ResetPasswordKey(string email, DateTime due)
            {
                return ToHexString(HashString(email + due.Ticks.ToString() + "pizza"));
            }

            /// <summary>
            /// Hash authentication token
            /// </summary>
            /// <param name="email">Email of an account to authenticate</param>
            /// <param name="password">Password of an account to authenticate</param>
            /// <returns>Hashed authentication key</returns>
            public static string AuthenticationToken(string email, string password)
            {
                return ToHexString(HashString(email + password + DateTime.Now.Ticks + "hashbrown"));
            }

            private static byte[] HashString(string input)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                }
            }

            private static string ToHexString(byte[] buffer)
            {
                const string HexDigits = "0123456789ABCDEF";

                StringBuilder resultBuilder = new StringBuilder();
                foreach (byte bt in buffer)
                {
                    resultBuilder.Append(HexDigits[bt >> 4]);
                    resultBuilder.Append(HexDigits[bt & 0xF]);
                }

                return resultBuilder.ToString();
            }
        }
    }
}
