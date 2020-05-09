using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Data
{
    /// <summary>
    /// A class represents settings
    /// </summary>  
    public struct Settings : IJsonConvertible
    {
        private const string JsonKeyWeightAlert = "weight";
        private const string JsonKeyBirthday = "birthday";
        private const string JsonKeyEventAlert = "event";
        private const string JsonKeyCommentAlert = "comment";

        /// 
        /// Weather make weight alert or not
        /// 
        public bool DoWeightAlert { get; set; }

        /// 
        /// Weather make birthday alert or not
        /// 
        public bool DoBirthdayAlert { get; set; }

        /// 
        /// Weather make event alert or not
        /// 
        public bool DoEventAlert { get; set; }

        /// 
        /// Weather make comment alert or not
        /// 
        public bool DoCommentAlert { get; set; }

        /// <summary>
        /// Create instance with data
        /// </summary>
        /// <param name="weightAlert">Do weight alert</param>
        /// <param name="birthdayAlert">Do birthday alert</param>
        /// <param name="eventAlert">Do event alert</param>
        /// <param name="commentAlert">do comment alert</param>
        public Settings(bool weightAlert, bool birthdayAlert, bool eventAlert, bool commentAlert)
        {
            DoWeightAlert = weightAlert;
            DoBirthdayAlert = birthdayAlert;
            DoEventAlert = eventAlert;
            DoCommentAlert = commentAlert;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();

            json.Add(JsonKeyWeightAlert, DoWeightAlert);
            json.Add(JsonKeyBirthday, DoBirthdayAlert);
            json.Add(JsonKeyEventAlert, DoEventAlert);
            json.Add(JsonKeyCommentAlert, DoCommentAlert);

            return json;
        }

        public bool ReadJson(JObject json)
        {
            bool? weight = null;
            bool? birthday = null;
            bool? @event = null;
            bool? comment = null;
            JToken token;

            if (json.TryGetValue(JsonKeyWeightAlert, out token))
            {
                weight = token.ToObject<bool>();
            }
            if (json.TryGetValue(JsonKeyBirthday, out token))
            {
                birthday = token.ToObject<bool>();
            }
            if (json.TryGetValue(JsonKeyEventAlert, out token))
            {
                @event = token.ToObject<bool>();
            }
            if (json.TryGetValue(JsonKeyCommentAlert, out token))
            {
                comment = token.ToObject<bool>();
            }

            if (new bool?[] { weight, birthday, @event, comment }.All(b => b != null))
            {
                DoWeightAlert = weight.Value;
                DoBirthdayAlert = birthday.Value;
                DoEventAlert = @event.Value;
                DoCommentAlert = comment.Value;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
