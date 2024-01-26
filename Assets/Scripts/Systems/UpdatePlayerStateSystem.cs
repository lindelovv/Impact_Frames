using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(PredictedSimulationSystemGroup))]
public partial struct UpdatePlayerStateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerComponentData>();
        state.RequireForUpdate<PlayerStateComponent>();
        state.RequireForUpdate<LocalTransform>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var (playerState, transform)
            in SystemAPI.Query<RefRW<PlayerStateComponent>, RefRO<LocalTransform>>()
                .WithAll<WorldRenderBounds>()
        ) {
            // Set isGrounded to true if the ray has collision close under player
            playerState.ValueRW.isGrounded = Physics.Raycast(
                transform.ValueRO.Position,
                -Vector3.up, 
                1.5f
            );
        }
    }
}
