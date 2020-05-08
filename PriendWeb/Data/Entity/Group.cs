using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Data.Entity
{
    /// <summary>
    /// A class represents group of managed animal
    /// </summary>
    public sealed class Group : IJsonConvertible
    {
        private const string JsonKeyId = "id";
        private const string JsonKeyOwner = "owner";
        private const string JsonKeyName = "name";

        /// <summary>
        /// Get the ID of group
        /// </summary>
        public int Id { get; private set; } = -1;

        /// <summary>
        /// Get the ID of owner of group
        /// </summary>
        public long OwnerId { get; private set; } = -1;

        /// <summary>
        /// Get the name of group
        /// </summary>
        public string Name { get; private set; } = null;

        public JObject ToJson()
        {
            JObject json = new JObject();

            json.Add(JsonKeyId, Id);
            json.Add(JsonKeyOwner, OwnerId);
            json.Add(JsonKeyName, Name);

            return json;
        }

        public bool ReadJson(JObject json)
        {
            int? id = null;
            long? owner = null;
            string? name = null;

            JToken token;

            if (json.TryGetValue(JsonKeyId, out token))
            {
                id = token.ToObject<int>();
            }
            if (json.TryGetValue(JsonKeyOwner, out token))
            {
                owner = token.ToObject<long>();
            }
            if (json.TryGetValue(JsonKeyName, out token))
            {
                name = token.ToObject<string>();
            }

            if (new object?[] { id, owner, name }.All(o => o != null))
            {
                Id = id.Value;
                OwnerId = owner.Value;
                Name = name;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
