using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;


[RequireComponent(typeof(PhysicsShapeAuthoring))]
/**
 * Put this Script on the obj in SubLevel that want to use AudioTriggerSystem.
 * Collision Response should be set to: Raises Trigger Events
 * Belongs to: Set param
 * Collides with: Set param
 */
public class AudioTriggerAuthoring : MonoBehaviour
{
    class Baker : Baker<AudioTriggerAuthoring>
    {
        public override void Bake(AudioTriggerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<AudioTrigger>(entity);
        }
    }
}

public class AudioTrigger : IComponentData { }
