using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership
{
    public sealed class ResetPasswordResponse : IResponse
    {
        private enum EResponse
        {
            Ok = 0,
            ServerError = 1,
        }

        string IResponse.Path => "/ws/membership/reset";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string email = conn.TextMessage;

            DateTime due = DateTime.Today.AddDays(7);
            string hash = Account.Hash.ResetPasswordKey(email, due);

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    string name = null;

                    cmd.CommandText = $"SELECT (name) FROM account WHERE email='{email}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            name = reader.GetString(0);
                        }
                    }

                    if (name == null)
                    {
                        await conn.SendByteAsync((byte)EResponse.ServerError);
                    }
                    else
                    {
                        // Insert if not exists, update otherwise
                        cmd.CommandText =
                            $"INSERT INTO reset_password VALUES ('{email}', {due.Ticks}, '{hash}') " +
                            $"ON CONFLICT (email) DO " +
                            $"UPDATE SET hash='{hash}', expire_due={due.Ticks};";
                        cmd.ExecuteNonQuery();

                        await MailSender.SendResetPasswordMailAsync(Startup.MailApiKey, email, name, hash);

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
