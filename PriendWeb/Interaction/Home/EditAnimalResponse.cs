using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Entity;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Home
{
    public sealed class EditAnimalResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            UnknownAnimal = 1,
            PasswordError = 2,
            AccountError = 3,
            ServerError = 4,
        }

        string IResponse.Path => "/ws/home/entity/animal/edit";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            long animalId = conn.Int64Message.Value;

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

            try
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

                    int groupId;
                    string hashedPassword = Account.Hash.Password(password);
                    cmd.CommandText = $"SELECT group_id FROM managed WHERE pet_id={animalId};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            groupId = reader.GetInt32(0);
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.UnknownAnimal);
                            return;
                        }
                    }

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
                            await conn.SendByteAsync((byte)EResponse.UnknownAnimal);
                            return;
                        }
                    }

                    cmd.CommandText = $"UPDATE animal SET species={species}, name={name}, birth={birthday}, sex={sex} WHERE id={animalId};";
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
            catch (NpgsqlException e)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
                throw e;
            }
        }
    }
}
