using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Commit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Calendar
{
    public sealed class UpdateMemoResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/calendar/memo/update";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            long animalId = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string commitJson = conn.TextMessage;

            Memo.Commit commit = new Memo.Commit();
            commit.ReadJson(JObject.Parse(commitJson));

            if (commit.HasChange)
            {
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

                    // Update memo on the database
                    var cmdBuilder = new StringBuilder("UPDATE memo SET ");

                    var changeQueryList = new List<string>();
                    if (commit.Title != null)
                    {
                        changeQueryList.Add($"title='{commit.Title}'");
                    }
                    if (commit.Content != null)
                    {
                        changeQueryList.Add($"content='{commit.Content}'");
                    }
                    if (commit.PhotoString != null)
                    {
                        changeQueryList.Add($"images='{commit.PhotoString}'");
                    }
                    cmdBuilder.Append(string.Join(", ", changeQueryList));
                    cmdBuilder.Append($" WHERE id={commit.Id};");

                    cmd.CommandText = cmdBuilder.ToString();
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
            else
            {
                await conn.SendByteAsync((byte)EResponse.Ok);
            }
        }
    }
}
