using System;
using Unity.Entities;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    class Baker : Baker<Dummy>
    {
        public override void Bake(Dummy authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<DummyTag>(entity);
        }
    }
}

public struct DummyTag : IComponentData { }
