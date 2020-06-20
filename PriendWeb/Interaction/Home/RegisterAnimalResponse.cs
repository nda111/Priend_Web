using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Entity;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Home
{
    public sealed class RegisterAnimalResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            UnknownGroup = 1,
            PasswordError = 2,
            AccountError = 3,
            ServerError = 4,
        }

        string IResponse.Path => "/ws/home/entity/animal/register";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            int groupId = conn.Int32Message.Value;

            await conn.ReceiveAsync();
            string password = conn.TextMessage;

            await conn.ReceiveAsync();
            string name = conn.TextMessage;

            await conn.ReceiveAsync();
            long birthday = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            short sex = conn.Int16Message.Value;

            await conn.ReceiveAsync();
            long species = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            double weight = double.Parse(conn.TextMessage);

            await conn.ReceiveAsync();
            long today = conn.Int64Message.Value;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    if (!AuthorizationChecker.ValidateToken(cmd, id, authToken))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                        return;
                    }

                    if (!AuthorizationChecker.CheckAuthorizationOnGroup(cmd, id, groupId))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                        return;
                    }

                    string hashedPassword = Account.Hash.Password(password);
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

                    long animalId;
                    cmd.CommandText = $"INSERT INTO animal (species, name, birth, sex) VALUES({species}, '{name}', {birthday}, {sex}) RETURNING id;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            animalId = reader.GetInt64(0);
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.ServerError);
                            return;
                        }
                    }

                    cmd.CommandText = $"INSERT INTO participates VALUES({groupId}, {animalId});";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = $"INSERT INTO weights VALUES({animalId}, {today}, {weight});";
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
