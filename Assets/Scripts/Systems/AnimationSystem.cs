using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Services.Relay.Models;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
public partial struct AnimationInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<AnimatedGameObject>();
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
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true),
 WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct AnimationUpdateSystem : ISystem
{
    private readonly struct _parameters {
        public static readonly int Random = Animator.StringToHash("Random");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        public static readonly int IsJumping = Animator.StringToHash("IsJumping");
        public static readonly int IsFalling = Animator.StringToHash("IsFalling");
        public static readonly int IsPunching = Animator.StringToHash("IsPunching");
    };

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<AnimationReferenceData>();
        state.RequireForUpdate<PlayerStateComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
        foreach (
            var (transform, animatorReference, playerState)
            in SystemAPI.Query<LocalTransform, AnimationReferenceData, PlayerStateComponent>()
        ) {
            animatorReference.Animator.SetInteger(_parameters.Random, Random.Range(0, 3));
            //if (playerState.isPunching)
            //{
            //    Debug.Log(animatorReference.Animator.GetInteger(_parameters.Random));
            //}

            animatorReference.Animator.SetBool(_parameters.IsMoving, playerState.isMoving);
            animatorReference.Animator.SetBool(_parameters.IsGrounded, playerState.isGrounded);
            animatorReference.Animator.SetBool(_parameters.IsFalling, playerState.isFalling);
            animatorReference.Animator.SetBool(_parameters.IsJumping, playerState.isJumping);
            animatorReference.Animator.SetBool(_parameters.IsPunching, playerState.isPunching);
            
            var animatorTransform = animatorReference.Animator.transform;
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
