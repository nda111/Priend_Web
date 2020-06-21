using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Calendar
{
    public sealed class DeleteMemoResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/calendar/memo/delete";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            long animalId = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            long memoId = conn.Int64Message.Value;

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

                // Actual deletion work
                cmd.CommandText = $"DELETE FROM memo WHERE id={memoId};";
                if (cmd.ExecuteNonQuery() == 0)
                {
                    // On DB error
                    await conn.SendByteAsync((byte)EResponse.ServerError);
                }
                else
                {
                    // Success
                    await conn.SendByteAsync((byte)EResponse.Ok);
                }
            }
        }
    }
}
