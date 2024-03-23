using System;
using Unity.Entities;
using Unity.NetCode;

//[GhostComponent(
//    PrefabType = GhostPrefabType.AllPredicted,
//    OwnerSendType = SendToOwnerType.SendToNonOwner
//)]
[GhostComponent(
    PrefabType = GhostPrefabType.All,
    SendTypeOptimization = GhostSendType.AllClients,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct PlayerId : IComponentData
{
    public Int16 Value;
}
