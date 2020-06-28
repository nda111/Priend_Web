using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Settings
{
    public sealed class ChangeNameResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/settings/account/change";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string token = conn.TextMessage;

            await conn.ReceiveAsync();
            string name = conn.TextMessage;

            using (var cmd = npgConn.CreateCommand())
            {
                if (!AuthorizationChecker.ValidateToken(cmd, id, token))
                {
                    await conn.SendByteAsync((byte)EResponse.AccountError);
                    return;
                }

                try
                {
                    cmd.CommandText = $"UPDATE account SET name='{name}' WHERE id={id};";
                    cmd.ExecuteNonQuery();
                }
                catch (NpgsqlException)
                {
                    await conn.SendByteAsync((byte)EResponse.ServerError);
                    return;
                }

                await conn.SendByteAsync((byte)EResponse.Ok);
            }
        }
    }
}
