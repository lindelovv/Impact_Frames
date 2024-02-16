    
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.NetCode;
    using Unity.Transforms;
    using UnityEngine;

    public struct PlayerSpawned : IComponentData { }

    public struct ConnectionOwner : IComponentData
    {
        public Entity Entity;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class SpawnPlayerSystem : SystemBase
    {
        private EntityQuery _newPlayers;

        protected override void OnCreate()
        {
            RequireForUpdate(_newPlayers);
            // Must wait for the spawner entity scene to be streamed in, most likely instantaneous in
            // this sample but good to be sure
            RequireForUpdate<SpawnerComponent>();
        }

        protected override void OnUpdate()
        {
            var prefab = SystemAPI.GetSingleton<SpawnerComponent>().Player;
            var spawnPoint = SystemAPI.GetSingleton<SpawnerComponent>().SpawnPoint;
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            Entities
                .WithName("SpawnPlayer")
                .WithStoreEntityQueryInField(ref _newPlayers)
                .WithNone<PlayerSpawned>()
                .ForEach((Entity connectionEntity, in NetworkStreamInGame req, in NetworkId networkId) =>
                {
                    Debug.Log($"Spawning player for connection {networkId.Value}");
                    var player = commandBuffer.Instantiate(prefab);

                    // The network ID owner must be set on the ghost owner component on the players
                    // this is used internally for example to set up the CommandTarget properly
                    commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

                    // This is to support thin client players and you don't normally need to do this when the
                    // auto command target feature is used (enabled on the ghost authoring component on the prefab).
                    // See the ThinClients sample for more details.
                    commandBuffer.SetComponent(connectionEntity, new CommandTarget(){targetEntity = player});

                    // Mark that this connection has had a player spawned for it so we won't process it again
                    commandBuffer.AddComponent<PlayerSpawned>(connectionEntity);

                    // Add the player to the linked entity group on the connection so it is destroyed
                    // automatically on disconnect (destroyed with connection entity destruction)
                    commandBuffer.AppendToBuffer(connectionEntity, new LinkedEntityGroup{Value = player});

                    commandBuffer.AddComponent(player, new ConnectionOwner { Entity = connectionEntity });
                    commandBuffer.SetComponent(player, spawnPoint);
                }
            ).Run();
            commandBuffer.Playback(EntityManager);
        }
    }
