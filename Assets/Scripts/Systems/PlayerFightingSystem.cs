using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public partial struct PlayerFightingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponentData>(); //inputComponent för att komma åt inputscriptets inputs
            state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
                var fightJob = new FightJob { DeltaTimea = SystemAPI.Time.DeltaTime };
        state.Dependency = fightJob.ScheduleParallel(state.Dependency);
    }

}

[BurstCompile]
public partial struct FightJob : IJobEntity
{
    public float DeltaTimea;
    public void Execute(in InputComponentData input)  // Execute tillhör IJobEntity Interfacet
    {
        
    }


}
