using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

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
            //Debug.Log($"AnimLock: {player.IsAnimLocked}");
            if (!player.IsAnimLocked)
            {
                if (player.Input.RequestJump)
                {
                    Debug.Log("jump");
                    cmdBuffer.AddComponent(player.Self, new Action
                    {
                        Name = ActionName.Jump,
                        StartTime = 0,
                        ActiveTime = 0,
                        RecoverTime = .2f,
                    });
                    cmdBuffer.AddComponent<DoAction>(player.Self);
                    continue;
                }
                if (player.Input.RequestDash)
                {
                    cmdBuffer.AddComponent(player.Self, new Action
                    {
                        Name = ActionName.Dash,
                        StartTime = 0,
                        ActiveTime = 0,
                        RecoverTime = .4f,
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
                                StartTime = 0,
                                ActiveTime = .2f,
                                RecoverTime = .4f,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
                        }
                        else
                        {
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.Punch,
                                StartTime = 0,
                                ActiveTime = .2f,
                                RecoverTime = .4f,
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
                                StartTime = 0,
                                ActiveTime = .2f,
                                RecoverTime = .4f,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
                        }
                        else
                        {
                            cmdBuffer.AddComponent(player.Self, new Action
                            {
                                Name = ActionName.Kick,
                                StartTime = 0,
                                ActiveTime = .2f,
                                RecoverTime = .4f,
                            });
                            cmdBuffer.AddComponent<DoAction>(player.Self);
                            continue;
                        }
                    }
                }
            }
        }
    }
}