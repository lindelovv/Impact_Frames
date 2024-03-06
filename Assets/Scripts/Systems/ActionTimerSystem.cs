using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateAfter(typeof(UpdatePlayerStateSystem))]
public partial struct ActionTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Action>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (action, entity)
            in SystemAPI.Query<RefRW<Action>>().WithEntityAccess()
        )
        {
            //Debug.Log($"Action: {action.ValueRO.Name}, State: {action.ValueRO.State}");
            switch (action.ValueRO.State)
            { 
                //_________________________
                case ActionState.Startup: {
                    //Debug.Log($"Startup {action.ValueRO.StartTime}");
                    action.ValueRW.StartTime -= Time.deltaTime;
                    if (action.ValueRO.StartTime < 0) {
                        action.ValueRW.State++;
                        cmdBuffer.AddComponent<DoAction>(entity);
                    }
                    
                    break;
                }
                //_________________________
                case ActionState.Active: {
                    //Debug.Log($"Active {action.ValueRO.ActiveTime}");
                    action.ValueRW.ActiveTime -= Time.deltaTime;
                    if (action.ValueRO.ActiveTime < 0) {
                        action.ValueRW.State++;
                        cmdBuffer.AddComponent<DoAction>(entity);
                    }
                    
                    break;
                }
                //_________________________
                case ActionState.Recovery: {
                    //Debug.Log($"Recover {action.ValueRO.RecoverTime}");
                    action.ValueRW.RecoverTime -= Time.deltaTime;
                    if (action.ValueRO.RecoverTime < 0) {
                        action.ValueRW.State++;
                        cmdBuffer.AddComponent<DoAction>(entity);
                    }
                    
                    break;
                }
                //_________________________
                case ActionState.Finished: {
                    cmdBuffer.RemoveComponent<Action>(entity);
                    break;
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
