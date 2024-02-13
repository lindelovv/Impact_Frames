using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Player;

    private class Baker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            var transform = authoring.transform;
            AddComponent(GetEntity(TransformUsageFlags.None), new SpawnerComponent
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

public struct SpawnerComponent : IComponentData
{
    public Entity Player;
    public LocalTransform SpawnPoint;
}
