using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership.Web
{
    public sealed class ResetPasswordWebResponse : IResponse
    {
        public enum EResponse
        {
            Ok = 0,
            Used = 1,
            Expired = 2,
            UnknownHash = 3,
            ServerError = 4,
        }

        string IResponse.Path => "/ws/membership/web/reset";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string hash = conn.TextMessage;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    string email = null;
                    long expireDue = -1;
                    bool bUsed = false;

                    cmd.CommandText = $"SELECT (email, expire_due, used) FROM reset_password WHERE hash='{hash}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            email = reader.GetString(0);
                            expireDue = reader.GetInt64(1);
                            bUsed = reader.GetBoolean(2);
                        }
                    }

                    if (email == null)
                    {
                        await conn.SendByteAsync((byte)EResponse.UnknownHash);
                    }
                    else if (bUsed)
                    {
                        await conn.SendByteAsync((byte)EResponse.Used);
                    }
                    else if (DateTime.Now.Ticks <= expireDue)
                    {
                        await conn.SendByteAsync((byte)EResponse.Expired);
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
