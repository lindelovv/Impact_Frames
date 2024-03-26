using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    [Header("Health")] 
    public float Current;
    public float Max;

    private class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Health    { Value = authoring.Current });
            AddComponent(entity, new MaxHealth { Value = authoring.Max     });
            AddBuffer<DamageBuffer>(entity);
            AddBuffer<DamageThisTick>(entity);
        }
    }
}

[PredictAll]
public struct Health : IComponentData
{
    [GhostField(Quantization=10)] public float Value;
}

[PredictAll]
public struct MaxHealth : IComponentData
{
    [GhostField(Quantization=10)] public float Value;
}

public struct TakeDamage : IComponentData
{
    [GhostField(Quantization=10)] public float Amount;
}

[PredictAll]
public struct DamageBuffer : IBufferElementData
{
    public float Value;
}

[PredictAll]
public struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public float Value;
}

public readonly partial struct HealthAspect : IAspect
{
    private readonly RefRW<Health> _health;
    private readonly RefRW<MaxHealth> _maxHealth;
    
    public float Current { get => _health.ValueRO.Value;    set => _health.ValueRW.Value = value;    }
    public float Max     { get => _maxHealth.ValueRO.Value; set => _maxHealth.ValueRW.Value = value; }
}
