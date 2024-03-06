using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;

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
            switch (action.State)
            {
                case ActionState.Startup:
                {
                    player.IsAnimLocked = true;
                    cmdBuffer.RemoveComponent<DoAction>(player.Self);
                    continue;
                }
                case ActionState.Finished:
                {
                    player.IsAnimLocked = false;
                    cmdBuffer.RemoveComponent<DoAction>(player.Self);
                    continue;
                }
                case ActionState.Active:
                {
                    var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
                    switch (action.Name)
                    {
                        //______________________________________________________________________________________________
                        case ActionName.Jump:
                        {
                            player.Velocity += new float3(
                                0,
                                (player is { IsGrounded: true } //or { IsOnBeat: true } // find what causes this to play
                                    ? player.JumpHeight                                 // multiple times with IsOnBeat
                                    : 0.0f),
                                0
                            );
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
                            PlayerFighting.Punch(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.HeavyPunch:
                        {
                            PlayerFighting.PunchHeavy(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.Kick:
                        {
                            PlayerFighting.Kick(player, cmdBuffer, ref state, ref collisionWorld);
                            break;
                        }
                        //______________________________________________________________________________________________
                        case ActionName.HeavyKick:
                        {
                            PlayerFighting.KickHeavy(player, cmdBuffer, ref state, ref collisionWorld);
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
}
