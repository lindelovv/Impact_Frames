using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct MoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponent>()
            .WithAll<LocalTransform>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var moveJob = new MoveJob { DeltaTime = SystemAPI.Time.DeltaTime };
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(in InputComponent input, ref LocalTransform transform)
    {
        // Movement
        var move = new float2(input.MoveValue.x, 0) * input.MovementSpeed;
        // Jump
        move += new float2(0, input.JumpValue) * input.JumpStrength;
        
        // Move
        transform.Position += new float3(move.x, move.y, 0) * DeltaTime;
    }
}
