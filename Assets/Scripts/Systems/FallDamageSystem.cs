using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Falling : IComponentData
{
    public float StartTime;
}

//______________________________________________________________________________________________________________________
public partial struct StartFallingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var player
            in SystemAPI.Query<PlayerAspect>()
                .WithNone<Falling>()
        ) {
            if (player.IsFalling)
            {
                cmdBuffer.AddComponent(player.Self, new Falling { StartTime = Time.time });
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
            var player
            in SystemAPI.Query<PlayerAspect>()
                .WithAll<Falling>()
        ) {
            if (player.IsJumping || player.IsDashing)
            {
                cmdBuffer.RemoveComponent<Falling>(player.Self);
                continue;
            }
            if (player.IsGrounded)
            {
                var damage = Time.time - state.EntityManager.GetComponentData<Falling>(player.Self).StartTime;
                if (damage < 0.7f)
                {
                    cmdBuffer.RemoveComponent<Falling>(player.Self);
                    continue;
                }
                cmdBuffer.AddComponent(player.Self, new TakeDamage
                {
                    Amount = math.clamp(damage, 0, 10),
                });
                cmdBuffer.RemoveComponent<Falling>(player.Self);
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
