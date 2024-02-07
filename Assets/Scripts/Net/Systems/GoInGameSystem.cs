using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation |
                   WorldSystemFilterFlags.ClientSimulation |
                   WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameSystem : ISystem
{
    private ComponentLookup<NetworkId> m_NetworkId;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AutoConnect>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        FixedString32Bytes worldName = state.World.Name;

        foreach (   
            var connection
            in SystemAPI.Query<ConnectionAspect>()
                    .WithNone<NetworkStreamInGame>()
        ) {
            Debug.Log($"[{worldName}] Go in game connection {connection.Id}");
            cmdBuffer.AddComponent<NetworkStreamInGame>(connection.Self);
        }

        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

