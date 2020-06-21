using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data.Commit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Calendar
{
    public sealed class CommitWeightResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            ServerError = 1,
        }

        string IResponse.Path => "/ws/calendar/weight/commit";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            await conn.ReceiveAsync();
            string commitJson = conn.TextMessage;
            var commit = new WeightCommit();

            if (!commit.ReadJson(JObject.Parse(commitJson)))
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
                return;
            }

            using (var cmd = npgConn.CreateCommand())
            {
                if (!AuthorizationChecker.ValidateToken(cmd, id, authToken))
                {
                    await conn.SendByteAsync((byte)EResponse.ServerError);
                    return;
                }

                if (!AuthorizationChecker.CheckAuthorizationOnAnimal(cmd, id, commit.AnimalId))
                {
                    await conn.SendByteAsync((byte)EResponse.ServerError);
                    return;
                }

                // Actual update work
                int successCount = 0;
                foreach (var pair in commit.Changes)
                {
                    var change = pair.Value;

                    switch (change.ChangeType)
                    {
                        case WeightCommit.Change.Type.Upsert:
                            cmd.CommandText =
                                $"INSERT INTO weights (pet_id, measured, weights) " +           // insert
                                $"VALUES({commit.AnimalId}, {change.Date}, {change.Weight}) " + // values
                                $"ON CONFLICT (pet_id, measured) DO " +                         // on conflict
                                $"UPDATE SET weights={change.Weight};";                         // update
                            break;

                        case WeightCommit.Change.Type.Delete:
                            cmd.CommandText =
                                $"DELETE FROM weights " +
                                $"WHERE pet_id={commit.AnimalId} " +
                                $"AND measured={change.Date};";
                            break;

                        default:
                            cmd.CommandText = "";
                            break;
                    }

                    if (cmd.CommandText.Length != 0)
                    {
                        int trial = 0;
                        int affectionCount;

                        do
                        {
                            if (trial++ == 3) // Max of 3 trials
                            {
                                // Do not increase successCount
                                goto UPDATE_FAILURE;
                            }

                            affectionCount = cmd.ExecuteNonQuery();
                        } while (affectionCount == 0);

                        successCount++;
                    }

                UPDATE_FAILURE: { }
                }

                await conn.SendByteAsync((byte)EResponse.Ok);
                await conn.SendInt32Async(successCount);
            }
        }
    }
}
