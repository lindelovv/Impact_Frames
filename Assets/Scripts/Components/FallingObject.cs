using Unity.Entities;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    class Baker : Baker<FallingObject>
    {
        public override void Bake(FallingObject authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FallingObjectData>(entity);
        }
    }
}

public struct FallingObjectData : IComponentData {}
