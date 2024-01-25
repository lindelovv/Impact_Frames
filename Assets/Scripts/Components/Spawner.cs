using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Info")]
    public GameObject Prefab;

    private class Baker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
            
            var transform = authoring.transform;
            AddComponent(entity, new SpawnerComponent {
                Player = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                SpawnPoint = new LocalTransform {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Scale = 1.0f,
                },
            });
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public Entity Player;
    public LocalTransform SpawnPoint;
}

public struct PlayerSpawned : IComponentData {}

public readonly partial struct SpawnPlayerRequestAspect : IAspect
{
    public readonly Entity Self;
    private readonly RefRO<SpawnPlayerRequest> SpawnPlayerRequest;
    private readonly RefRO<ReceiveRpcCommandRequest> ReceiveRpcCommandRequest;
    public Entity SourceCOnnection => ReceiveRpcCommandRequest.ValueRO.SourceConnection;
}

//[DisallowMultipleComponent]
//public class Spawner : MonoBehaviour
//{
//    public GameObject Player;
//    public Entity Follow;
//    private EntityManager _manager;
//
//    private void Awake()
//    {
//        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
//    }
//
//    private void LateUpdate()
//    {
//        transform.position = _manager.GetComponentData<LocalTransform>(Follow).Position;
//    }
//
//    private class Baker : Baker<Spawner>
//    {
//        public override void Bake(Spawner authoring)
//        {
//            var entity = GetEntity(TransformUsageFlags.Dynamic);
//            var transform = authoring.transform;
//            AddComponent(entity, new SpawnerComponent
//            {
//                Player = GetEntity(authoring.Player, TransformUsageFlags.Dynamic),
//                SpawnPoint = new LocalTransform {
//                    Position = transform.position, 
//                    Rotation = transform.rotation, 
//                    Scale = 1.0f
//                },
//            });
//        }
//    }
//}
//
