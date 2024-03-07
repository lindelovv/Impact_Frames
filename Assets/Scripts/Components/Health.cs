using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float CurrentHealth;
    public float MaxHealth;

    private class Baker : Baker<Health>
    {
        public override void Bake(Health authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new HealthComponent {
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
public struct HealthComponent : IComponentData
{
    [GhostField(Quantization=10)] public float Current;
    [GhostField(Quantization=10)] public float Max;
}
