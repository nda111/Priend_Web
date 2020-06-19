using Microsoft.AspNetCore.Http;
using Npgsql;
using PriendWeb.Authorization;
using PriendWeb.Data;
using PriendWeb.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriendWeb.Interaction.Home
{
    public sealed class EntityListResponse : IResponse
    {
        private enum EResponse : byte
        {
            Ok = 0,
            BeginGroup = 1,
            EndOfGroup = 2,
            EndOfList = 3,
            AccountError = 4,
            ServerError = 5,
        }

        string IResponse.Path => "/ws/home/entity/list";

        async Task IResponse.Response(HttpContext context, WebSocketConnection conn, NpgsqlConnection npgConn)
        {
            await conn.ReceiveAsync();
            long id = conn.Int64Message.Value;

            await conn.ReceiveAsync();
            string authToken = conn.TextMessage;

            try
            {
                using (var cmd = npgConn.CreateCommand())
                {
                    if (!AuthorizationChecker.ValidateToken(cmd, id, authToken))
                    {
                        await conn.SendByteAsync((byte)EResponse.AccountError);
                        return;
                    }

                    LinkedList<int> groupIdList = new LinkedList<int>();
                    cmd.CommandText = $"SELECT group_id FROM participates WHERE account_id={id};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int groupId = reader.GetInt32(0);
                            groupIdList.AddLast(groupId);
                        }
                    }

                    var groups = new List<Group>();
                    var idEntries = new Dictionary<Group, LinkedList<long>>();
                    foreach (var groupId in groupIdList)
                    {
                        Group group;
                        cmd.CommandText = $"SELECT owner_id, name FROM animal_group WHERE id={groupId};";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                long owner = reader.GetInt64(0);
                                string name = reader.GetString(1);

                                group = new Group(groupId, owner, name);
                                groups.Add(group);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        var animalIdList = new LinkedList<long>();
                        cmd.CommandText = $"SELECT pet_id FROM managed WHERE group_id={groupId};";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long animalId = reader.GetInt64(0);
                                animalIdList.AddLast(animalId);
                            }
                        }

                        idEntries.Add(group, animalIdList);
                    }

                    var entries = new Dictionary<Group, LinkedList<Animal>>();
                    foreach (var group in groups)
                    {
                        var animals = new LinkedList<Animal>();
                        var idEntry = idEntries[group];

                        foreach (var animalId in idEntry)
                        {
                            var weights = new SortedDictionary<long, double>();
                            cmd.CommandText = $"SELECT measured, weights FROM weights WHERE pet_id={animalId};";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    long when = reader.GetInt64(0);
                                    double weight = reader.GetDouble(1);

                                    weights.Add(when, weight);
                                }
                            }
                            
                            cmd.CommandText = $"SELECT species, name, birth, sex FROM animal WHERE id={animalId};";
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    long species = reader.GetInt64(0);
                                    string name = reader.GetString(1);
                                    long birthday = reader.GetInt64(2);
                                    short sex = reader.GetInt16(3);

                                    var animal = new Animal(animalId, species, birthday, name, (Sex)sex, weights);
                                    animals.AddLast(animal);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        entries.Add(group, animals);
                    }

                    await conn.SendByteAsync((byte)EResponse.Ok);
                    foreach (Group group in groups)
                    {
                        await conn.SendByteAsync((byte)EResponse.BeginGroup);
                        await conn.SendTextAsync(group.ToJson().ToString());

                        foreach (Animal animal in entries[group])
                        {
                            await conn.SendTextAsync(animal.ToJson().ToString());
                        }

                        await conn.SendByteAsync((byte)EResponse.EndOfGroup);
                    }
                    await conn.SendByteAsync((byte)EResponse.EndOfList);
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
