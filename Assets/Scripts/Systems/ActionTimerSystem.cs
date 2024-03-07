using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup)),
 UpdateBefore(typeof(InitActionSystem))]
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
            in SystemAPI.Query<RefRW<Action>>()
                .WithEntityAccess()
        ) {
            //Debug.Log($"Action: {action.ValueRO.Name}, State: {action.ValueRO.State}");
            switch (action.ValueRO.State)
            { 
                //_________________________
                case ActionState.Startup: {
                    //Debug.Log($"Startup {action.ValueRO.StartTime}");
                    
                    if (action.ValueRO.StartTime < 0) {
                        action.ValueRW.State++;
                        cmdBuffer.AddComponent<DoAction>(entity);
                    }
                    action.ValueRW.StartTime -= Time.deltaTime;
                    
                    break;
                }
                //_________________________
                case ActionState.Active: {
                    //Debug.Log($"Active {action.ValueRO.ActiveTime}");
                    
                    if (action.ValueRO.ActiveTime < 0) {
                        action.ValueRW.State++;
                        cmdBuffer.AddComponent<DoAction>(entity);
                    }
                    action.ValueRW.ActiveTime -= Time.deltaTime;
                    
                    break;
                }
                //_________________________
                case ActionState.Recovery: {
                    //Debug.Log($"Recover {action.ValueRO.RecoverTime}");
                    
                    if (action.ValueRO.RecoverTime < 0) {
                        action.ValueRW.State++;
                        cmdBuffer.AddComponent<DoAction>(entity);
                    }
                    action.ValueRW.RecoverTime -= Time.deltaTime;
                    
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
