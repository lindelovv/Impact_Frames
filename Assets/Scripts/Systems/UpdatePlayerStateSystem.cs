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
            var isMoving = (inputComponentData.ValueRO.RequestedMovement != float2.zero);
            // Character rotation
            if (isMoving.x)
            {
                playerState.ValueRW.isMoving = true;
                playerState.ValueRW.isFacingRight = (inputComponentData.ValueRO.RequestedMovement.x > 0.0f);
            }
            else if (!isMoving.y)
            {
                playerState.ValueRW.isMoving = false;
            }
        }
    }
}
