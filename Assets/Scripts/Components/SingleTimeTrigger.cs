using Unity.Entities;
using UnityEngine;

public class SingleTimeTrigger : MonoBehaviour
{
    class Baker : Baker<SingleTimeTrigger>
    {
        public override void Bake(SingleTimeTrigger authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<SingleTimeTriggerTag>(entity);
        }
    }
}

public struct SingleTimeTriggerTag : IComponentData { }