using Unity.Entities;
using UnityEngine;

public class Attack : MonoBehaviour
{
    class Baker : Baker<Attack>
    {
        public override void Bake(Attack authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<AttackData>(entity);
        }
    }
}

public struct AttackData : IComponentData
{
    public float StartupTime;
    public float ActiveTime;
    public float RecoveryTime;
}
