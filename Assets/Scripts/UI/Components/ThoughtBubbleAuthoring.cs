using Unity.Entities;
using UnityEngine;

public class ThoughtBubbleAuthoring : MonoBehaviour
{
    class Baker : Baker<ThoughtBubbleAuthoring>
    {
        public override void Bake(ThoughtBubbleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ThoughtBubble>(entity);
        }
    }
}

public class ThoughtBubble : IComponentData {}
