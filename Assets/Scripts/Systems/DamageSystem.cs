using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct DamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HealthComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (health, entity)
            in SystemAPI.Query<RefRW<HealthComponent>>()
                .WithEntityAccess()
                .WithAll<TakeDamage>()
        ) {
            health.ValueRW.CurrentHealth--;
            
            if (health.ValueRO.CurrentHealth <= 0) { cmdBuffer.DestroyEntity(entity); }

            cmdBuffer.RemoveComponent<TakeDamage>(entity);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

public struct TakeDamage : IComponentData {}
