using Microsoft.AspNetCore.Http;
using Npgsql;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership
{
    public sealed class EvaluationResponse : IResponse
    {
        private enum EResponse : byte
        {
            Verified        = 0,
            NotVerified     = 1,
            Unknown         = 2,
            ServerError     = 3,
        }

        public string Path => "/ws/membership/evaluation";

        public async Task Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string email = conn.TextMessage;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT (verified) FROM account WHERE email='{email}';";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            bool bVerified = reader.GetBoolean(0);

                            if (bVerified)
                            {
                                await conn.SendByteAsync((byte)EResponse.Verified);
                            }
                            else
                            {
                                await conn.SendByteAsync((byte)EResponse.NotVerified);
                            }
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.Unknown);
                        }
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
