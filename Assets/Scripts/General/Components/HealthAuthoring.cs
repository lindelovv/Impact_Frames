using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public float CurrentHealth;
    public float MaxHealth;

    private class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Health {
                Current = authoring.CurrentHealth,
                Max = authoring.MaxHealth,
            });
        }
    }
}

[GhostComponent(
    PrefabType = GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct Health : IComponentData
{
    [GhostField(Quantization=10)] public float Current;
    [GhostField(Quantization=10)] public float Max;
}
