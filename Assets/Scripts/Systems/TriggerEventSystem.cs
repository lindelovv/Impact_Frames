using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
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
        var query = SystemAPI.QueryBuilder()
            .WithAllRW<PhysicsVelocity>()
            .WithAll<LocalTransform, PhysicsMass>()
            .WithNone<StatefulTriggerEvent>()
            .Build();
        
        new TriggerEventJob {
            NonTriggerDynamicBodyMask = query.GetEntityQueryMask(),
            StepComponent = SystemAPI.HasSingleton<PhysicsStep>()
                ? SystemAPI.GetSingleton<PhysicsStep>()
                : PhysicsStep.Default,
            DeltaTime = SystemAPI.Time.DeltaTime,
            LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(),
            Velocities = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
            Masses = SystemAPI.GetComponentLookup<PhysicsMass>(),
        }.Schedule();
    }

    [BurstCompile]
    partial struct TriggerEventJob : IJobEntity
    {
        public EntityQueryMask NonTriggerDynamicBodyMask;
        public PhysicsStep StepComponent;
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransforms;
        [ReadOnly] public ComponentLookup<PhysicsMass> Masses;
        public ComponentLookup<PhysicsVelocity> Velocities;

        public void Execute(Entity entity, ref DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer)
        {
            foreach (var triggerEvent in triggerEventBuffer)
            {
                var other = triggerEvent.GetOtherEntity(entity);
                var velocity = Velocities[other];
                velocity.Linear += new float3(1, 1, 0);
            }
        }
    }
}
