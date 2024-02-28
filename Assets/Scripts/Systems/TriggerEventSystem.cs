using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct TriggerEventSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
        foreach (
            var (fallingObjectData, entity)
            in SystemAPI.Query<FallingObjectData>()
                .WithEntityAccess()
        ) {
            state.Dependency = new CollisionEventJob {
                FallingObjectData = SystemAPI.GetComponentLookup<FallingObjectData>(),
                Entity = entity,
                CmdBuffer = cmdBuffer,
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            state.Dependency.Complete();
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }

    [BurstCompile]
    struct CollisionEventJob : ITriggerEventsJob
    {
        public ComponentLookup<FallingObjectData> FallingObjectData;
        public Entity Entity;
        public EntityCommandBuffer CmdBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Debug.Log("test");
            CmdBuffer.AddComponent<PhysicsVelocity>(Entity);
            CmdBuffer.AddComponent<PhysicsMass>(Entity);
        }
    }
}