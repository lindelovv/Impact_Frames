using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

// Experiment if this help ease of use
//public struct ActionTimer
//{
//    private float3 Time;
//    public float Startup { get => Time.x; set => Time.x = value; }
//    public float Active  { get => Time.y; set => Time.y = value; }
//    public float Recover { get => Time.z; set => Time.z = value; }
//}

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
