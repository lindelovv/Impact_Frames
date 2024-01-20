using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]

public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerComponent>();
        state.RequireForUpdate<InputComponent>();
        state.RequireForUpdate<NetworkId>();
    }

    public void OnDestroy(ref SystemState state)
    {}

    public void OnUpdate(ref SystemState state)
    {
       

        bool left = UnityEngine.Input.GetKey(KeyCode.A);
        bool right = UnityEngine.Input.GetKey(KeyCode.D);
        bool jump = 

        foreach (var input in SystemAPI.Query<RefRW<InputComponent>>()
                     .WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW = default;
            if (left)  { input.ValueRW.X -= 1; }
            if (right) { input.ValueRW.X += 1; }
        }
    }
}
