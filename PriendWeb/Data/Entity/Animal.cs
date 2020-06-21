using Newtonsoft.Json.Linq;
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
        public long Id { get; private set; } = -1;

        /// <summary>
        /// Get the species ID animal
        /// </summary>
        public long Species { get; private set; } = -1;

        /// <summary>
        /// Get the birthday of animal
        /// </summary>
        public ulong Birthday { get; private set; } = 0;

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
        public SortedDictionary<ulong, double> Weights { get; private set; } = null;

        /// <summary>
        /// Create instance with data
        /// </summary>
        /// <param name="id">ID of the animal</param>
        /// <param name="species">Species ID of the animal</param>
        /// <param name="birthday">Birthday of the animal</param>
        /// <param name="name">Name of the animal</param>
        /// <param name="sex">Sex of the animal</param>
        /// <param name="weights">Dictionary of weight of the animal</param>
        public Animal(long id, long species, ulong birthday, string name, Sex sex, SortedDictionary<ulong, double> weights)
        {
            Id = id;
            Species = species;
            Birthday = birthday;
            Name = name;
            Sex = sex;
            Weights = weights;
        }

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
            long? id = null;
            string name = null;
            ulong? birthday = null;
            short? sex = null;
            long? species = null;
            JArray weightArray = null;

            JToken token;

            if (json.TryGetValue(JsonKeyId, out token))
            {
                id = token.ToObject<long>();
            }
            if (json.TryGetValue(JsonKeyName, out token))
            {
                name = token.ToObject<string>();
            }
            if (json.TryGetValue(JsonKeyBirthday, out token))
            {
                birthday = token.ToObject<ulong>();
            }
            if (json.TryGetValue(JsonKeySex, out token))
            {
                sex = token.ToObject<short>();
            }
            if (json.TryGetValue(JsonKeySpecies, out token))
            {
                species = token.ToObject<long>();
            }
            if (json.TryGetValue(JsonKeyWeight, out token))
            {
                weightArray = token.ToObject<JArray>();
            }

            if (new object[] { id, name, birthday, sex, species, weightArray }.All(o => o != null))
            {
                SortedDictionary<ulong, double> weights = new SortedDictionary<ulong, double>();
                foreach (var pairToken in weightArray)
                {
                    JObject pair = pairToken.ToObject<JObject>();

                    if (pair.TryGetValue(JsonKeyWeightDate, out var dateToken) && pair.TryGetValue(JsonKeyWeightValue, out var valueToken))
                    {
                        weights.Add(dateToken.ToObject<ulong>(), valueToken.ToObject<double>());
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
