using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

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
        var fightJob = new FightJob { DeltaTime = SystemAPI.Time.DeltaTime };
        state.Dependency = fightJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct FightJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(in InputComponentData input)  // Execute tillhör IJobEntity Interfacet
    {
        
    }
}
