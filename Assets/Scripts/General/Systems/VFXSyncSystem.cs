using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode.Hybrid;
using UnityEngine;

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
        RequireForUpdate<PlayerData>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        foreach (
            var (playerState, player, entity)
            in SystemAPI.Query<RefRW<PlayerStateComponent>, PlayerAspect>()
                .WithAll<GhostPresentationGameObjectPrefabReference, PlayerData>()
                .WithEntityAccess()
        ) {
            var reference = _ghostPresentationGameObjectSystem.GetGameObjectForEntity(EntityManager, entity);
            if (!reference) { Debug.Log("[VFXSync] GameObject reference null"); continue; }

            var get = reference.GetComponent<Getters>();
            if (!get) { Debug.Log("[VFXSync] VFX getter null"); continue; }

            get._hasHit = playerState.ValueRO.HasHit;

            if (playerState.ValueRO.IsBlocking && !get._blockActive)
            {
                Debug.Log("Activate block");
                get.BlockAudioSource.Play();
                get.Block.SetBool("KillParticle", true);
                get.Block.Play();
                get._blockActive = true;
            }
            else if (!playerState.ValueRO.IsBlocking && get._blockActive)
            {
                Debug.Log("Block deactivate");
                get.BlockAudioSource.Stop();
                get.Block.SetBool("KillParticle", false);
                get.Block.Stop();
                get._blockActive = false;
            }

            if (playerState.ValueRO.IsHit)
            {
                get.InkSplatter.Play();
            }

            // Reactional sets cooldown to reset bool value (to play once and not have to use isPlaying)
            if (playerState.ValueRO.IsDashing && !get.isTimerRunning)
            {
                get.DashTrail.Play();
                get.DashSmoke.Play();
                get.ToggleEthereal(true);
                get.Timer();
                get.AudioSource_MainOneShots.PlayOneShot(get.DashAudioClip);
            }
            else if (get.timerCompleted)
            {
                get.ToggleEthereal(false);
                get.DashSmoke.Stop();
                get.DashTrail.Stop();
                get.timerCompleted = false;
            }

            if (playerState.ValueRO.IsFallingHigh && playerState.ValueRO.IsGrounded)
            {
                // Add some cool ass camera shake
                get.LandingImpact.Play();
                get.AudioSource_MainOneShots.PlayOneShot(get.LandingHard);
                playerState.ValueRW.IsFallingHigh = false;
            }
        }
    }
}
