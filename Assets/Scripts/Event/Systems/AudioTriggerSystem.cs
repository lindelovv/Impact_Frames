using Unity.Burst;
using Unity.Entities;
using Unity.NetCode.Hybrid;
using UnityEngine;
using static Unity.VisualScripting.Member;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class AudioTriggerSystem : SystemBase
{
    private GhostPresentationGameObjectSystem _ghostPresentationGameObjectSystem;
    
    [BurstCompile]
    protected override void OnCreate()
    {
        _ghostPresentationGameObjectSystem = World.GetExistingSystemManaged<GhostPresentationGameObjectSystem>();
        RequireForUpdate<GhostPresentationGameObjectPrefabReference>();
        RequireForUpdate<AudioTrigger>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        foreach (
            var (triggerEventBuffer, entity) 
            in SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>>()
                .WithAll<GhostPresentationGameObjectPrefabReference>()
                .WithAll<AudioTrigger>()
                .WithEntityAccess()
        ) {
            var reference = _ghostPresentationGameObjectSystem.GetGameObjectForEntity(EntityManager, entity);
            if(!reference) { Debug.Log("reference null"); continue; }
            
            var audioTriggerZoneScript = reference.GetComponent<AudioTriggerZone_Source>();
            if(!audioTriggerZoneScript) { Debug.Log("dialog popup null"); continue; }
            
            foreach (var triggerEvent in triggerEventBuffer)
            {
                switch (triggerEvent.State)
                {
                    case StatefulEventState.Enter: {

                            audioTriggerZoneScript.source.PlayOneShot(audioTriggerZoneScript.clip);

                            if (audioTriggerZoneScript.PlayOnce)
                            {
                                audioTriggerZoneScript.StartCoroutine(audioTriggerZoneScript.DestroyAfterClip());
                            }
                            break; }

                    // Here goes everything that happens on Exit.
                    //case StatefulEventState.Exit:  {  break; }
                }
            }
        }
    }
}
    