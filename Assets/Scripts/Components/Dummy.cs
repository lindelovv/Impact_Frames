using System;
using Unity.Entities;
using Unity.NetCode;
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

[GhostComponent(
    PrefabType = GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct DummyTag : IComponentData { }
