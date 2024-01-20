using UnityEngine;
using System.Collections;

using System.Diagnostics;
using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem  // Detta hette tidigare MoveSystem, menades det Moves, som i alla saker en player kan g�ra? Isf kan vi flytta tillbaka fightingsystem hit
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponentData>()  //inputComponent f�r att komma �t inputscriptets inputs
            .WithAll<LocalTransform>(); // f�r att kunna r�ra saker, transform
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var moveJob = new MoveJob { DeltaTime = SystemAPI.Time.DeltaTime };  //Movejob har valt deltatime som variablar vi vill ha d�r och Job �r f�r multithreading
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);

        var fightJob = new FightJob { DeltaTimea = SystemAPI.Time.DeltaTime };
        state.Dependency = fightJob.ScheduleParallel(state.Dependency);

        
    }
}

[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(in InputComponentData input, ref LocalTransform transform)  // Execute tillh�r IJobEntity Interfacet
    {
        // Movement
        var move = new float2(input.MoveValue.x, 0) * input.MovementSpeed;
        // Jump
        move += new float2(0, input.JumpValue) * input.JumpStrength;
        
        // Move
        transform.Position += new float3(move.x, move.y, 0) * DeltaTime;
    }

}
