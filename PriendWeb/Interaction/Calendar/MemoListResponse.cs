﻿using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Commit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Calendar
{
    public sealed class MemoListResponse : IResponse
    {
        private enum EResponse : byte
        {
            ServerError = 0,
            AccountError = 1,
            BeginList = 2,
            Ok = 3,
        }

        string IResponse.Path => "/ws/calendar/memos";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long accountId = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            long animalId = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            ulong when = conn.UInt64Message.Value;

            using (var cmd = npgConn.CreateCommand())
            {
                if (!AuthorizationChecker.ValidateToken(cmd, accountId, authToken))
                {
                    await conn.SendByteAsync((byte)EResponse.AccountError);
                    return;
                }

                if (!AuthorizationChecker.CheckAuthorizationOnAnimal(cmd, accountId, animalId))
                {
                    await conn.SendByteAsync((byte)EResponse.AccountError);
                    return;
                }

                // Send list of memo
                LinkedList<Memo> memoList = new LinkedList<Memo>();

                cmd.CommandText = $"SELECT id, title, content, images FROM memo WHERE animal_id={animalId} AND date_time={when};";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        string title = reader.GetString(1);
                        string content = reader.GetString(2);
                        string photoString;
                        if (reader.IsDBNull(3))
                        {
                            photoString = null;
                        }
                        else
                        {
                            photoString = reader.GetString(3);
                        }

                        memoList.AddLast(new Memo(id, when, title, content, photoString));
                    }
                }

                await conn.SendByteAsync((byte)EResponse.BeginList);
                foreach (var memo in memoList)
                {
                    await conn.SendTextAsync(memo.ToJson().ToString());
                }

                await conn.SendByteAsync((byte)EResponse.Ok);
            }
        }
    }
}
