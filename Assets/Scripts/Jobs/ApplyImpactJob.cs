using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public partial struct ApplyImpactJob : IJobEntity
{
    public float2 Impact;
    
    public void Execute(PhysicsVelocity velocity)
    {
        velocity.Linear += new float3(Impact, 0);
    }
}
