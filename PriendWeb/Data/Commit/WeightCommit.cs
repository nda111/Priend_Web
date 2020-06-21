using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Data.Commit
{
    /// <summary>
    /// 몸무게 업데이트에 대한 커미션을 나타내는 클래스
    /// </summary>
    public sealed class WeightCommit : IJsonConvertible
    {
        /// <summary>
        /// 한 개의 몸무게 업데이트를 나타내는 클래스
        /// </summary>
        public struct Change : IJsonConvertible
        {
            private const string JsonKeyType = "type";
            private const string JsonKeyDate = "date";
            private const string JsonKeyWeight = "weight";

            private const string JsonValueUpsert = "ups";
            private const string JsonValueDelete = "del";

            public enum Type : byte
            {
                Upsert, Delete
            }

            public Type ChangeType { get; private set; }

            public ulong Date { get; private set; }

            public double Weight { get; private set; }

            public bool ReadJson(JObject json)
            {
                JToken typeToken;
                JToken dateToken;
                JToken weightToken;

                if (!json.TryGetValue(JsonKeyType, out typeToken) ||
                    !json.TryGetValue(JsonKeyDate, out dateToken) ||
                    !json.TryGetValue(JsonKeyWeight, out weightToken))
                {
                    return false;
                }

                switch (typeToken.ToObject<string>())
                {
                    case JsonValueUpsert:
                        ChangeType = Type.Upsert;
                        break;

                    case JsonValueDelete:
                        ChangeType = Type.Delete;
                        break;

                    default:
                        return false;
                }

                if (dateToken.Type == JTokenType.Integer && weightToken.Type == JTokenType.Float)
                {
                    Date = dateToken.ToObject<ulong>();
                    Weight = weightToken.ToObject<double>();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public JObject ToJson()
            {
                JObject json = new JObject();

                switch (ChangeType)
                {
                    case Type.Upsert:
                        json.Add(JsonKeyType, JsonValueUpsert);
                        break;

                    case Type.Delete:
                        json.Add(JsonKeyType, JsonValueDelete);
                        break;

                    default:
                        return null;
                }

                json.Add(JsonKeyDate, Date);
                json.Add(JsonKeyWeight, Weight);

                return json;
            }
        }

        private const string JsonKeyId = "id";
        private const string JsonKeyChanges = "changes";

        /// <summary>
        /// 몸무게를 업데이트할 동물의 식별자
        /// </summary>
        public long AnimalId { get; private set; } = 0;

        /// <summary>
        /// 몸무게 업데이트에 대한 리스트
        /// </summary>
        public SortedList<ulong, Change> Changes { get; } = new SortedList<ulong, Change>();

        public bool ReadJson(JObject json)
        {
            JToken idToken;
            JToken arrayToken;

            if (json.TryGetValue(JsonKeyId, out idToken) && 
                json.TryGetValue(JsonKeyChanges, out arrayToken))
            {
                long id = idToken.ToObject<long>();
                JArray changes = arrayToken.ToObject<JArray>();

                LinkedList<Change> changeList = new LinkedList<Change>();

                foreach (JToken token in changes)
                {
                    var change = new Change();
                    if (change.ReadJson(token.ToObject<JObject>()))
                    {
                        changeList.AddLast(change);
                    }
                    else
                    {
                        return false;
                    }
                }

                AnimalId = id;

                Changes.Clear();
                foreach (var change in changeList)
                {
                    Changes.Add(change.Date, change);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public JObject ToJson()
        {
            return null;
        }
    }
}
