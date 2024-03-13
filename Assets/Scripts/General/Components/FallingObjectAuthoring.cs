using Unity.Entities;
using UnityEngine;

public class FallingObjectAuthoring : MonoBehaviour
{
    public float timeToRespawn = 4f;
    class Baker : Baker<FallingObjectAuthoring>
    {
        public override void Bake(FallingObjectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FallingObject {
                Active = false,
                ResetTimer = -1,
                TimeToRespawn = authoring.timeToRespawn,
            });
            var transform = authoring.transform;
            AddComponent(entity, new SpawnPoint {
                Position = transform.position,
                Rotation = transform.rotation,
            });
        }
    }
}

public struct FallingObject : IComponentData
{
    public bool Active;
    public float ResetTimer;
    public float TimeToRespawn;
}
