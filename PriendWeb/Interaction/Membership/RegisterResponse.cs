using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership
{
    public sealed class RegisterResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            ServerError = 1,
        }

        string IResponse.Path => "/ws/membership/register";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string email = conn.TextMessage;

            await conn.ReceiveAsync();
            string hashedPassword = Account.Hash.Password(conn.TextMessage);

            await conn.ReceiveAsync();
            string name = conn.TextMessage;

            string hash = Account.Hash.VerificationKey(email);

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    cmd.CommandText = 
                        $"INSERT INTO account (email, pw, name) VALUES ('{email}', '{hashedPassword}', '{name}');" +
                        $"INSERT INTO verifying_hash VALUES ('{email}', '{hash}');";
                    cmd.ExecuteNonQuery();

                    await MailSender.SendVerificationMailAsync(Startup.MailApiKey, email, name, hash);
                }

                await conn.SendByteAsync((byte)EResponse.Ok);
            }
            catch (NpgsqlException e)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);

                throw e;
            }
        }
    }
}
