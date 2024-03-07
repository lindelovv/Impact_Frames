using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

public class VFXJumpImpactReference : MonoBehaviour
{
    public GameObject Prefab;

    private class Baker : Baker<VFXJumpImpactReference>
    {
        public override void Bake(VFXJumpImpactReference authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new VFXJumpImpactGameObject {
                Prefab = authoring.Prefab,
            });
        }
    }
}

public class VFXJumpImpactGameObject : IComponentData
{
    public GameObject Prefab;
}

public class VFXJumpImpactReferenceData : ICleanupComponentData
{
    public VisualEffect VFX;
}
