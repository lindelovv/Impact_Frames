using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public struct PlayerSpawned : IComponentData { }

public struct ConnectionOwner : IComponentData
{
    public Entity Entity;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class SpawnPlayerSystem : SystemBase
{
    private EntityQuery _newPlayers;

    protected override void OnCreate()
    {
        RequireForUpdate(_newPlayers);
        RequireForUpdate<Spawner>();
    }

    protected override void OnUpdate()
    {
        var prefab = SystemAPI.GetSingleton<Spawner>().Player;
        var spawnPoint = SystemAPI.GetSingleton<Spawner>().SpawnPoint;
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities
            .WithStoreEntityQueryInField(ref _newPlayers)
            .WithNone<PlayerSpawned>()
            .ForEach((Entity connectionEntity, in NetworkStreamInGame req, in NetworkId networkId) =>
            {
                //Debug.Log($"Spawning player for connection {networkId.Value}");
                var player = cmdBuffer.Instantiate(prefab);

                cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                cmdBuffer.AppendToBuffer(connectionEntity, new LinkedEntityGroup { Value = player });

                cmdBuffer.AddComponent(player, new ConnectionOwner { Entity = connectionEntity });
                cmdBuffer.SetComponent(player, new PlayerId { Value = (Int16)networkId.Value });
                
                cmdBuffer.SetComponent(player, spawnPoint);
                cmdBuffer.AddComponent(player, new SpawnPoint {
                    Position = new float3(spawnPoint.Position.x + (Int16)networkId.Value, spawnPoint.Position.y, spawnPoint.Position.z),
                    Rotation = spawnPoint.Rotation,
                });
                cmdBuffer.AddComponent<Respawn>(player);
                
                cmdBuffer.AddComponent<PlayerSpawned>(connectionEntity);
            }
        ).Run();
        cmdBuffer.Playback(EntityManager);
    }
}
