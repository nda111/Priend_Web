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

                    List<int> groupIdList = new List<int>();
                    int prevGroupId = -1;
                    SortedList<int, LinkedList<long>> idEntries = new SortedList<int, LinkedList<long>>();
                    LinkedList<long> currentPetIdList = null;
                    cmd.CommandText =
                        $"SELECT managed.group_id, managed.pet_id FROM " +
                        $"participates JOIN managed ON participates.group_id=managed.group_id " +
                        $"WHERE account_id={id} " +
                        $"ORDER BY managed.group_id;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int groupId = reader.GetInt32(0);
                            long petId = reader.GetInt64(1);

                            if (prevGroupId != groupId)
                            {
                                groupIdList.Add(groupId);
                                currentPetIdList = new LinkedList<long>();
                                idEntries.Add(groupId, currentPetIdList);

                                prevGroupId = groupId;
                            }

                            currentPetIdList.AddLast(petId);
                        }
                    }

                    var groups = new List<Group>();
                    var entries = new Dictionary<Group, LinkedList<Animal>>();
                    foreach (int groupId in groupIdList)
                    {
                        var animalList = new LinkedList<Animal>();

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
                                entries.Add(group, animalList);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        foreach (long petId in idEntries[groupId])
                        {
                            Animal animal;
                            SortedDictionary<long, double> weights;

                            cmd.CommandText = $"SELECT species, name, birth, sex FROM animal WHERE id={petId};";
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    long species = reader.GetInt64(0);
                                    string name = reader.GetString(1);
                                    long birthday = reader.GetInt64(2);
                                    short sex = reader.GetInt16(3);

                                    weights = new SortedDictionary<long, double>();
                                    animal = new Animal(petId, species, birthday, name, (Sex)sex, weights);
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            cmd.CommandText = $"SELECT measured, weights FROM weights WHERE pet_id={petId};";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    long when = reader.GetInt64(0);
                                    double weight = reader.GetDouble(1);

                                    weights.Add(when, weight);
                                }
                            }

                            animalList.AddLast(animal);
                        }

                        entries.Add(group, animalList);
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
            catch (NpgsqlException)
            {
                await conn.SendByteAsync((byte)EResponse.ServerError);
            }
        }
    }
}
