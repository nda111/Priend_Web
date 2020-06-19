using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Entity;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Home
{
    public sealed class CreateGroupReponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/home/entity/group/create";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long userId = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            string groupName = conn.TextMessage;

            await conn.ReceiveAsync();
            string password = Account.Hash.Password(conn.Int32Message.Value.ToString());

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    if (!AuthorizationChecker.ValidateToken(cmd, userId, authToken))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                        return;
                    }

                    int groupId;
                    cmd.CommandText = $"INSERT (owner_id, name, passwd) INTO animal_group VALUES({userId}, '{groupName}', '{password}') RETURNING id;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            groupId = reader.GetInt32(0);
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.ServerError);
                            return;
                        }
                    }

                    cmd.CommandText = $"INSERT INTO participates VALUES({groupId}, {userId});";
                    cmd.ExecuteNonQuery();

                    await conn.SendByteAsync((byte)EResponse.Ok);
                }
            }
            catch (NpgsqlException e)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
                throw e;
            }
        }
    }
}
