﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using Random = UnityEngine.Random;

[BurstCompile]
//[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct ActionSystem : ISystem
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
            in SystemAPI.Query<PlayerAspect, RefRW<Action>>()
        ) {
            switch (action.ValueRO.State)
            {
                case ActionState.Startup:
                {
                    player.IsAnimLocked = true;
                    action.ValueRW.DoAction = false;
                    break;
                }
                case ActionState.Finished:
                {
                    player.IsAnimLocked = false;
                    //action.ValueRW.DoAction = false;
                    break;
                }
            }
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var playerPosition = player.Position;
            var playerHasHit = player.HasHit;
            switch (action.ValueRO.Name)
            {
                //______________________________________________________________________________________________
                case ActionName.Jump:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsJumping = true; break; }
                        case ActionState.Active:
                        {
                            // find what causes this to play multiple times with IsOnBeat
                            player.IsAnimLocked = true;
                            player.Velocity = new float3(player.Velocity.x, player.JumpHeight, 0);
                            player.CayoteTimer = -1;
                            break;
                        }
                        case ActionState.Recovery: { player.IsJumping = false; break; }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.Dash:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsDashing = true; break; }
                        case ActionState.Active:
                        {
                            // note: maybe add some Y velocity
                            player.Velocity = new float3((player.IsFacingRight ? player.DashSpeed : -player.DashSpeed), player.Velocity.y, 0);
                            break;
                        }
                        case ActionState.Recovery: { break; }
                        case ActionState.Finished: { player.IsDashing = false; break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.Punch:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsPunching = true; break; }
                        case ActionState.Active:
                        {
                            player.Random = Random.Range(0, 2);
                            Punch(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.Pushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld, 
                                out playerHasHit
                            );
                            player.HitTime = Time.time;
                            break;
                        }
                        case ActionState.Recovery:
                        {
                            player.IsPunching = false;
                            player.HasHit = false;
                            break;
                        }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.HeavyPunch:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsPunching = true; break; }
                        case ActionState.Active:
                        {
                            PunchHeavy(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.Pushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld,
                                out playerHasHit
                            );
                            player.HitTime = Time.time;
                            break;
                        }
                        case ActionState.Recovery:
                        {
                            player.IsPunching = false;
                            player.HasHit = false;
                            break;
                        }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.Kick:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsKicking = true; break; }
                        case ActionState.Active:
                        {
                            player.Random = Random.Range(0, 2);
                            Kick(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.Pushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld,
                                out playerHasHit
                            );
                            player.HitTime = Time.time;
                            break;
                        }
                        case ActionState.Recovery:
                        {
                            player.IsKicking = false;
                            player.HasHit = false;
                            break;
                        }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.HeavyKick:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsKicking = true; break; }
                        case ActionState.Active:
                        {
                            KickHeavy(
                                player.Self,
                                player.IsFacingRight ? 1 : -1,
                                ref playerPosition,
                                player.Pushback,
                                ref cmdBuffer,
                                ref state,
                                ref collisionWorld,
                                out playerHasHit
                            );
                            player.HitTime = Time.time;
                            break;
                        }
                        case ActionState.Recovery:
                        {
                            player.IsKicking = false;
                            player.HasHit = false;
                            break;
                        }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.Block:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsBlocking = true; break; }
                        case ActionState.Active:
                        {
                            if (!player.Input.RequestBlock)
                            {
                                player.IsBlocking = false;
                                player.IsAnimLocked = false;
                                action.ValueRW.DoAction = false;
                                action.ValueRW.ActiveTime = 0;
                                action.ValueRW.State = ActionState.None;
                                action.ValueRW.Name = ActionName.None;
                            }
                            break;
                        }
                        case ActionState.Recovery:
                        {
                            player.BlockTimer = player.BlockCooldown;
                            player.IsBlocking = false;
                            break;
                        }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.Parry:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsParrying = true; break; }
                        case ActionState.Active:
                        {
                            break;
                        }
                        case ActionState.Recovery: { player.IsParrying = false; break; }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
                //______________________________________________________________________________________________
                case ActionName.HitStun:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup:  { player.IsHit = true; break; }
                        case ActionState.Active:
                        {
                            player.Random = Random.Range(0, 2);
                            action.ValueRW.DoAction = false;
                            break;
                        }
                        case ActionState.Recovery: { player.IsHit = false; break; }
                        case ActionState.Finished: { break; }
                    }
                    break;
                }
            }
            if (!action.ValueRO.Repeating) { action.ValueRW.DoAction = false; }

            if (playerHasHit)
            {
                action.ValueRW.State++;
                action.ValueRW.RecoverTime += action.ValueRO.ActiveTime;
                action.ValueRW.ActiveTime = 0;
                action.ValueRW.DoAction = true;
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
    
    [BurstCompile]
    public static void Punch(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld, out bool hasHit)
    {
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Apply Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<Health>(entity)
        ) {
            cmdBuffer.AddComponent(entity, new TakeDamage {
                Amount = 1,
            });
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }
    
    [BurstCompile]
    public static void PunchHeavy(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld, out bool hasHit)
    {
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<Health>(entity)
        ) {
            cmdBuffer.AddComponent(entity, new TakeDamage {
                Amount = 1,
            });
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }

    [BurstCompile]
    public static void Kick(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld, out bool hasHit)
    {
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<Health>(entity)
        ) {
            cmdBuffer.AddComponent(entity, new TakeDamage {
                Amount = 1,
            });
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }
    
    [BurstCompile]
    public static void KickHeavy(in Entity self, in int forward, ref float3 position, in float2 pushback, ref EntityCommandBuffer cmdBuffer, ref SystemState state, ref CollisionWorld collisionWorld, out bool hasHit)
    {
        Entity entity;
        CastCollider(ref position, forward, ref collisionWorld, out entity, out hasHit);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && entity != self 
            && entityManager.HasComponent<Health>(entity)
        ) {
            cmdBuffer.AddComponent(entity, new TakeDamage {
                Amount = 1,
            });
            
            cmdBuffer.AddComponent(entity, new ApplyImpact {
                Amount = new float2(forward * pushback),
            });
        }
    }
    
    [BurstCompile]
    private static unsafe void CastCollider(ref float3 position, int forward, ref CollisionWorld collisionWorld, out Entity entity, out bool hasHit)
    {
        var center = position + (new float3(forward * 1.5f, 1f, 0));
        var size = new float3(1, 1, 1);

        var collider = Unity.Physics.BoxCollider.Create(
            new BoxGeometry {
                BevelRadius = 0f,
                Center = float3.zero,
                Orientation = quaternion.identity,
                Size = size
            },
            filter: new CollisionFilter {
                BelongsTo = 1,
                CollidesWith = 1,
                GroupIndex = 0,
            }
        );
        hasHit = collisionWorld.CastCollider(
            new ColliderCastInput {
                Collider = (Unity.Physics.Collider*)collider.GetUnsafePtr(),
                Start = center,
                End = center,
            },
            out var hit
        );
        entity = hit.Entity;
        DrawBox(ref center, ref center, ref size);
        Debug.Log($"Collider hit: {hasHit}");
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
