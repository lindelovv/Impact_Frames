using Unity.Entities;
using Unity.NetCode;

[GhostComponent(
    PrefabType = GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct Action : IComponentData
{
    [GhostField] public ActionName Name;
    [GhostField] public ActionState State;
    [GhostField] public float StartTime;
    [GhostField] public float ActiveTime;
    [GhostField] public float RecoverTime;
    [GhostField] public bool Repeating;
}

[GhostComponent(
    PrefabType = GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct DoAction : IComponentData { }

public enum ActionState
{
    Startup,
    Active,
    Recovery,
    Finished,
}

public enum ActionName
{
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
