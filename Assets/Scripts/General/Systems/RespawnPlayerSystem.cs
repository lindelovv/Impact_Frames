using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[GhostComponent(
    PrefabType=GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct SpawnPoint : IComponentData
{
    public float3 Position;
}

public struct Respawn : IComponentData {}

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct RespawnPlayerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<SpawnPoint>();
        state.RequireForUpdate<Respawn>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (transform, spawnPoint, entity) 
            in SystemAPI.Query<RefRW<LocalTransform>, SpawnPoint>()
                .WithAll<Respawn>()
                .WithEntityAccess()
        ) {
            Debug.Log("Respawn");
            transform.ValueRW.Position = spawnPoint.Position;
            cmdBuffer.RemoveComponent<Respawn>(entity);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}