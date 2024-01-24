using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        
        state.RequireForUpdate<SpawnerComponent>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (id, entity) 
            in SystemAPI.Query<RefRO<NetworkId>>()
                .WithEntityAccess()
                .WithNone<NetworkStreamInGame>()
        ) {
            cmdBuffer.AddComponent<NetworkStreamInGame>(entity);
            var req = cmdBuffer.CreateEntity();
            cmdBuffer.AddComponent<GoInGameRPC>(req);
            cmdBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
        }
        cmdBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> m_NetworkId;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRPC>()
            .WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate<SpawnerComponent>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
        m_NetworkId = state.GetComponentLookup<NetworkId>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<SpawnerComponent>().Player;
        state.EntityManager.GetName(prefab, out FixedString64Bytes prefabName);
        
        var worldName = state.WorldUnmanaged.Name;
        
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        m_NetworkId.Update(ref state);

        foreach (
            var (reqSrc, reqEntity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                .WithAll<GoInGameRPC>()
                .WithEntityAccess()
        )
        {
            cmdBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkId = m_NetworkId[reqSrc.ValueRO.SourceConnection];
            
            var player = cmdBuffer.Instantiate(prefab);
            cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });
            Debug.Log($"[Networking] {worldName} connecting {networkId.Value}");
            cmdBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
            cmdBuffer.DestroyEntity(reqEntity);
        }
        cmdBuffer.Playback(state.EntityManager);
    }
}
