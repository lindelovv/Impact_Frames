using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using Random = UnityEngine.Random;

public struct AttackPropterties
{
    public float Damage;
    public float2 Pushback;
    public CollisionFilter Filter;
    public float3 Size;
}

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
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
            var (player, action, playerId)
            in SystemAPI.Query<PlayerAspect, RefRW<Action>, PlayerId>()
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
            var forward = player.IsFacingRight ? 1 : -1;
            var id = playerId.Value;
            var otherId = id == 1 ? 2 : 1;
            
            switch (action.ValueRO.Name)
            {
                //______________________________________________________________________________________________
                case ActionName.Jump:
                {
                    switch (action.ValueRO.State)
                    {
                        case ActionState.Startup: 
                        {
                            player.IsJumping = true;
                            player.CayoteTimer = -1;
                            player.IsAnimLocked = true;
                            break;
                        }
                        case ActionState.Active:
                        {
                            // find what causes this to play multiple times with IsOnBeat
                            player.Velocity = new float3(player.Velocity.x, player.JumpHeight, 0);
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
                            playerHasHit = Attack(
                                player.Self,
                                playerPosition + new float3(forward * 2, 1, 0),
                                new AttackPropterties {
                                    Damage   = 1,
                                    Pushback = new float2(forward * 4,1),
                                    Size     = new float3(1, 1, 1),
                                    Filter   = new CollisionFilter {
                                        BelongsTo    = (uint)id,
                                        CollidesWith = (uint)otherId,
                                        GroupIndex   = 0,
                                    },
                                },
                                ref collisionWorld,
                                ref state,
                                ref cmdBuffer
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
                            playerHasHit = Attack(
                                player.Self,
                                playerPosition + new float3(forward * 2, 1, 0),
                                new AttackPropterties {
                                    Damage   = 2,
                                    Pushback = new float2(forward * 8,1),
                                    Size     = new float3(1, 1, 1),
                                    Filter   = new CollisionFilter {
                                        BelongsTo    = (uint)id,
                                        CollidesWith = (uint)otherId,
                                        GroupIndex   = 0,
                                    },
                                },
                                ref collisionWorld,
                                ref state,
                                ref cmdBuffer
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
                            playerHasHit = Attack(
                                player.Self,
                                playerPosition + new float3(forward * 2, 1, 0),
                                new AttackPropterties {
                                    Damage   = 1,
                                    Pushback = new float2(forward * 4,1),
                                    Size     = new float3(1, 1, 1),
                                    Filter   = new CollisionFilter {
                                        BelongsTo    = (uint)id,
                                        CollidesWith = (uint)otherId,
                                        GroupIndex   = 0,
                                    },
                                },
                                ref collisionWorld,
                                ref state, 
                                ref cmdBuffer
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
                            playerHasHit = Attack(
                                player.Self,
                                playerPosition + new float3(forward * 2, 1, 0),
                                new AttackPropterties {
                                    Damage   = 2,
                                    Pushback = new float2(forward * 8,1),
                                    Size     = new float3(1, 1, 1),
                                    Filter   = new CollisionFilter {
                                        BelongsTo    = (uint)id,
                                        CollidesWith = (uint)otherId,
                                        GroupIndex   = 0,
                                    },
                                },
                                ref collisionWorld,
                                ref state,
                                ref cmdBuffer
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

            // TODO: why does this break after first hit ?
            //player.HasHit = playerHasHit;
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
    public bool Attack(Entity self, in float3 position, in AttackPropterties attackProperties, ref CollisionWorld collisionWorld, ref SystemState state, ref EntityCommandBuffer cmd)
    {
        var attack = attackProperties;
        var colliderCenter = position;
        if (CastCollider(ref colliderCenter, ref attack.Size, ref attack.Filter, ref collisionWorld, out var entity) 
            && entity != self
            && state.EntityManager.HasComponent<Health>(entity)
        ) {
            var action = state.EntityManager.GetComponentData<Action>(entity);
            if (action is { Name: ActionName.Block, State: ActionState.Active })
            {
                cmd.AddComponent(entity, new TakeDamage {
                    Amount = attack.Damage / 4,
                });
                cmd.SetComponent(entity, new ApplyImpact {
                    Amount = attack.Pushback / 4,
                });
            }
            else 
            {
                cmd.AddComponent(entity, new TakeDamage {
                    Amount = attack.Damage,
                });
                cmd.SetComponent(entity, new ApplyImpact {
                    Amount = attack.Pushback,
                });
            }
            return true;
        }
        return false;
    }
    
    [BurstCompile]
    private static unsafe bool CastCollider(ref float3 colliderCenter, ref float3 colliderSize, ref CollisionFilter filter, ref CollisionWorld collisionWorld, out Entity entity)
    {
        var collider = Unity.Physics.BoxCollider.Create(
            new BoxGeometry {
                BevelRadius = 0f,
                Center = float3.zero,
                Orientation = quaternion.identity,
                Size = colliderSize
            },
            filter
        );
        var hasHit = collisionWorld.CastCollider(
            new ColliderCastInput {
                Collider = (Unity.Physics.Collider*)collider.GetUnsafePtr(),
                Start = colliderCenter,
                End = colliderCenter,
            },
            out var hit
        );
        entity = hit.Entity;
        DrawBox(ref colliderCenter, ref colliderCenter, ref colliderSize);
        //Debug.Log($"Collider hit: {hasHit}");
        return hasHit;
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
