using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

public class VFXReference : MonoBehaviour
{
    public String Name;
    public GameObject BlockPrefab;

    private class Baker : Baker<VFXReference>
    {
        public override void Bake(VFXReference authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new VFXGameObject {
                NameHash = Animator.StringToHash(authoring.Name),
                Prefab = authoring.BlockPrefab,
            });
        }
    }
}

public class VFXGameObject : IComponentData
{
    public Int32 NameHash;
    public GameObject Prefab;
}

public class VFXReferenceData : ICleanupComponentData
{
    public Int32 NameHash;
    public VisualEffect VFX;
}
