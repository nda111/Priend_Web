using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership.Web
{
    public sealed class ResetPasswordConfirmResponse : IResponse
    {
        public enum EResponse
        {
            Ok = 0,
            Used = 1,
            Expired = 2,
            UnknownHash = 3,
            InvalidPattern = 4,
            ServerError = 5,
        }

        public string Path => $"/ws/membership/web/reset/confirm";

        public async Task Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string hash = conn.TextMessage;

            await conn.ReceiveAsync();
            string password = conn.TextMessage;

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
                    else if (!Regex.IsMatch(password, "((?=.*[a-z])(?=.*[0-9])(?=.*[!@#$%^&*])(?=.*[A-Z]).{8,})"))
                    {
                        await conn.SendByteAsync((byte)EResponse.InvalidPattern);
                    }
                    else
                    {
                        cmd.CommandText = $"UPDATE account SET password='{Account.Hash.Password(password)}' WHERE email='{email}';";
                        bool bSucceed = cmd.ExecuteNonQuery() != 0;

                        if (bSucceed)
                        {
                            cmd.CommandText = $"UPDATE reset_password SET used=TRUE WHERE email='{email}';";
                            cmd.ExecuteNonQuery();

                            await conn.SendByteAsync((byte)EResponse.Ok);
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.ServerError);
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
