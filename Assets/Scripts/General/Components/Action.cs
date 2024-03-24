using Unity.Entities;
using Unity.NetCode;

[GhostComponent(
    PrefabType           = GhostPrefabType.All,
    SendTypeOptimization = GhostSendType.AllClients,
    OwnerSendType        = SendToOwnerType.SendToNonOwner)]
public struct Action : IComponentData
{
    [GhostField] public ActionName Name;
    [GhostField] public ActionState State;
    
    [GhostField] public bool DoAction;
    [GhostField] public bool Repeating;
    
    [GhostField(Quantization = 0)] public float StartTime;
    [GhostField(Quantization = 0)] public float ActiveTime;
    [GhostField(Quantization = 0)] public float RecoverTime;
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
