using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Services.Relay.Models;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public struct InitAnimations : IComponentData {}
public struct SyncAnimations : IComponentData {}

//______________________________________________________________________________________________________________________
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
public partial struct AnimationInitSyncSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<InitAnimations>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);

        foreach (
            var (animatedGameObject, entity)
            in SystemAPI.Query<AnimatedGameObject>()
                .WithNone<AnimationReferenceData>()
                .WithEntityAccess()
        ) {
            var gameObject = Object.Instantiate(animatedGameObject.Prefab);
            var animationReference = new AnimationReferenceData {
                Animator = gameObject.GetComponent<Animator>(),
            };
            cmdBuffer.AddComponent(entity, animationReference);
            cmdBuffer.AddComponent<SyncAnimations>(entity);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

//______________________________________________________________________________________________________________________
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct AnimationSyncSystem : ISystem
{
    private readonly struct _parameters {
        public static readonly int Random     = Animator.StringToHash("Random");
        public static readonly int IsMoving   = Animator.StringToHash("IsMoving");
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        public static readonly int IsJumping  = Animator.StringToHash("IsJumping");
        public static readonly int IsFalling  = Animator.StringToHash("IsFalling");
        public static readonly int IsPunching = Animator.StringToHash("IsPunching");
        public static readonly int IsKicking = Animator.StringToHash("IsKicking");
        public static readonly int IsFallingFromHigh = Animator.StringToHash("IsFallingFromHigh");
        public static readonly int IsBlocking = Animator.StringToHash("IsBlocking");
        public static readonly int IsParrying = Animator.StringToHash("IsParrying");
        public static readonly int IsAnimLocked = Animator.StringToHash("IsAnimLocked");

        




    };

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SyncAnimations>();
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<AnimationReferenceData>();
        state.RequireForUpdate<PlayerStateComponent>();
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.GetSingleton<NetworkTime>().IsFinalPredictionTick)
        {
            var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            foreach (
                var (transform, reference, playerState)
                in SystemAPI.Query<LocalTransform, AnimationReferenceData, PlayerStateComponent>()
            ) {
                reference.Animator.SetInteger(_parameters.Random, Random.Range(0, 3));
                //if (playerState.isPunching)
                //{
                //    Debug.Log(reference.Animator.GetInteger(_parameters.Random));
                //}

                reference.Animator.SetBool(_parameters.IsMoving, playerState.isMoving);
                reference.Animator.SetBool(_parameters.IsGrounded, playerState.isGrounded);
                reference.Animator.SetBool(_parameters.IsFalling, playerState.isFalling);
                reference.Animator.SetBool(_parameters.IsJumping, playerState.isJumping);
                reference.Animator.SetBool(_parameters.IsPunching, playerState.isPunching);
                reference.Animator.SetBool(_parameters.IsKicking, playerState.isKicking);
                reference.Animator.SetBool(_parameters.IsFallingFromHigh, playerState.isFallingFromHigh);
                reference.Animator.SetBool(_parameters.IsBlocking, playerState.IsBlocking);
                reference.Animator.SetBool(_parameters.IsParrying, playerState.IsParrying);
                reference.Animator.SetBool(_parameters.IsAnimLocked, playerState.IsAnimLocked);
                


                var animatorTransform = reference.Animator.transform;
                animatorTransform.position = new float3(
                    transform.Position.x,
                    transform.Position.y - 1f,
                    transform.Position.z
                );
                animatorTransform.rotation = transform.Rotation;
            }

            cmdBuffer.Playback(state.EntityManager);
            cmdBuffer.Dispose();
        }
    }
}

// not currently working
//______________________________________________________________________________________________________________________
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
public partial struct AnimationTerminateSyncSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SyncAnimations>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);

        foreach (
            var (animatorReference, entity)
            in SystemAPI.Query<AnimationReferenceData>()
                .WithAll<NetworkStreamRequestDisconnect>()
                .WithAll<SyncAnimations>()
                .WithEntityAccess()
        ) {
            Debug.Log($"Disconnect");
            cmdBuffer.RemoveComponent<SyncAnimations>(entity);
            cmdBuffer.DestroyEntity(entity);
            Object.Destroy(animatorReference.Animator.gameObject);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
