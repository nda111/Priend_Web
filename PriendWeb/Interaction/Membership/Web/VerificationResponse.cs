using Microsoft.AspNetCore.Http;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership.Web
{
    public sealed class VerificationResponse : IResponse
    {
        public enum EResponse
        { 
            Ok = 0,
            UnknownHash = 1,
            ServerError = 2,
        }

        string IResponse.Path => "/ws/membership/web/verify";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string hash = conn.TextMessage;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    string email = null;
                    string name = null;

                    cmd.CommandText = $"SELECT email FROM verifying_hash WHERE hash='{hash}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            email = reader.GetString(0);
                        }
                    }

                    if (email == null)
                    {
                        await conn.SendByteAsync((byte)EResponse.UnknownHash);
                    }
                    else
                    {
                        cmd.CommandText = $"SELECT name FROM account WHERE email='{email}';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            name = reader.GetString(0);
                        }

                        cmd.CommandText = 
                            $"DELETE FROM verifying_hash WHERE email='{email}';" +
                            $"UPDATE account SET verified=TRUE WHERE email='{email}';";
                        cmd.ExecuteNonQuery();

                        await conn.SendByteAsync((byte)EResponse.Ok);
                        await conn.SendTextAsync(name);
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
