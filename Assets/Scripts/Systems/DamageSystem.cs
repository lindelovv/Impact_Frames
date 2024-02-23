using System.Threading;
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
            var (health, playerState, entity)
            in SystemAPI.Query<RefRW<HealthComponent>, RefRW<PlayerStateComponent>>()
                .WithEntityAccess()
                .WithAll<TakeDamage>()
        ) {
            health.ValueRW.Current--;
            Debug.Log($"{state.EntityManager.GetName(entity)} Health: {health.ValueRO.Current}");
            cmdBuffer.RemoveComponent<TakeDamage>(entity);
            playerState.ValueRW.IsHit = true;
            
            if (state.EntityManager.HasComponent<NetworkId>(entity))
            {
                var connectionId = state.EntityManager.GetComponentData<NetworkId>(entity).Value;
                UIManager.instance.UpdateHealth(health.ValueRW.Current, connectionId);

                if (health.ValueRO.Current <= 0)
                {
                    UIManager.instance.DecreaseLife(connectionId);
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

public struct TakeDamage : IComponentData {}
