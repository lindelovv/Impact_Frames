using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Player, Input, LocalTransform, ApplyImpact>()
            .Build(state.EntityManager)
        );
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (player, impact)
            in SystemAPI.Query<PlayerAspect, ApplyImpact>()
        ) {
            // Increase gravity if falling
            {
                if (player.Data.OverrideGravity)
                {
                    player.GravityFactor = player.Data.CustomGravity;
                }
                else
                {
                    player.GravityFactor = player is { IsGrounded: false, Velocity: { y: <= 2.0f } }
                        ? player.FallGravity
                        : 1;
                }

                var velocityY = player.Velocity.y;
                if (player.IsJumping && velocityY > 2f)
                {
                    velocityY *= player.JumpDecay;
                }
            }

            if (!player.IsAnimLocked)
            {
                // Calculate & Add Horizontal Movement
                {
                    if (!player.IsDashing)
                    {
                        if (player.Input.RequestedMovement.x == 0) // If not moving, change velocity towards 0
                        {
                            player.Velocity = new float3(Util.moveTowards(
                                player.Velocity.x,
                                player.Position.x,
                                player.Damping * SystemAPI.Time.DeltaTime
                            ), player.Velocity.y, 0);
                        }
                        else if (player is { IsAnimLocked: false, IsBlocking: false })
                        {
                            player.Velocity = new float3(Util.moveTowards( // Else towards max speed
                                player.Velocity.x,
                                player.Input.RequestedMovement.x * player.MaxSpeed,
                                player.Acceleration * SystemAPI.Time.DeltaTime
                            ), player.Velocity.y, 0);
                            player.IsFacingRight = (player.Input.RequestedMovement.x > 0.0f);
                        }
                        else
                        {
                            player.IsFacingRight = (player.Input.RequestedMovement.x > 0.0f);
                        }
                    }
                }

                // Rotation
                {
                    player.Rotation = quaternion.EulerXYZ(
                        0f,
                        (player.IsFacingRight ? 90f : -89.8f),
                        0f
                    );
                }
            }

            // Apply impact
            {
                player.Velocity += new float3(impact.Amount, 0);
                cmdBuffer.SetComponent(player.Self, new ApplyImpact { Amount = 0, });
            }
            Debug.DrawLine(player.Position, player.Position + (player.Velocity / 2), Color.cyan, 1);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
