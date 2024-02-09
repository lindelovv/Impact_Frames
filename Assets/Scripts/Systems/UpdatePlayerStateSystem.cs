using Unity.Burst;
using Unity.Entities;
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
            var (playerState, transform, inputComponentData)
            in SystemAPI.Query<RefRW<PlayerStateComponent>, RefRO<LocalTransform>, RefRO<InputComponentData>>()
                //.WithAll<WorldRenderBounds>()
        ) {
            // Set isGrounded to true if the ray has collision close under player
            playerState.ValueRW.isGrounded = Physics.Raycast(
                transform.ValueRO.Position,
                -Vector3.up, 
                1.5f
            );
            
            // Character rotation
            if (inputComponentData.ValueRO.RequestedMovement.x != 0.0f)
            {
                playerState.ValueRW.isFacingRight = (inputComponentData.ValueRO.RequestedMovement.x > 0.0f);
            }
        }
    }
}
