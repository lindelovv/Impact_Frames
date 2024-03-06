using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
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
        RequireForUpdate<SpawnerComponent>();
    }

    protected override void OnUpdate()
    {
        var prefab = SystemAPI.GetSingleton<SpawnerComponent>().Player;
        var spawnPoint = SystemAPI.GetSingleton<SpawnerComponent>().SpawnPoint;
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities
            .WithStoreEntityQueryInField(ref _newPlayers)
            .WithNone<PlayerSpawned>()
            .ForEach((Entity connectionEntity, in NetworkStreamInGame req, in NetworkId networkId) =>
            {
                Debug.Log($"Spawning player for connection {networkId.Value}");
                var player = cmdBuffer.Instantiate(prefab);

                cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                cmdBuffer.AppendToBuffer(connectionEntity, new LinkedEntityGroup {Value = player});

                cmdBuffer.AddComponent(player, new ConnectionOwner { Entity = connectionEntity });
                cmdBuffer.SetComponent(player, new PlayerId { Value = (Int16)networkId.Value });
                
                cmdBuffer.SetComponent(player, spawnPoint);
                cmdBuffer.AddComponent(player, new SpawnPoint {
                    Position = spawnPoint.Position,
                });
                
                cmdBuffer.AddComponent<PlayerSpawned>(connectionEntity);
                cmdBuffer.AddComponent<InitAnimations>(connectionEntity);
            }
        ).Run();
        cmdBuffer.Playback(EntityManager);
    }
}
