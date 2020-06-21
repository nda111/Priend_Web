using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Npgsql;
using PriendWeb.Data;
using PriendWeb.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Membership
{
    public sealed class LoginResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            WrongPassword = 1,
            UnknownEmail = 2,
            ServerError = 3,
        }

        string IResponse.Path => "/ws/membership/login";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            string email = conn.TextMessage;

            await conn.ReceiveAsync();
            string password = conn.TextMessage;
            string hashedPassword = Account.Hash.Password(password);

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT pw, id, name, weights_alert, birth_alert, event_alert, com_comment FROM account WHERE email='{email}';";

                    Account account = null;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            string readPassword = reader.GetString(0);

                            if (hashedPassword == readPassword)
                            {
                                string authToken = Account.Hash.AuthenticationToken(email, password);

                                account = new Account(
                                    id: reader.GetInt64(1),
                                    name: reader.GetString(2),
                                    email: email,
                                    authToken: authToken,
                                    settings: new Settings(
                                        weightAlert: reader.GetBoolean(3),
                                        birthdayAlert: reader.GetBoolean(4),
                                        eventAlert: reader.GetBoolean(5),
                                        commentAlert: reader.GetBoolean(6)));
                            }
                            else
                            {
                                await conn.SendByteAsync((byte)EResponse.WrongPassword);
                            }
                        }
                        else
                        {
                            await conn.SendByteAsync((byte)EResponse.UnknownEmail);
                        }
                    }

                    if (account != null) // on login success
                    {
                        cmd.CommandText = $"UPDATE account SET auth_token='{account.AuthenticationToken}' WHERE email='{email}';";
                        cmd.ExecuteNonQuery();

                        string jsonString = account.ToJson().ToString();

                        await conn.SendByteAsync((byte)EResponse.Ok);
                        await conn.SendTextAsync(jsonString);
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
