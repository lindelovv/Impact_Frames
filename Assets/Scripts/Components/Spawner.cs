using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Player;
    public float3 SpawnPoint;
    public float RotationDegrees;

    private class Baker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.None), new SpawnerComponent
            {
                Player = GetEntity(authoring.Player, TransformUsageFlags.Dynamic),
                SpawnPoint = new LocalTransform { Position = authoring.SpawnPoint, Rotation = quaternion.RotateY(authoring.RotationDegrees), Scale = 1.0f },
            });
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public Entity Player;
    public LocalTransform SpawnPoint;
}
