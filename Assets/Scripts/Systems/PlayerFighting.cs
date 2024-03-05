using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using BoxCollider = Unity.Physics.BoxCollider;
using Collider = Unity.Physics.Collider;

[BurstCompile]
public struct PlayerFighting
{
    [BurstCompile]
    public static void Punch(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward, collisionWorld);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && hit.Entity != player.Self 
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);
            
            cmdBuffer.AddComponent(hit.Entity, new ApplyImpact {
                Amount = new float2(forward * player.PunchPushback),
            });
        }
    }
    
    [BurstCompile]
    public static void PunchHeavy(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward, collisionWorld);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (hasHit
            && hit.Entity != player.Self
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        )
        {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);

            cmdBuffer.AddComponent(hit.Entity, new ApplyImpact
            {
                Amount = new float2((forward * player.PunchPushback) * 2),
            });
        }
    }

    [BurstCompile]
    public static void Kick(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward, collisionWorld);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && hit.Entity != player.Self 
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);
            
            cmdBuffer.AddComponent(hit.Entity, new ApplyImpact {
                Amount = new float2(forward * player.PunchPushback),
            });
        }
    }
    
    [BurstCompile]
    public static void KickHeavy(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward, collisionWorld);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && hit.Entity != player.Self 
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);
            
            cmdBuffer.AddComponent(hit.Entity, new ApplyImpact {
                Amount = new float2(forward * player.PunchPushback),
            });
        }
    }
    
    [BurstCompile]
    private static unsafe (ColliderCastHit,bool) CastCollider(PlayerAspect player, int forward, CollisionWorld collisionWorld)
    {
        ColliderCastHit hit = new ColliderCastHit();
        bool hasHit = collisionWorld.CastCollider(new ColliderCastInput
            {
                Collider = (Collider*)BoxCollider.Create(new BoxGeometry
                {
                    BevelRadius = 0f,
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = new float3(1, 1, 1)
                }, filter: new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0,
                }).GetUnsafePtr(),
                Start = player.Position + (forward * new float3(0.9f, 0, 0)),
                End = player.Position + (forward * new float3(1, 0, 0)),
            },
            out hit);
        return (hit, hasHit);
    }

    [BurstCompile]
    private void Block(PlayerAspect player)
    {
        //Debug.Log("Block");
    }
}
