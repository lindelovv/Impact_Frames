using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            Debug.Log($"Action: {action.Name}, State: {action.State}");
            if (action.State == ActionState.Finished )
            {
                player.IsAnimLocked = false;
                cmdBuffer.RemoveComponent<DoAction>(player.Self);
                continue;
            } 
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            switch (action.Name)
            {
                //______________________________________________________________________________________________________
                case ActionName.Jump:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        {
                             player.Velocity = new float3(
                                 player.Velocity.x,
                                 (player is { IsGrounded: true } //or { IsOnBeat: true }
                                     ? player is { IsFalling: true }
                                         ? -player.Velocity.y + player.JumpHeight
                                         : player.JumpHeight
                                     : 0.0f),
                                 0
                             );
                             Debug.Log($"{player.Velocity}");
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
                //______________________________________________________________________________________________________
                case ActionName.Dash:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        {
                            player.Velocity = new float3(player.IsFacingRight ? 20f : -20f, player.Velocity.y, 0);
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
                //______________________________________________________________________________________________________
                case ActionName.Punch:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        {
                            PlayerFighting.Punch(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
                //______________________________________________________________________________________________________
                case ActionName.HeavyPunch:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        { 
                            PlayerFighting.PunchHeavy(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
                //______________________________________________________________________________________________________
                case ActionName.Kick:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        { 
                            PlayerFighting.Kick(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
                //______________________________________________________________________________________________________
                case ActionName.HeavyKick:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        { 
                            PlayerFighting.KickHeavy(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
                //______________________________________________________________________________________________________
                case ActionName.Parry:
                {
                    switch (action.State)
                    {
                        //________________________
                        case ActionState.Startup:
                        {
                            player.IsAnimLocked = true;
                            break;
                        }
                        //________________________
                        case ActionState.Active:
                        {
                            break;
                        }
                        //________________________
                        case ActionState.Recovery:
                        {
                            break;
                        }
                    }
                    break;
                }
            }
            cmdBuffer.RemoveComponent<DoAction>(player.Self);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
    
    [BurstCompile]
    private unsafe (ColliderCastHit,bool) CastCollider(PlayerAspect player, int forward)
    {
        ColliderCastHit hit = new ColliderCastHit();
        bool hasHit = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
            .CollisionWorld.CastCollider(new ColliderCastInput
            {
                Collider = (Unity.Physics.Collider*) Unity.Physics.BoxCollider.Create(new BoxGeometry
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
}
