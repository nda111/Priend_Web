using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Entity;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Home
{
    public sealed class JoinGroupResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            UnknownGroup = 1,
            PasswordError = 2,
            AccountError = 3,
            ServerError = 4,
        }

        string IResponse.Path => "/ws/home/entity/group/join";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            int groupId = conn.Int32Message.Value;

            await conn.ReceiveAsync();
            int password = conn.Int32Message.Value;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    if (!AuthorizationChecker.ValidateToken(cmd, id, authToken))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                        return;
                    }

                    string hashedPassword = Account.Hash.Password(password.ToString());
                    cmd.CommandText = $"SELECT passwd FROM animal_group WHERE id={groupId};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string passwd = reader.GetString(0);
                            if (passwd != hashedPassword)
                            {
                                await conn.SendByteAsync((byte)EResponse.PasswordError);
                                return;
                            }
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.UnknownGroup);
                            return;
                        }
                    }

                    cmd.CommandText = $"INSERT INTO participates VALUES({groupId}, {id});";
                    cmd.ExecuteNonQuery();

                    await conn.SendByteAsync((byte)EResponse.Ok);
                }
            }
            catch (NpgsqlException)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
            }
        }
    }
}
