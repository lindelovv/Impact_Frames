using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup)), UpdateBefore(typeof(InitActionSystem))]
public partial struct ActionTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Action>();
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (action, entity)
            in SystemAPI.Query<RefRW<Action>>()
                .WithEntityAccess()
        ) {
            switch (action.ValueRO.State)
            { 
                //_________________________
                case ActionState.None: { continue; }
                //_________________________
                case ActionState.Startup: {
                    
                    if (action.ValueRO.StartTime < 0) {
                        action.ValueRW.State++;
                        action.ValueRW.DoAction = true;
                    }
                    action.ValueRW.StartTime -= Time.deltaTime;
                    
                    break;
                }
                //_________________________
                case ActionState.Active: {
                    
                    if (action.ValueRO.ActiveTime < 0) {
                        action.ValueRW.State++;
                        action.ValueRW.DoAction = true;
                    }
                    action.ValueRW.ActiveTime -= Time.deltaTime;
                    
                    break;
                }
                //_________________________
                case ActionState.Recovery: {
                    
                    if (action.ValueRO.RecoverTime < 0) {
                        action.ValueRW.State++;
                        action.ValueRW.DoAction = true;
                    }
                    action.ValueRW.RecoverTime -= Time.deltaTime;
                    
                    break;
                }
                //_________________________
                case ActionState.Finished: {
                    action.ValueRW.State = ActionState.None;
                    action.ValueRW.Name = ActionName.None;
                    action.ValueRW.DoAction = false;
                    
                    break;
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
