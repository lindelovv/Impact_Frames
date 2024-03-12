using Unity.Burst;
using Unity.Entities;
using Unity.NetCode.Hybrid;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ThoughtBubbleTriggerSystem : SystemBase
{
    private GhostPresentationGameObjectSystem _ghostPresentationGameObjectSystem;
    
    [BurstCompile]
    protected override void OnCreate()
    {
        _ghostPresentationGameObjectSystem = World.GetExistingSystemManaged<GhostPresentationGameObjectSystem>();
        RequireForUpdate<GhostPresentationGameObjectPrefabReference>();
        RequireForUpdate<ThoughtBubble>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        foreach (
            var (triggerEventBuffer, entity) 
            in SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>>()
                .WithAll<GhostPresentationGameObjectPrefabReference>()
                .WithAll<ThoughtBubble>()
                .WithEntityAccess()
        ) {
            var reference = _ghostPresentationGameObjectSystem.GetGameObjectForEntity(EntityManager, entity);
            if(!reference) { Debug.Log("reference null"); continue; }
            
            var dialogPopUp = reference.GetComponent<DialogPopUp>();
            if(!dialogPopUp) { Debug.Log("dialog popup null"); continue; }
            
            foreach (var triggerEvent in triggerEventBuffer)
            {
                switch (triggerEvent.State)
                {
                    case StatefulEventState.Enter: { dialogPopUp.Activate();   break; }
                    case StatefulEventState.Exit:  { dialogPopUp.DeActivate(); break; }
                }
            }
        }
    }
}
    