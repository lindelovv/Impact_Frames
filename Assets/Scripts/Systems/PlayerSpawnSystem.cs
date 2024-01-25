
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct RequstSpawnPlayer : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var requestSpawnEntity = state.EntityManager.CreateEntity(typeof(SendRpcCommandRequest));
        state.EntityManager.AddComponent<SpawnPlayerRequest>(requestSpawnEntity);
        state.Enabled = false;
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(GhostSimulationSystemGroup))]
public partial struct PlayerSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AutoConnectComponent>();
        state.RequireForUpdate<SpawnerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var spawner = SystemAPI.GetSingleton<SpawnerComponent>();
        foreach (var request in SystemAPI.Query<SpawnPlayerRequestAspect>())
        {
            cmdBuffer.DestroyEntity(request.Self);

            var entityNetwork = request.SourceCOnnection;
            var networkId = state.EntityManager.GetComponentData<NetworkId>(entityNetwork);
            Debug.Log($"Spawning player for connection {networkId.Value}");

            var player = cmdBuffer.Instantiate(spawner.Player);
            cmdBuffer.AddComponent(player, new GhostOwner { NetworkId = networkId.Value });
            cmdBuffer.AddComponent(player, new PlayerName { Name = $"Player{networkId.Value}" });
            // TODO: Add player state
            cmdBuffer.AddComponent<PlayerSpawned>(entityNetwork);
            cmdBuffer.AddComponent(player, spawner.SpawnPoint);
            
            // Add the player to the linked entity group on the connection so it is destroyed
            // automatically on disconnect (destroyed with connection entity destruction)
            cmdBuffer.AppendToBuffer(entityNetwork, new LinkedEntityGroup { Value = player });
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
        
    }
}