using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Player;

    private class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var transform = authoring.transform;
            AddComponent(GetEntity(TransformUsageFlags.None), new Spawner
            {
                Player = GetEntity(authoring.Player, TransformUsageFlags.Dynamic),
                SpawnPoint = new LocalTransform {
                    Position = transform.position, 
                    Rotation = transform.rotation, 
                    Scale = 1.0f
                },
            });
        }
    }
}

public struct Spawner : IComponentData
{
    [GhostField] public Entity Player;
    [GhostField] public LocalTransform SpawnPoint;
}
