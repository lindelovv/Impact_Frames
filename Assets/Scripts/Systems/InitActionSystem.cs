using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct InitActionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponentData>(); //inputComponent för att komma åt inputscriptets inputs
        
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var player 
            in SystemAPI.Query<PlayerAspect>()
        ) {
            if (player.Input.RequestReset)
            {
                cmdBuffer.AddComponent<Respawn>(player.Self);
            }
            //Debug.Log($"AnimLock: {player.IsAnimLocked}");
            if (!player.IsAnimLocked)
            {
                if (player.Input.RequestJump)
                {
                    cmdBuffer.AddComponent(player.Self, new Action
                    {
                        Name = ActionName.Jump,
                        StartTime   = player.Data.jStartTime,
                        ActiveTime  = player.Data.jActiveTime,
                        RecoverTime = player.Data.jRecoverTime,
                    });
                    cmdBuffer.AddComponent<DoAction>(player.Self);
                    continue;
                }
                if (player.Input.RequestDash)
                {
                    cmdBuffer.AddComponent(player.Self, new Action
                    {
                        Name = ActionName.Dash,
                        StartTime   = player.Data.dStartTime,
                        ActiveTime  = player.Data.dActiveTime,
                        RecoverTime = player.Data.dRecoverTime,
                    });
                    cmdBuffer.AddComponent<DoAction>(player.Self);
                    continue;
                }
                if (player.IsBlocking)
                {
                    //Block(player);
                    continue;
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
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.HeavyPunch,
                                StartTime   = player.Data.hpStartTime,
                                ActiveTime  = player.Data.hpActiveTime,
                                RecoverTime = player.Data.hpRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
                        }
                        else
                        {
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.Punch,
                                StartTime   = player.Data.pStartTime,
                                ActiveTime  = player.Data.pActiveTime,
                                RecoverTime = player.Data.pRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
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
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.HeavyKick,
                                StartTime   = player.Data.hkStartTime,
                                ActiveTime  = player.Data.hkActiveTime,
                                RecoverTime = player.Data.hkRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
                        }
                        else
                        {
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.Kick,
                                StartTime   = player.Data.kStartTime,
                                ActiveTime  = player.Data.kActiveTime,
                                RecoverTime = player.Data.kRecoverTime,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
                        }
                    }
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}