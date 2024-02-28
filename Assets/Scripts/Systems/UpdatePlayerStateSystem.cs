using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
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
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var serverTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        
        foreach (
            var player
            in SystemAPI.Query<PlayerAspect>()
                .WithAll<Simulate>()
        ) {
            player.IsGrounded = (Physics.Raycast(
                player.Position,
                Vector3.down,
                1.5f
            ) || Physics.Raycast(
                player.Position + new float3(0.6f, 0, 0),
                Vector3.down,
                1.5f
            ) || Physics.Raycast(
                player.Position + new float3(-0.6f, 0, 0),
                Vector3.down,
                1.5f
            ));
            
            if (player.IsGrounded)
            {
                player.IsJumping = player.Input.RequestJump;
                player.IsFalling = false;
            }
            else
            {
                player.IsFalling = player is { Velocity: { y: < 0f } };
                //if (Physics.Raycast(
                //            player.Position,
                //            Vector3.down,
                //            1.5f
                //)) {
                //    // Is falling from high
                //    // toggle on here and turn off somewhere else after landing?
                //}
            }
            player.IsDashing = player.Input.RequestDash;
            if (!player.IsDashing)
            {
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

                player.IsBlocking = player.Input.RequestBlockParry;
                if (!player.IsBlocking)
                {
                    player.IsPunching = player.Input.RequestPunch;
                    player.IsKicking = player.Input.RequestKick;
                }
            }
        }
    }
}
