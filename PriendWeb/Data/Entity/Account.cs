using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "rice-burger"));

                const string HexDigits = "0123456789ABCDEF";
                StringBuilder resultBuilder = new StringBuilder();
                foreach (byte bt in hashed)
                {
                    resultBuilder.Append(HexDigits[bt >> 4]);
                    resultBuilder.Append(HexDigits[bt & 0xF]);
                }

                return resultBuilder.ToString();
            }
        }
    }
}
