using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Home
{
    public sealed class DeleteAnimalResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            AccountError = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/home/entity/animal/delete";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            long animalId = conn.Int64Message.Value;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    if (!AuthorizationChecker.ValidateToken(cmd, id, authToken))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                    }

                    if (!AuthorizationChecker.CheckAuthorizationOnAnimal(cmd, id, animalId))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                    }

                    cmd.CommandText = 
                        $"DELETE FROM weights WHERE pet_id={animalId};" +
                        $"DELETE FROM animal WHERE id={animalId};";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
            }
        }
    }
}
