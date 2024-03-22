using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

//[UpdateInGroup(typeof(PredictedSimulationSystemGroup)), UpdateBefore(typeof(ActionSystem)), UpdateAfter(typeof(ActionTimerSystem))]
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
            var (player, action) 
            in SystemAPI.Query<PlayerAspect, Action>()
        ) {
            if (action.State != ActionState.None)
            {
                continue;
            }
            if (player.Input.RequestReset)
            {
                cmdBuffer.AddComponent<Respawn>(player.Self);
            }
            if (!player.IsAnimLocked)
            {
                // Check if you have Coyote or Groundbuffer
                if (player.Input.RequestJump && ((player.IsGrounded || player.CayoteTimer > 0) || player.IsOnBeat))
                {
                    cmdBuffer.SetComponent(player.Self, new Action
                    {
                        Name = ActionName.Jump,
                        Repeating = false,
                        State = ActionState.Startup,
                        StartTime = player.Data.jStartTime,
                        ActiveTime = player.Data.jActiveTime,
                        RecoverTime = player.Data.jRecoverTime,
                        DoAction = true,
                    });
                    player.IsJumping = true;
                    continue;
                }
                if (player.Input.RequestDash)
                {
                    cmdBuffer.SetComponent(player.Self, new Action
                    {
                        Name = ActionName.Dash,
                        Repeating = false,
                        State = ActionState.Startup,
                        StartTime   = player.Data.dStartTime,
                        ActiveTime  = player.Data.dActiveTime,
                        RecoverTime = player.Data.dRecoverTime,
                        DoAction = true,
                    });
                    continue;
                }
                if (player.Input.RequestParry)
                {
                    cmdBuffer.SetComponent(player.Self, new Action
                    {
                        Name = ActionName.Parry,
                        Repeating = false,
                        State = ActionState.Startup,
                        ActiveTime  = .2f,
                        DoAction = true,
                    });
                }
                if (player.Input.RequestBlock)
                {
                    if (player.BlockTimer < 0)
                    {
                        cmdBuffer.SetComponent(player.Self, new Action
                        {
                            Name = ActionName.Block,
                            Repeating = true,
                            State = ActionState.Startup,
                            ActiveTime  = 4,
                            DoAction = true,
                        });
                        player.IsBlocking = true;
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
                            cmdBuffer.SetComponent(player.Self, new Action
                            {
                                Name = ActionName.HeavyPunch,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.hpStartTime,
                                ActiveTime  = player.Data.hpActiveTime,
                                RecoverTime = player.Data.hpRecoverTime,
                                DoAction = true,
                            });
                        }
                        else
                        {
                            cmdBuffer.SetComponent(player.Self, new Action
                            {
                                Name = ActionName.Punch,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.pStartTime,
                                ActiveTime  = player.Data.pActiveTime,
                                RecoverTime = player.Data.pRecoverTime,
                                DoAction = true,
                            });
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
                            cmdBuffer.SetComponent(player.Self, new Action
                            {
                                Name = ActionName.HeavyKick,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.hkStartTime,
                                ActiveTime  = player.Data.hkActiveTime,
                                RecoverTime = player.Data.hkRecoverTime,
                                DoAction = true,
                            });
                        }
                        else
                        {
                            cmdBuffer.SetComponent(player.Self, new Action
                            {
                                Name = ActionName.Kick,
                                Repeating = false,
                                State = ActionState.Startup,
                                StartTime   = player.Data.kStartTime,
                                ActiveTime  = player.Data.kActiveTime,
                                RecoverTime = player.Data.kRecoverTime,
                                DoAction = true,
                            });
                        }
                    }
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}