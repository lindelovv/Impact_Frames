﻿using Unity.Burst;
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
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);

        foreach (
            var (playerState, entity)
            in SystemAPI.Query<RefRW<PlayerStateComponent>>()
                .WithAll<GhostPresentationGameObjectPrefabReference, PlayerData>()
                .WithEntityAccess()
        ) {
            var reference = _ghostPresentationGameObjectSystem.GetGameObjectForEntity(EntityManager, entity);
            if (!reference) { /*Debug.Log("GameObject reference null");*/ continue; }

            var get = reference.GetComponent<Getters>();
            if (!get) { Debug.Log("VFX getter null"); continue; }

            get._hasHit = playerState.ValueRO.HasHit;

            if (playerState.ValueRO.IsBlocking && !get._blockActive)
            {
                get.BlockAudioSource.Play();
                get.Block.SetBool("KillParticle", true);
                get.Block.Play();
                get._blockActive = true;
            }
            else if (get._blockActive)
            {
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

        cmdBuffer.Playback(EntityManager);
        cmdBuffer.Dispose();
    }
}
