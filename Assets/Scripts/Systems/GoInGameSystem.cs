using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation |
                   WorldSystemFilterFlags.ServerSimulation |
                   WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameSystem : ISystem
{
    //public void OnCreate(ref SystemState state)
    //{
    //    state.RequireForUpdate<AutoConnectComponent>();
    //}

    //public void OnUpdate(ref SystemState state)
    //{
    //    var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
    //    FixedString32Bytes worldName = state.World.Name;

    //    foreach (var networkId
    //             in SystemAPI.Query<NetworkIdAspect>()
    //                 .WithNone<NetworkStreamInGame>()
    //    ) {
    //        Debug.Log($"[{worldName}] Go in game connection {networkId.Id}");
    //        cmdBuffer.AddComponent<NetworkStreamInGame>(networkId.Self);
    //    }

    //    cmdBuffer.Playback(state.EntityManager);
    //    cmdBuffer.Dispose();
    //}
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
    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<SpawnerComponent>().Player;
        var spawnPoint = SystemAPI.GetSingleton<SpawnerComponent>().SpawnPoint;
        state.EntityManager.GetName(prefab, out FixedString64Bytes prefabName);

        var worldName = state.WorldUnmanaged.Name;

        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        m_NetworkId.Update(ref state);

        foreach (var (reqSrc, reqEntity)
                 in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                     .WithAll<GoInGameRPC>()
                     .WithEntityAccess()
        ) {
            cmdBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkId = m_NetworkId[reqSrc.ValueRO.SourceConnection];

            var player = cmdBuffer.Instantiate(prefab);
            cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });
            cmdBuffer.SetComponent(player, spawnPoint);

            Debug.Log($"[Networking] {worldName} connecting {networkId.Value}");

            cmdBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
            cmdBuffer.DestroyEntity(reqEntity);
        }

        cmdBuffer.Playback(state.EntityManager);
    }
}

//-----------------------
// Server
//[BurstCompile]
//[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
//public partial struct GoInGameServerSystem : ISystem
//{
//    private ComponentLookup<NetworkId> m_NetworkId;
//    
//    [BurstCompile]
//    public void OnCreate(ref SystemState state)
//    {
//        var builder = new EntityQueryBuilder(Allocator.Temp)
//            .WithAll<GoInGameRPC>()
//            .WithAll<ReceiveRpcCommandRequest>();
//        state.RequireForUpdate<SpawnerComponent>();
//        state.RequireForUpdate(state.GetEntityQuery(builder));
//        m_NetworkId = state.GetComponentLookup<NetworkId>(true);
//    }
//
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        var prefab = SystemAPI.GetSingleton<SpawnerComponent>().Player;
//        var spawnPoint = SystemAPI.GetSingleton<SpawnerComponent>().SpawnPoint;
//        state.EntityManager.GetName(prefab, out FixedString64Bytes prefabName);
//        
//        var worldName = state.WorldUnmanaged.Name;
//        
//        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
//        m_NetworkId.Update(ref state);
//
//        foreach (var (reqSrc, reqEntity)
//                 in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
//                     .WithAll<GoInGameRPC>()
//                     .WithEntityAccess()
//        ) {
//            cmdBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
//            var networkId = m_NetworkId[reqSrc.ValueRO.SourceConnection];
//            
//            var player = cmdBuffer.Instantiate(prefab);
//            cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });
//            cmdBuffer.SetComponent(player, spawnPoint);
//
//            Debug.Log($"[Networking] {worldName} connecting {networkId.Value}");
//            
//            cmdBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
//            cmdBuffer.DestroyEntity(reqEntity);
//        }
//        cmdBuffer.Playback(state.EntityManager);
//    }
//    
//    [BurstCompile] public void OnDestroy(ref SystemState state) {}
//}
