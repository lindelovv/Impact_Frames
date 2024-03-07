using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup)),
 UpdateBefore(typeof(PerformActionSystem)),
 UpdateAfter(typeof(ActionTimerSystem))]
public partial struct InitActionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputData>(); //inputComponent för att komma åt inputscriptets inputs
        
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var player 
            in SystemAPI.Query<PlayerAspect>()
                .WithNone<Action>()
        ) {
            if (player.Input.RequestReset)
            {
                cmdBuffer.AddComponent<Respawn>(player.Self);
            }
            if (!player.IsAnimLocked)
            {
                // Check if you have Coyote or Groundbuffer
                if (player.Input.RequestJump && (player.IsGrounded || player.CayoteTimer > 0))
                {
                    cmdBuffer.AddComponent(player.Self, new Action
                    {
                        Name = ActionName.Jump,
                        Repeating = false,
                        State = ActionState.Startup,
                        StartTime = player.Data.jStartTime,
                        ActiveTime = player.Data.jActiveTime,
                        RecoverTime = player.Data.jRecoverTime,
                    });
                    player.IsJumping = true;
                    cmdBuffer.AddComponent<DoAction>(player.Self);
                    continue;
                }
                if (player.Input.RequestDash)
                {
                    cmdBuffer.AddComponent(player.Self, new Action
                    {
                        Name = ActionName.Dash,
                        Repeating = false,
                        State = ActionState.Startup,
                        StartTime   = player.Data.dStartTime,
                        ActiveTime  = player.Data.dActiveTime,
                        RecoverTime = player.Data.dRecoverTime,
                    });
                    cmdBuffer.AddComponent<DoAction>(player.Self);
                    continue;
                }
                if (player.Input.RequestBlock)
                {
                    if (player.BlockTimer < 0)
                    {
                        cmdBuffer.AddComponent(player.Self, new Action
                        {
                            Name = ActionName.Block,
                            Repeating = true,
                            State = ActionState.Startup,
                            ActiveTime  = 4,
                        });
                        player.IsBlocking = true;
                        cmdBuffer.AddComponent<DoAction>(player.Self);
                    }
                } else {
                    //Input button logik för att köra punch
                    if (player.Input.RequestPunch)
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
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.HeavyPunch,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.hpStartTime,
                                ActiveTime  = player.Data.hpActiveTime,
                                RecoverTime = player.Data.hpRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                        }
                        else
                        {
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.Punch,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.pStartTime,
                                ActiveTime  = player.Data.pActiveTime,
                                RecoverTime = player.Data.pRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                        }
                    }
                    //Input button logik för att köra kick
                    else if (player.Input.RequestKick)
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
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.HeavyKick,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.hkStartTime,
                                ActiveTime  = player.Data.hkActiveTime,
                                RecoverTime = player.Data.hkRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                        }
                        else
                        {
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.Kick,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.kStartTime,
                                ActiveTime  = player.Data.kActiveTime,
                                RecoverTime = player.Data.kRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                        }
                    }
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}