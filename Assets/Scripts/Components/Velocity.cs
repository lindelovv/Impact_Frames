
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class Velocity : MonoBehaviour
{
    private float3 CurrentVelocity;
    private float TerminalVelocity;
    private float AirResistance;
    private float StaticFriction;
    private float KineticFriction;
    private float3 Gravity;

    private class Baker : Baker<Velocity>
    {
        public override void Bake(Velocity authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new VelocityComponent {
                CurrentVelocity = authoring.CurrentVelocity,
                TerminalVelocity = authoring.TerminalVelocity,
                AirResistance = authoring.AirResistance,
                StaticFriction = authoring.StaticFriction,
                KineticFriction = authoring.KineticFriction,
                Gravity = authoring.Gravity,
            });
        }
    }
}

public struct VelocityComponent : IComponentData
{
    [GhostField] public float3 CurrentVelocity;
    [GhostField] public float TerminalVelocity;
    [GhostField] public float AirResistance;
    [GhostField] public float StaticFriction;
    [GhostField] public float KineticFriction;
    [GhostField] public float3 Gravity;
}
