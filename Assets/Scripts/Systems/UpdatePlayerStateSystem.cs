using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Aspects;
using Unity.Physics.Extensions;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using Collider = UnityEngine.Collider;

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
        )
        {
            playerState.ValueRW.isGrounded = Physics.Raycast(
                transform.ValueRO.Position,
                -Vector3.up, 
                1.5f
            );
        }
    }
}
