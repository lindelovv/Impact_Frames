
using Unity.Entities;

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
