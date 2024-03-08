using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class TriggerToggleSystem : SystemBase
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
        foreach (
            var (thoughtBubble, entity)
            in SystemAPI.Query<PlayerData>()
                .WithEntityAccess()
        ) {
            Dependency = new TriggerToggleJob {
                FallingObjectData = SystemAPI.GetComponentLookup<FallingObjectData>(),
                ThoughtBubble = entity,
                Manager = EntityManager,
                CmdBuffer = cmdBuffer,
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);
            Dependency.Complete();
        }
        cmdBuffer.Playback(EntityManager);
        cmdBuffer.Dispose();
    }

    [BurstCompile]
    struct TriggerToggleJob : ITriggerEventsJob
    {
        public ComponentLookup<FallingObjectData> FallingObjectData;
        public Entity ThoughtBubble;
        public EntityManager Manager;
        public EntityCommandBuffer CmdBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            //Debug.Log($"{Manager.GetName(triggerEvent.EntityA)}");
            //Debug.Log($"{Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityA)}");
            //Debug.Log($"{Manager.GetName(triggerEvent.EntityB)}");
            //Debug.Log($"{Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityB)}");
            //if (Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityA))
            //{
            //    Debug.Log("A has player data");
            //}
            //if (Manager.HasComponent<SingleTimeTriggerTag>(triggerEvent.EntityA))
            //{
            //    Debug.Log("B has player data");
            //}
        }
    }
}
