using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PriendWeb.Data
{
    /// <summary>
    /// An interface represents object that is able to converted to JSON object.
    /// </summary>
    public interface IJsonConvertible
    {
        /// <summary>
        /// Converts it into a JSON object.
        /// </summary>
        /// <returns>Converted JSON object</returns>
        JObject ToJson();

        /// <summary>
        /// Read date from JSON Object.
        /// </summary>
        /// <param name="json">JSON object to read.</param>
        /// <returns>True if succeed, False otherwise.</returns>
        bool ReadJson(JObject json);
    }
}
