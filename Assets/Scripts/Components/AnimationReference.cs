using Unity.Entities;
using UnityEngine;

public class AnimationReference : MonoBehaviour
{
    public GameObject Prefab;

    private class Baker : Baker<AnimationReference>
    {
        public override void Bake(AnimationReference authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new AnimatedGameObject {
                Prefab = authoring.Prefab,
            });
        }
    }
}

public class AnimatedGameObject : IComponentData
{
    public GameObject Prefab;
}

public class AnimationReferenceData : ICleanupComponentData
{
    public Animator Animator;
}
