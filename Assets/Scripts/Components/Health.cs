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
                CurrentHealth = authoring.CurrentHealth,
                MaxHealth = authoring.MaxHealth,
            });
        }
    }
}

public struct HealthComponent : IComponentData
{
    [GhostField] public float CurrentHealth;
    [GhostField] public float MaxHealth;
}
