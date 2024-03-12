using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(
    PrefabType = GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct ApplyImpact : IComponentData
{
    public float2 Amount;
}
