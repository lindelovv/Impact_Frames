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
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities
            .WithStoreEntityQueryInField(ref _newPlayers)
            .WithNone<PlayerSpawned>()
            .ForEach((Entity connectionEntity, in NetworkStreamInGame req, in NetworkId networkId) =>
            {
                Debug.Log($"Spawning player for connection {networkId.Value}");
                var player = commandBuffer.Instantiate(prefab);

                commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                commandBuffer.AppendToBuffer(connectionEntity, new LinkedEntityGroup {Value = player});

                commandBuffer.AddComponent(player, new ConnectionOwner { Entity = connectionEntity });
                commandBuffer.SetComponent(player, spawnPoint);
                commandBuffer.SetComponent(player, new PlayerId { Value = (Int16)networkId.Value });
                
                commandBuffer.AddComponent<PlayerSpawned>(connectionEntity);
                commandBuffer.AddComponent<InitAnimations>(connectionEntity);
            }
        ).Run();
        commandBuffer.Playback(EntityManager);
    }
}
