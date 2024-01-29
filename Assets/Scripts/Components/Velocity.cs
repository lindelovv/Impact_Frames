
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class Velocity : MonoBehaviour
{
    private float3 CurrentVelocity;

    private class Baker : Baker<Velocity>
    {
        public override void Bake(Velocity authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new VelocityComponent {
                CurrentVelocity = authoring.CurrentVelocity,
            });
        }
    }
}

public struct VelocityComponent : IComponentData
{
    [GhostField] public float3 CurrentVelocity;
}