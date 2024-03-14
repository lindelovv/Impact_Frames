using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Hybrid;
using UnityEngine;
using UnityEngine.VFX;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class VFXSyncSystem : SystemBase
{
    private GhostPresentationGameObjectSystem _ghostPresentationGameObjectSystem;

    [BurstCompile]
    protected override void OnCreate()
    {
        _ghostPresentationGameObjectSystem = World.GetExistingSystemManaged<GhostPresentationGameObjectSystem>();
        RequireForUpdate<GhostPresentationGameObjectPrefabReference>();
        RequireForUpdate<PredictedGhost>();
        RequireForUpdate<PlayerData>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
        
        foreach (
            var (playerState, entity)
            in SystemAPI.Query<RefRW<PlayerStateComponent>>()
                .WithAll<GhostPresentationGameObjectPrefabReference, PredictedGhost, PlayerData>()
                .WithEntityAccess()
        ) {
            var reference = _ghostPresentationGameObjectSystem.GetGameObjectForEntity(EntityManager, entity);
            if (!reference) { Debug.Log("GameObject reference null"); continue; }
            
            var get = reference.GetComponent<Getters>();
            if (!get) { Debug.Log("VFX getter null"); continue; }

            if (playerState.ValueRO.IsBlocking)
            {
                get.Block.enabled = true;
            }
            else
            {
                get.Block.enabled = false;
            }

            if (playerState.ValueRO.IsDashing && !get.AudioSource.isPlaying)
            {
                get.AudioSource.PlayOneShot(get.DashAudioClip);
            }
            
            if (playerState.ValueRO.IsFallingHigh && playerState.ValueRO.IsGrounded)
            {
                // Add some cool ass camera shake
                get.LandingImpact.Play();
                get.AudioSource.PlayOneShot(get.LandingHard);
                playerState.ValueRW.IsFallingHigh = false;
            }
        }
        cmdBuffer.Playback(EntityManager);
        cmdBuffer.Dispose();
    }
}
