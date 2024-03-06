using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(ActionTimerSystem))]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
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
                             player.Velocity += new float3(
                                 0,
                                 (player is { IsGrounded: true } //or { IsOnBeat: true }
                                     ? player.JumpHeight
                                     : 0.0f),
                                 0
                             );
                             cmdBuffer.RemoveComponent<DoAction>(player.Self);
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
                            //PlayerFighting.Punch(player, cmdBuffer, ref state, ref collisionWorld);
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
                            //PlayerFighting.PunchHeavy(player, cmdBuffer, ref state, ref collisionWorld);
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
}
