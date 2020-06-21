using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Data.Commit
{
    /// <summary>
    /// 메모를 나타내는 데이터 클래스
    /// </summary>
    public sealed class Memo : IJsonConvertible
    {
        private const string JsonKeyId = "id";
        private const string JsonKeyWhen = "when";
        private const string JsonKeyTitle = "title";
        private const string JsonKeyContent = "content";
        private const string JsonKeyPhoto = "photo";

        public sealed class Commit : IJsonConvertible
        {
            public long Id { get; private set; }

            public string Title { get; private set; } = null;

            public string Content { get; private set; } = null;

            public string PhotoString { get; private set; } = null;

            public bool HasChange => Title != null || Content != null || PhotoString != null;

            public bool ReadJson(JObject json)
            {
                JToken token;

                if (json.TryGetValue(JsonKeyId, out token))
                {
                    Id = token.ToObject<long>();

                    if (json.TryGetValue(JsonKeyTitle, out token))
                    {
                        Title = token.ToObject<string>();
                    }

                    if (json.TryGetValue(JsonKeyContent, out token))
                    {
                        Content = token.ToObject<string>();
                    }

                    if (json.TryGetValue(JsonKeyPhoto, out token))
                    {
                        PhotoString = token.ToObject<string>();
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
                throw new NotImplementedException();
            }
        }

        public Memo() { }

        public Memo(long id, ulong when, string title, string content, string photoString = null)
        {
            Id = id;
            When = when;
            Title = title;
            Content = content;
            PhotoString = photoString;
        }

        public long Id { get; private set; } = -1;

        public ulong When { get; private set; } = 0;

        public string Title { get; private set; } = null;

        public string Content { get; private set; } = null;

        public string PhotoString { get; private set; } = null;

        public bool ReadJson(JObject json)
        {
            try
            {
                long id = json.GetValue(JsonKeyId).ToObject<long>();
                ulong when = json.GetValue(JsonKeyWhen).ToObject<ulong>();
                string title = json.GetValue(JsonKeyTitle).ToObject<string>();
                string text = json.GetValue(JsonKeyContent).ToObject<string>();
                string photoString = json.GetValue(JsonKeyPhoto)?.ToObject<string>();

                Id = id;
                When = when;
                Title = title;
                Content = text;
                PhotoString = photoString;

                return true;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public JObject ToJson()
        {
            JObject json = new JObject();

            json.Add(JsonKeyId, Id);
            json.Add(JsonKeyWhen, When);
            json.Add(JsonKeyTitle, Title);
            json.Add(JsonKeyContent, Content);

            if (PhotoString != null)
            {
                json.Add(JsonKeyPhoto, PhotoString);
            }

            return json;
        }
    }
}
