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
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerFightingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponentData>(); //inputComponent f�r att komma �t inputscriptets inputs
        state.RequireForUpdate(state.GetEntityQuery(builder));
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var player in SystemAPI.Query<PlayerAspect>())
        {
            if (player.IsBlocking)
            {
                Block(player);
            } else {
                //Input button logik för att köra punch
                if (player.IsPunching)
                {
                    player.HitCounter = player.HitCounter == 4
                        ? 0                                    // if 4th hit set to 0;
                        : player.HitCounter + 1;               // else increment

                    if (Time.time - player.HitTime > player.MaxComboDelay)
                    {
                        player.HitCounter = 0;
                    }
                    player.HitTime = Time.time;
                    
                    if (player.HitCounter == 4)
                    {
                        PunchHeavy(player, cmdBuffer, ref state);
                    }
                    else
                    {
                        Punch(player, cmdBuffer, ref state);
                    }
                }
                //Input button logik för att köra kick
                else if (player.IsKicking)
                {
                    player.HitCounter = player.HitCounter == 4
                        ? 0                                    // if 4th hit set to 0;
                        : player.HitCounter + 1;               // else increment

                    if (Time.time - player.HitTime > player.MaxComboDelay)
                    {
                        player.HitCounter = 0;
                    }
                    player.HitTime = Time.time;
                    
                    if (player.HitCounter == 4)
                    {
                        KickHeavy(player, cmdBuffer, ref state);
                    }
                    else
                    {
                        Kick(player, cmdBuffer, ref state);
                    }
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
    
    [BurstCompile]
    private void Punch(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && hit.Entity != player.Self 
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);
            
            cmdBuffer.SetComponent(hit.Entity, new ApplyImpact {
                Amount = new float2(forward * player.PunchPushback),
            });
        }
    }
    
    [BurstCompile]
    private void PunchHeavy(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (hasHit
            && hit.Entity != player.Self
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        )
        {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);

            cmdBuffer.SetComponent(hit.Entity, new ApplyImpact
            {
                Amount = new float2((forward * player.PunchPushback) * 2),
            });
        }
    }

    [BurstCompile]
    private void Kick(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && hit.Entity != player.Self 
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);
            
            cmdBuffer.SetComponent(hit.Entity, new ApplyImpact {
                Amount = new float2(forward * player.PunchPushback),
            });
        }
    }
    
    [BurstCompile]
    private void KickHeavy(PlayerAspect player, EntityCommandBuffer cmdBuffer, ref SystemState state)
    {
        var forward = player.IsFacingRight ? 1 : -1;

        //Debug Draws Cross for Hitboxes
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);

        var (hit, hasHit) = CastCollider(player, forward);

        // Check Health and Appyl Damage
        var entityManager = state.EntityManager;
        if (   hasHit 
            && hit.Entity != player.Self 
            && entityManager.HasComponent<HealthComponent>(hit.Entity)
        ) {
            cmdBuffer.AddComponent<TakeDamage>(hit.Entity);
            
            cmdBuffer.SetComponent(hit.Entity, new ApplyImpact {
                Amount = new float2(forward * player.PunchPushback),
            });
        }
    }
    
    [BurstCompile]
    private unsafe (ColliderCastHit,bool) CastCollider(PlayerAspect player, int forward)
    {
        ColliderCastHit hit = new ColliderCastHit();
        bool hasHit = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
            .CollisionWorld.CastCollider(new ColliderCastInput
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
