using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public float Lifetime;
    
    private class Baker : Baker<Hitbox>
    {
        public override void Bake(Hitbox authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HitboxData {
                Lifetime = authoring.Lifetime,
            });
            AddComponent(entity, new PhysicsCollider {
                
            });
        }
    }
}

public struct HitboxData : IComponentData
{
    [GhostField] public float Lifetime;
}
