using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(ActionTimerSystem))]
public partial struct PerformActionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (player, action)
            in SystemAPI.Query<PlayerAspect, Action>()
                .WithAll<DoAction>()
        ) {
            switch (action.State)
            {
                case ActionState.Startup: // If StatupTime is 0 this will be skipped
                {
                    player.IsAnimLocked = true;
                    cmdBuffer.RemoveComponent<DoAction>(player.Self);
                    continue;
                }
                case ActionState.Finished:
                {
                    if (action.Name == ActionName.Jump)
                    {
                        player.IsJumping = false;
                    }
                    player.IsAnimLocked = false;
                    cmdBuffer.RemoveComponent<DoAction>(player.Self);
                    continue;
                }
                case ActionState.Active:
                {
                    var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
                    var playerPosition = player.Position;
                    switch (action.Name)
                    {
                        //______________________________________________________________________________________________
                        case ActionName.Jump:
                        {
                            // find what causes this to play multiple times with IsOnBeat
                            player.IsAnimLocked = true;
                            player.Velocity = new float3(player.Velocity.x, player.JumpHeight, 0);
                            player.CayoteTimer = -1;
                            cmdBuffer.RemoveComponent<DoAction>(player.Self);
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.Dash:
                        {
                            player.Velocity = new float3(player.IsFacingRight ? 20f : -20f, player.Velocity.y, 0);
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.Punch:
                        {
                            PunchHeavy(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.PunchPushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld
                            );
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.HeavyPunch:
                        {
                            PunchHeavy(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.PunchPushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld
                            );
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.Kick:
                        {
                            Kick(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.PunchPushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld
                            );
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.HeavyKick:
                        {
                            KickHeavy(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.PunchPushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld
                            );
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.Parry:
                        {
                            // TODO
                            break;
                        }
                    }
                    cmdBuffer.RemoveComponent<DoAction>(player.Self);
                    break;
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
    
    [BurstCompile]
    public static void Punch(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        bool hasHit;
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<HealthComponent>(entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(entity);
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }
    
    [BurstCompile]
    public static void PunchHeavy(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        bool hasHit;
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<HealthComponent>(entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(entity);
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }

    [BurstCompile]
    public static void Kick(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        bool hasHit;
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<HealthComponent>(entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(entity);
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }
    
    [BurstCompile]
    public static void KickHeavy(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld)
    {
        bool hasHit;
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<HealthComponent>(entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(entity);
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }
    
    [BurstCompile]
    private static unsafe void CastCollider(ref float3 position, int forward, ref CollisionWorld collisionWorld, out Entity entity, out bool hasHit)
    {
        var start = position + (new float3(forward * 0.9f, 1f, 0));
        var end   = position + (new float3(forward * 1,    1f, 0));
        var size = new float3(1, 1, 1);
        
        ColliderCastHit hit = new ColliderCastHit();
        hasHit = collisionWorld.CastCollider(new ColliderCastInput
            {
                Collider = (Unity.Physics.Collider*)Unity.Physics.BoxCollider.Create(new BoxGeometry
                {
                    BevelRadius = 0f,
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = size
                }, filter: new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0,
                }).GetUnsafePtr(),
                Start = start,
                End = end,
            },
            out hit);
        entity = hit.Entity;
        DrawBox(ref start, ref end, ref size);
    }

    [BurstCompile]
    private void Block(PlayerAspect player)
    {
        
        //Debug.Log("Block");
    }

    [BurstCompile]
    private static void DrawBox(ref float3 start, ref float3 end, ref float3 size)
    {
        //var t = new Color(0, 0, 0, 0); // TODO: for fun change color over time :o
        Debug.DrawLine(start + new float3(-(size.x / 2), (size.y / 2), 0), start + new float3(-(size.x / 2), -(size.y / 2), 0), Color.magenta, .2f);
        Debug.DrawLine(start + new float3((size.x / 2), -(size.y / 2), 0), start + new float3((size.x / 2), (size.y / 2), 0),   Color.magenta, .2f);
        
        Debug.DrawLine(start + new float3(-(size.x / 2), (size.y / 2), 0), start + new float3((size.x / 2), (size.y / 2), 0),   Color.magenta, .2f);
        Debug.DrawLine(start + new float3(-(size.x / 2), -(size.y / 2), 0), start + new float3((size.x / 2), -(size.y / 2), 0), Color.magenta, .2f);
        
        Debug.DrawLine(end + new float3(-(size.x / 2), (size.y / 2), 0), end + new float3(-(size.x / 2), -(size.y / 2), 0), Color.cyan, .2f);
        Debug.DrawLine(end + new float3((size.x / 2), -(size.y / 2), 0), end + new float3((size.x / 2), (size.y / 2), 0),   Color.cyan, .2f);
        
        Debug.DrawLine(end + new float3(-(size.x / 2), (size.y / 2), 0), end + new float3((size.x / 2), (size.y / 2), 0),   Color.cyan, .2f);
        Debug.DrawLine(end + new float3(-(size.x / 2), -(size.y / 2), 0), end + new float3((size.x / 2), -(size.y / 2), 0), Color.cyan, .2f);
    }
}
