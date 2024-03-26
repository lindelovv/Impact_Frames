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
        state.RequireForUpdate<Player>();
        state.RequireForUpdate<PlayerState>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var player
            in SystemAPI.Query<PlayerAspect>()
                .WithAll<Simulate>()
        ) {
            player.IsGrounded = (Physics.Raycast(
                    player.Position + new float3(0, 1, 0),
                    Vector3.down,
                    1.5f,
                    Physics.AllLayers
                ) || Physics.Raycast(
                    player.Position + new float3(0.6f, 1, 0),
                    Vector3.down,
                    1.5f,
                    Physics.AllLayers
                ) || Physics.Raycast(
                    player.Position + new float3(-0.6f, 1, 0),
                    Vector3.down,
                    1.5f,
                    Physics.AllLayers
            ));
            
            if (player is { IsBlocking: false, BlockTimer: >= 0 } ) { player.BlockTimer -= Time.deltaTime; }

            if (player.CayoteTimer > 0) { player.CayoteTimer -= Time.deltaTime; }  // Lower jump buffer timer

            if (player.IsGrounded)
            {
                player.IsFalling = false;
                player.CayoteTimer = player.CayoteTime;  // Set jump time buffer to initial time
            }
            else { player.IsFalling = player is { Velocity: { y: < 0f } }; }
            
            if (!player.IsAnimLocked)
            {
                var isMoving = (player.Input.RequestedMovement != float2.zero);

                // Character rotation
                if (isMoving.x)
                {
                    player.IsMoving = true;
                    player.IsFacingRight = (player.Input.RequestedMovement.x > 0.001f);
                }
                else
                {
                    player.IsMoving = false;
                }
            }
            else
            {
                player.IsMoving = false;
            }
        }
    }
}
