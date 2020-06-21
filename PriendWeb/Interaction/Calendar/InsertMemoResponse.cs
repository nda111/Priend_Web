using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Commit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Calendar
{
    public sealed class InsertMemoResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/calendar/memo/insert";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {            
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            long animalId = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string memoJson = conn.TextMessage;

            Memo memo = new Memo();
            memo.ReadJson(JObject.Parse(memoJson));

            using (var cmd = npgConn.CreateCommand())
            {
                if (!AuthorizationChecker.ValidateToken(cmd, id, authToken))
                {
                    await conn.SendByteAsync((byte)EResponse.AccountError);
                    return;
                }

                if (!AuthorizationChecker.CheckAuthorizationOnAnimal(cmd, id, animalId))
                {
                    await conn.SendByteAsync((byte)EResponse.AccountError);
                    return;
                }

                // Insert new memo on the database
                if (memo.PhotoString == null)
                {
                    cmd.CommandText = $"INSERT INTO memo (animal_id, date_time, title, content) VALUES({memo.Id}, {memo.When}, '{memo.Title}', '{memo.Content}');";
                }
                else
                {
                    cmd.CommandText = $"INSERT INTO memo (animal_id, date_time, title, content, images) VALUES({memo.Id}, {memo.When}, '{memo.Title}', '{memo.Content}', '{memo.PhotoString}');";
                }

                if (cmd.ExecuteNonQuery() == 0)
                {
                    await conn.SendByteAsync((byte)EResponse.ServerError);
                }
                else
                {
                    await conn.SendByteAsync((byte)EResponse.Ok);
                }
            }
        }
    }
}
