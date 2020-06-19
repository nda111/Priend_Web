using Newtonsoft.Json.Linq;

namespace PriendWeb.Data
{
    public struct Species : IJsonConvertible
    {
        private const string JsonKeyId = "id";
        private const string JsonKeyEnUs = "en_us";
        private const string JsonKeyKoKr = "ko_kr";

        public long ID;
        public string EnUs;
        public string KoKr;

        public Species(long id, string enUs, string koKr)
        {
            ID = id;
            EnUs = enUs;
            KoKr = koKr;
        }

        public bool ReadJson(JObject json)
        {
            return false;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();

            json.Add(JsonKeyId, ID);
            json.Add(JsonKeyEnUs, EnUs);
            json.Add(JsonKeyKoKr, KoKr);

            return json;
        }
    }
}
