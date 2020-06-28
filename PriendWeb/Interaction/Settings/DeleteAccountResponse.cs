using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Settings
{
    public sealed class DeleteAccountResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/settings/account/delete";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string token = conn.TextMessage;

            using (var cmd = npgConn.CreateCommand())
            {
                if (!AuthorizationChecker.ValidateToken(cmd, id, token))
                {
                    await conn.SendByteAsync((byte)EResponse.AccountError);
                    return;
                }

                try
                {
                    cmd.CommandText = $"DELETE FROM account WHERE id={id};";
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
