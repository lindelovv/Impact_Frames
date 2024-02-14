using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
public partial struct AnimationSystem : ISystem
{
    private readonly struct _parameters {
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
    };
    
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

        foreach (
            var (transform, animatorReference, playerState)
            in SystemAPI.Query<LocalTransform, AnimationReferenceData, PlayerStateComponent>()
        ) {
            animatorReference.Animator.SetBool(_parameters.IsMoving, playerState.isMoving);
            
            animatorReference.Animator.transform.position = new float3(
                transform.Position.x,
                transform.Position.y - 1f,
                transform.Position.z
            );
            animatorReference.Animator.transform.rotation = transform.Rotation;
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
