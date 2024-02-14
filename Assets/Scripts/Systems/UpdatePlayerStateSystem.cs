using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup)), UpdateBefore(typeof(PlayerMovementSystem))]
public partial struct UpdatePlayerStateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerData>();
        state.RequireForUpdate<PlayerStateComponent>();
        state.RequireForUpdate<LocalTransform>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var player
            in SystemAPI.Query<PlayerAspect>()
                //.WithAll<WorldRenderBounds>()
        ) {
            // Set isGrounded to true if the ray has collision close under player
            player.IsGrounded = Physics.Raycast(
                player.Position,
                -Vector3.up,
                1.5f
            );
            if (player.IsGrounded)
            {
                player.IsJumping = player.Input.RequestJump;
                player.IsFalling = false;
            }
            else
            {
                player.IsFalling = player is { Velocity: { y: < 0f } };
            }
            
            var isMoving = (player.Input.RequestedMovement != float2.zero);
            
            // Character rotation
            if (isMoving.x)
            {
                player.IsMoving = true;
                player.IsFacingRight = (player.Input.RequestedMovement.x > 0.0f);
            }
            else if (!isMoving.y)
            {
                player.IsMoving = false;
            }
        }
    }
}
