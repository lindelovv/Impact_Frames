using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(
    PrefabType           = GhostPrefabType.AllPredicted,
    SendTypeOptimization = GhostSendType.AllClients,
    OwnerSendType        = SendToOwnerType.SendToNonOwner)]
public struct ApplyImpact : IComponentData
{
    [GhostField(Quantization = 0)] public float2 Amount;
}
