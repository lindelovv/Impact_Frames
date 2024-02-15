    using Unity.Entities;
    using UnityEngine;

    public struct EnableSpawnPlayerComponent : IComponentData { }

    [DisallowMultipleComponent]
    public class EnableSpawnPlayer : MonoBehaviour
    {
        class Baker : Baker<EnableSpawnPlayer>
        {
            public override void Bake(EnableSpawnPlayer authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnableSpawnPlayerComponent>(entity);
            }
        }
    }
