﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Data.Entity
{
    /// <summary>
    /// A class represents managed animal
    /// </summary>
    public sealed class Animal : IJsonConvertible
    {
        private const string JsonKeyId = "id";
        private const string JsonKeySpecies = "species";
        private const string JsonKeyBirthday = "birthday";
        private const string JsonKeyName = "name";
        private const string JsonKeySex = "sex";
        private const string JsonKeyWeight = "weights";
        private const string JsonKeyWeightDate = "date";
        private const string JsonKeyWeightValue = "value";

        /// <summary>
        /// Get the ID of animal
        /// </summary>
        public int Id { get; private set; } = -1;

        /// <summary>
        /// Get the species ID animal
        /// </summary>
        public int Species { get; private set; } = -1;

        /// <summary>
        /// Get the birthday of animal
        /// </summary>
        public long Birthday { get; private set; } = -1;

        /// <summary>
        /// Get the name of aniaml
        /// </summary>
        public string Name { get; private set; } = null;

        /// <summary>
        /// Get the sex of animal
        /// </summary>
        public Sex Sex { get; private set; } = default;

        /// <summary>
        /// Get the list of weight of animal
        /// </summary>
        public SortedDictionary<long, double> Weights { get; private set; } = null;

        public JObject ToJson()
        {
            JArray weightArray = new JArray();
            foreach (var entry in Weights)
            {
                JObject weight = new JObject();
                weight.Add(JsonKeyWeightDate, entry.Key);
                weight.Add(JsonKeyWeightValue, entry.Value);

                weightArray.Add(weight);
            }

            JObject json = new JObject()
            {
                { JsonKeyId, Id },
                { JsonKeySpecies, Species },
                { JsonKeyBirthday, Birthday },
                { JsonKeyName, Name },
                { JsonKeySex, (short)Sex },
                { JsonKeyWeight, weightArray },
            };

            return json;
        }

        public bool ReadJson(JObject json)
        {
            int? id = null;
            string name = null;
            long? birthday = null;
            short? sex = null;
            int? species = null;
            JArray weightArray = null;

            JToken token;

            if (json.TryGetValue(JsonKeyId, out token))
            {
                id = token.ToObject<int>();
            }
            if (json.TryGetValue(JsonKeyName, out token))
            {
                name = token.ToObject<string>();
            }
            if (json.TryGetValue(JsonKeyBirthday, out token))
            {
                birthday = token.ToObject<long>();
            }
            if (json.TryGetValue(JsonKeySex, out token))
            {
                sex = token.ToObject<short>();
            }
            if (json.TryGetValue(JsonKeySpecies, out token))
            {
                species = token.ToObject<int>();
            }
            if (json.TryGetValue(JsonKeyWeight, out token))
            {
                weightArray = token.ToObject<JArray>();
            }

            if (new object[] { id, name, birthday, sex, species, weightArray }.All(o => o != null))
            {
                SortedDictionary<long, double> weights = new SortedDictionary<long, double>();
                foreach (var pairToken in weightArray)
                {
                    JObject pair = pairToken.ToObject<JObject>();

                    if (pair.TryGetValue(JsonKeyWeightDate, out var dateToken) && pair.TryGetValue(JsonKeyWeightValue, out var valueToken))
                    {
                        weights.Add(dateToken.ToObject<long>(), valueToken.ToObject<double>());
                    }
                    else
                    {
                        return false;
                    }
                }

                Id = id.Value;
                Name = name;
                Birthday = birthday.Value;
                Sex = (Sex)sex;
                Species = species.Value;
                Weights = weights;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
