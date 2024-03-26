using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

//______________________________________________________________________________________________________________________
public partial struct StartFallingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (playerState, entity)
            in SystemAPI.Query<PlayerState>()
                .WithNone<Falling>()
                .WithEntityAccess()
        ) {
            if (playerState.IsFalling)
            {
                cmdBuffer.AddComponent(entity, new Falling { StartTime = Time.time });
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

//______________________________________________________________________________________________________________________
public partial struct FallDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (playerState, falling, entity)
            in SystemAPI.Query<RefRW<PlayerState>, Falling>()
                .WithAll<Falling>()
                .WithEntityAccess()
        ) {
            if (playerState.ValueRO.IsJumping || playerState.ValueRO.IsDashing)
            {
                cmdBuffer.RemoveComponent<Falling>(entity);
                continue;
            }
            
            playerState.ValueRW.IsFallingHigh = Time.time - falling.StartTime > .7f;
            
            if (playerState.ValueRO.IsGrounded)
            {
                var damage = Time.time - falling.StartTime;
                if (damage < 0.7f)
                {
                    cmdBuffer.RemoveComponent<Falling>(entity);
                    continue;
                }
                playerState.ValueRW.IsFallingHigh = true;
                cmdBuffer.AddComponent(entity, new TakeDamage {
                    Amount = math.clamp((damage * 2), 0, 10),
                });
                cmdBuffer.RemoveComponent<Falling>(entity);
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

//______________________________________________________________________________________________________________________
[PredictAll]
public struct Falling : IComponentData
{
    [GhostField(Quantization = 0)] public float StartTime;
}
