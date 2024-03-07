using System;
using Unity.Entities;
using UnityEngine;

public class TagAuthoring : MonoBehaviour
{
    public string Name;
    
    class Baker : Baker<TagAuthoring>
    {
        public override void Bake(TagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Tag {
                NameHash = Animator.StringToHash(authoring.Name),
            });
        }
    }
}

public struct Tag : IComponentData
{
    public Int32 NameHash;
}
