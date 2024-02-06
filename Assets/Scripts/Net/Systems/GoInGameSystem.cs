using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation |
                   WorldSystemFilterFlags.ServerSimulation |
                   WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AutoConnect>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        FixedString32Bytes worldName = state.World.Name;

        foreach (var networkId in Query<NetworkIdAspect>().WithNone<NetworkStreamInGame>())
        {
            Debug.Log($"[{worldName}] Go in game connection {networkId.Id}");
            commandBuffer.AddComponent<NetworkStreamInGame>(networkId.Self);
        }

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
