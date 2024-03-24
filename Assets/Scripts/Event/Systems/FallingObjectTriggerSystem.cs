using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct FallingObjectTriggerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FallingObject>();
        state.RequireForUpdate<UnscaledClientTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (
            var (triggerEventBuffer, gravityFactor, fallingObject, entity) 
            in SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>, RefRW<PhysicsGravityFactor>, FallingObject>()
                .WithNone<Respawn>()
                .WithAll<FallingObject>()
                .WithEntityAccess()
        ) {
            if(fallingObject.Active) { continue; }
            foreach (var triggerEvent in triggerEventBuffer)
            {
                switch (triggerEvent.State)
                {
                    case StatefulEventState.Enter:
                    {
                        gravityFactor.ValueRW.Value = 0.4f;
                        break;
                    }
                }
            }
        }

        foreach (
            var (collisionEventBuffer, fallingObject, entity) 
            in SystemAPI.Query<DynamicBuffer<StatefulCollisionEvent>, RefRW<FallingObject>>()
                .WithNone<Respawn>()
                .WithEntityAccess()
        ) {
            if(fallingObject.ValueRO.Active) { continue; }
            foreach (var collisionEvent in collisionEventBuffer)
            {
                switch (collisionEvent.State)
                { 
                    case StatefulEventState.Enter:
                    {
                        var other = collisionEvent.GetOtherEntity(entity);
                        if (state.EntityManager.HasComponent<Health>(other))
                        {
                            cmdBuffer.AddComponent(other, new TakeDamage {
                                Amount = 1,
                            });
                        }
                        fallingObject.ValueRW.Active = true;
                        fallingObject.ValueRW.ResetTimer = fallingObject.ValueRO.TimeToRespawn;
                        break;
                    }
                }
            }
        }
        
        foreach (
            var (fallingObject, gravityFactor, velocity, entity)
            in SystemAPI.Query<RefRW<FallingObject>, RefRW<PhysicsGravityFactor>, RefRW<PhysicsVelocity>>()
                .WithEntityAccess()
        ) {
            if (!fallingObject.ValueRO.Active) { continue; }
            if (fallingObject.ValueRO.ResetTimer > 0)
            {
                fallingObject.ValueRW.ResetTimer -= SystemAPI.GetSingleton<UnscaledClientTime>().UnscaleDeltaTime;
            }
            else
            {
                fallingObject.ValueRW.ResetTimer = -1;
                fallingObject.ValueRW.Active = false;
                gravityFactor.ValueRW.Value = 0;
                velocity.ValueRW.Linear = float3.zero;
                cmdBuffer.AddComponent<Respawn>(entity);
            }
        }
        
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
    