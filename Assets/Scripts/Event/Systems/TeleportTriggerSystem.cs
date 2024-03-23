using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct TeleportTriggerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Teleporter>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (teleporter, triggerEventBuffer, entity) 
            in SystemAPI.Query<Teleporter, DynamicBuffer<StatefulTriggerEvent>>()
                .WithEntityAccess()
        ) {
            foreach (var triggerEvent in triggerEventBuffer)
            {
                var other = triggerEvent.GetOtherEntity(entity);
                switch (triggerEvent.State)
                {
                    case StatefulEventState.Enter:
                    {
                        cmdBuffer.AddComponent(other, new SpawnPoint {
                            Position = teleporter.NextPosition,
                            Rotation = quaternion.identity,
                        });
                        cmdBuffer.AddComponent(other, new PhysicsVelocity {
                            Linear = 0,
                            Angular = 0,
                        });
                        cmdBuffer.AddComponent<Respawn>(other);
                        break;
                    }
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
