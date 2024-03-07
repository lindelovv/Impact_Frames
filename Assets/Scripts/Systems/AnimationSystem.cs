using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Hybrid;
using UnityEngine;
using Random = UnityEngine.Random;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class AnimationSyncSystem : SystemBase
{
    private GhostPresentationGameObjectSystem _ghostPresentationGameObjectSystem;
    private readonly struct Parameters {
        public static readonly int Random            = Animator.StringToHash("Random");
        public static readonly int IsMoving          = Animator.StringToHash("IsMoving");
        public static readonly int IsGrounded        = Animator.StringToHash("IsGrounded");
        public static readonly int IsJumping         = Animator.StringToHash("IsJumping");
        public static readonly int IsFalling         = Animator.StringToHash("IsFalling");
        public static readonly int IsPunching        = Animator.StringToHash("IsPunching");
        public static readonly int IsKicking         = Animator.StringToHash("IsKicking");
        public static readonly int IsFallingFromHigh = Animator.StringToHash("IsFallingFromHigh");
        public static readonly int IsBlocking        = Animator.StringToHash("IsBlocking");
        public static readonly int IsParrying        = Animator.StringToHash("IsParrying");
        public static readonly int IsAnimLocked      = Animator.StringToHash("IsAnimLocked");
        public static readonly int HitCounter        = Animator.StringToHash("HitCounter");
        public static readonly int IsHit             = Animator.StringToHash("IsHit");
    };

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
            var animator = _ghostPresentationGameObjectSystem.GetGameObjectForEntity(EntityManager, entity)
                .GetComponent<Animator>();
            
            animator.SetInteger(Parameters.Random, Random.Range(0, 2)); // CHANGE DAMAGE TO ITS OWN RANDOM
            //if (playerState.isPunching)
            //{
            //    Debug.Log(reference.Animator.GetInteger(_parameters.Random));
            //}
            animator.SetBool(   Parameters.IsMoving,          playerState.ValueRO.IsMoving     );
            animator.SetBool(   Parameters.IsGrounded,        playerState.ValueRO.IsGrounded   );
            animator.SetBool(   Parameters.IsFalling,         playerState.ValueRO.IsFalling    );
            animator.SetBool(   Parameters.IsJumping,         playerState.ValueRO.IsJumping    );
            animator.SetBool(   Parameters.IsPunching,        playerState.ValueRO.IsPunching   );
            animator.SetBool(   Parameters.IsKicking,         playerState.ValueRO.IsKicking    );
            animator.SetBool(   Parameters.IsFallingFromHigh, playerState.ValueRO.IsFallingHigh);
            animator.SetBool(   Parameters.IsBlocking,        playerState.ValueRO.IsBlocking   );
            animator.SetBool(   Parameters.IsParrying,        playerState.ValueRO.IsParrying   );
            animator.SetBool(   Parameters.IsAnimLocked,      playerState.ValueRO.IsAnimLocked );
            animator.SetBool(   Parameters.IsHit,             playerState.ValueRO.IsHit        );
            animator.SetInteger(Parameters.HitCounter,        playerState.ValueRO.HitCounter   );

            if (playerState.ValueRO.IsHit)
            {
                playerState.ValueRW.IsHit = false;
            }
        }
        cmdBuffer.Playback(EntityManager);
        cmdBuffer.Dispose();
    }
}
