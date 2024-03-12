using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct TriggerTestSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new TriggerCheckJob {
            NonTriggerDynamicBodyMask = SystemAPI.QueryBuilder()
                .WithAllRW<PhysicsVelocity>()
                .WithAll<LocalTransform, PhysicsMass>()
                .WithNone<StatefulTriggerEvent>()
                .Build()
                .GetEntityQueryMask(),
        }.Schedule();
    }

    [BurstCompile]
    partial struct TriggerCheckJob : IJobEntity
    {
        public EntityQueryMask NonTriggerDynamicBodyMask;

        public void Execute(Entity e, ref DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer, FallingObjectData fallingObject)
        {
            foreach (var triggerEvent in triggerEventBuffer)
            {
                var otherEntity = triggerEvent.GetOtherEntity(e);
                
                if (triggerEvent.State != StatefulEventState.Stay) { Debug.Log($"Trigger State: {triggerEvent.State}"); }
                
                // exclude static bodies, other triggers and enter/exit events
                if (triggerEvent.State != StatefulEventState.Stay || !NonTriggerDynamicBodyMask.MatchesIgnoreFilter(otherEntity)){ continue; }
            }
        }
    }
}
    