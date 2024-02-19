using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

public partial struct HitboxSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HitboxData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var hitbox
            in SystemAPI.Query<HitboxData>()
        ) {
            
        }
    }
}

[BurstCompile]
public struct HitboxJob : IJobParallelFor
{
    [BurstCompile]
    public void Execute(int index)
    {
        throw new System.NotImplementedException();
    }
}