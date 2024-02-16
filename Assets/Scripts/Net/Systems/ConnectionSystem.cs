using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public struct GoInGameRPC : IRpcCommand {}

//[Preserve]
//public class Connection : ClientServerBootstrap
//{
//    public override bool Initialize(string defaultWorldName)
//    {
//        AutoConnectPort = 7979;                     // Enable auto connect
//        return base.Initialize(defaultWorldName);   // Use the regular bootstrap
//    }
//}

//-----------------------
// Client
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

//-----------------------
// Server
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> _networkId;
    private EntityQuery _newConnections;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerComponent>();
        state.RequireForUpdate(new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRPC>()
            .WithAll<ReceiveRpcCommandRequest>()
            .Build(ref state)
        );
        _networkId = state.GetComponentLookup<NetworkId>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<SpawnerComponent>();
        state.EntityManager.GetName(spawner.Player, out FixedString64Bytes prefabName);
        
        var worldName = state.WorldUnmanaged.Name;
        
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        _networkId.Update(ref state);

        foreach (
            var (reqSrc, entity)
            in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                .WithAll<GoInGameRPC>()
                .WithEntityAccess()
        ) {
            // Error: Received a ghost - Player - from the server which has a different hash on the client.
            // https://forum.unity.com/threads/received-a-ghost-from-the-server-which-has-a-different-hash-on-the-client.1542728/
            
            // https://docs.unity3d.com/Packages/com.unity.netcode@1.0/manual/ghost-spawning.html
            
            cmdBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkId = _networkId[reqSrc.ValueRO.SourceConnection];
            
            //var player = cmdBuffer.Instantiate(spawner.Player);
            //cmdBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });
            //cmdBuffer.SetComponent(player, spawner.SpawnPoint);

            Debug.Log($"[Networking] Player {networkId.Value} connecting to {worldName}.");
            
            //cmdBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });
            cmdBuffer.DestroyEntity(entity);
        }
        cmdBuffer.Playback(state.EntityManager);
    }
}

// reference

//// Place any established network connection in-game so ghost snapshot sync can start
//    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
//    public partial class GoInGameSystem : SystemBase
//    {
//        private EntityQuery m_NewConnections;
//
//        protected override void OnCreate()
//        {
//            RequireForUpdate<EnableGoInGame>();
//            RequireForUpdate(m_NewConnections);
//        }
//
//        protected override void OnUpdate()
//        {
//            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
//            FixedString32Bytes worldName = World.Name;
//            // Go in game as soon as we have a connection set up (connection network ID has been set)
//            Entities.WithName("NewConnectionsGoInGame").WithStoreEntityQueryInField(ref m_NewConnections).WithNone<NetworkStreamInGame>().ForEach(
//                (Entity ent, in NetworkId id) =>
//                {
//                    UnityEngine.Debug.Log($"[{worldName}] Go in game connection {id.Value}");
//                    commandBuffer.AddComponent<NetworkStreamInGame>(ent);
//                }).Run();
//            commandBuffer.Playback(EntityManager);
//        }
//    }
