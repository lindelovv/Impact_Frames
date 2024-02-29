using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

public class VFXBlockReference : MonoBehaviour
{
    public GameObject Prefab;

    private class Baker : Baker<VFXBlockReference>
    {
        public override void Bake(VFXBlockReference authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new VFXBlockGameObject {
                Prefab = authoring.Prefab,
            });
        }
    }
}

public class VFXBlockGameObject : IComponentData
{
    public GameObject Prefab;
}

public class VFXBlockReferenceData : ICleanupComponentData
{
    public VisualEffect VFX;
}
