using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct OneOffTriggerEventSystem : ISystem
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
            var (_, entity)
            in SystemAPI.Query<SingleTimeTriggerTag>()
                .WithEntityAccess()
        ) {
            state.Dependency = new OneOffTriggerEventJob {
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
    struct OneOffTriggerEventJob : ITriggerEventsJob
    {
        public Entity Entity;
        public EntityManager Manager;
        public EntityCommandBuffer CmdBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
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
            return;
            
            //if (triggerEvent.EntityA == Player || triggerEvent.EntityB == Player) { Debug.Log("One is player :-)"); return; }
            //
            //Debug.Log("Remove collider");
            //var (player, other) = (triggerEvent.EntityA == Player) 
            //    ? (triggerEvent.EntityA, triggerEvent.EntityB) 
            //    : (triggerEvent.EntityB, triggerEvent.EntityA);
            //CmdBuffer.RemoveComponent<PhysicsCollider>(other);
        }
    }
}
