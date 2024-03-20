using Unity.Entities;
using Unity.NetCode;

//[GhostComponent(
//    PrefabType = GhostPrefabType.AllPredicted,
//    OwnerSendType = SendToOwnerType.SendToNonOwner
//)]
[GhostComponent(
    PrefabType=GhostPrefabType.All,
    SendTypeOptimization=GhostSendType.AllClients,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct Action : IComponentData
{
    [GhostField] public ActionName Name;
    [GhostField] public ActionState State;
    
    [GhostField] public bool DoAction;
    [GhostField] public bool Repeating;
    
    [GhostField] public float StartTime;
    [GhostField] public float ActiveTime;
    [GhostField] public float RecoverTime;
}

public enum ActionState
{
    None,
    Startup,
    Active,
    Recovery,
    Finished,
}

public enum ActionName
{
    None,
    Jump,
    Dash,
    Punch,
    HeavyPunch,
    Kick,
    HeavyKick,
    Block,
    Parry,
    HitStun,
}
