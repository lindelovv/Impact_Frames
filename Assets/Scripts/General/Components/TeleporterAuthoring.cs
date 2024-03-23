using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TeleporterAuthoring : MonoBehaviour
{
    public float3 nextPosition;
    class Baker : Baker<TeleporterAuthoring>
    {
        public override void Bake(TeleporterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Teleporter {
                NextPosition = authoring.nextPosition,
            });
            AddComponent(entity, new SpawnPoint {
                Position = authoring.nextPosition,
            });
        }
    }
}

public struct Teleporter : IComponentData
{
    public float3 NextPosition;
}
