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
            if (Physics.Raycast(
                    player.Position,
                    -Vector3.up,
                    1.5f
            )) {
                player.State &= State.IsGrounded;
            }
            else 
            {
                player.State |= State.IsGrounded;
            }
            
            if ((player.State & State.IsGrounded) != 0)
            {
                if (player.Input.RequestJump)
                {
                    player.State |= State.IsJumping;
                }
                else
                {
                    player.State &= State.IsFalling;
                }
            }
            else
            {
                if (player.Velocity.y < 0f)
                {
                    player.State |= State.IsFalling;
                };
            }
            
            var isMoving = (player.Input.RequestedMovement != float2.zero);
            
            // Character rotation
            if (isMoving.x)
            {
                player.State |= State.IsMoving;
                if (player.Input.RequestedMovement.x > 0.0f && (player.State & State.IsFacingRight) != 0)
                {
                    player.State |= State.IsFacingRight;
                }
                else if ((player.State & State.IsFacingRight) == 0)
                {
                    player.State &= State.IsFacingRight;
                }
                    
            }
            else if (!isMoving.y)
            {
                player.State &= State.IsMoving;
            }

            if (player.Input.RequestPunch)
            {
                player.State |= State.IsPunching;
            }
            else /* if(timer/animation) */
            {
                player.State &= State.IsPunching;
            }
        }
    }
}
