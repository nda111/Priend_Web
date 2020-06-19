using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Npgsql;
using PriendWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Data
{
    public sealed class SpeciesListResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            ServerError = 1,
        }

        string IResponse.Path => "/ws/data/species";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            try
            {
                JArray speciesArray = new JArray();
                using (var cmd = npgConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, en_us, ko_kr FROM animal_species;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader.GetInt64(0);
                            string enUs = reader.GetString(1);
                            string koKr = reader.GetString(2);

                            var species = new Species(id, enUs, koKr);
                            speciesArray.Add(species.ToJson());
                        }
                    }
                }

                await conn.SendByteAsync((byte)EResponse.Ok);
                await conn.SendTextAsync(speciesArray.ToString());
            }
            catch (NpgsqlException)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
            }
        }
    }
}
