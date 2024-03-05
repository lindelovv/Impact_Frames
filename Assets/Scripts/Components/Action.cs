using Unity.Entities;
using Unity.NetCode;

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
    Parry,
}

public struct Action : IComponentData
{
    [GhostField] public ActionName Name;
    [GhostField] public ActionState State;
    [GhostField] public float StartTime;
    [GhostField] public float ActiveTime;
    [GhostField] public float RecoverTime;
}

public struct DoAction : IComponentData {}
