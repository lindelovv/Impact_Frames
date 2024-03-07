using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(PhysicsSystemGroup))]
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
            state.Dependency = new TriggerEventJob {
                FallingObjectData = SystemAPI.GetComponentLookup<FallingObjectData>(),
                Entity = entity,
                Manager = state.EntityManager,
                CmdBuffer = cmdBuffer,
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            state.Dependency.Complete();
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }

    [BurstCompile]
    struct TriggerEventJob : ITriggerEventsJob
    {
        public ComponentLookup<FallingObjectData> FallingObjectData;
        public Entity Entity;
        public EntityManager Manager;
        public EntityCommandBuffer CmdBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            //Debug.Log($"{Manager.GetName(triggerEvent.EntityA)}");
            //Debug.Log($"{Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityA)}");
            //Debug.Log($"{Manager.GetName(triggerEvent.EntityB)}");
            //Debug.Log($"{Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityB)}");
            if (Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityA))
            {
                Debug.Log("A has player data");
                CmdBuffer.RemoveComponent<PhysicsCollider>(triggerEvent.EntityB);
            }
            if (Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityA))
            {
                Debug.Log("B has player data");
                CmdBuffer.RemoveComponent<PhysicsCollider>(triggerEvent.EntityA);
            }
        }
    }
}